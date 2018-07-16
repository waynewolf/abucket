#define LOG_TAG "bottom_line_artifact"
#include <cutils/memory.h>
#include <utils/Log.h>
#include <android/native_window.h>

#include <binder/IPCThreadState.h>
#include <binder/ProcessState.h>
#include <binder/IServiceManager.h>

#include <gui/Surface.h>
#include <gui/SurfaceComposerClient.h>

#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>

using namespace android;

#define MGD_DATA 0

const char* eglStatusStr() {
    EGLint error = eglGetError();

    switch (error) {
        case EGL_SUCCESS: return "EGL_SUCCESS";
        case EGL_BAD_DISPLAY: return "EGL_BAD_DISPLAY";
        case EGL_NOT_INITIALIZED: return "EGL_NOT_INITIALIZED";
        case EGL_BAD_ACCESS: return "EGL_BAD_ACCESS";
        case EGL_BAD_ALLOC: return "EGL_BAD_ALLOC";
        case EGL_BAD_ATTRIBUTE: return "EGL_BAD_ATTRIBUTE";
        case EGL_BAD_CONFIG: return "EGL_BAD_CONFIG";
        case EGL_BAD_CONTEXT: return "EGL_BAD_CONTEXT";
        case EGL_BAD_CURRENT_SURFACE: return "EGL_BAD_CURRENT_SURFACE";
        case EGL_BAD_MATCH: return "EGL_BAD_MATCH";
        case EGL_BAD_NATIVE_PIXMAP: return "EGL_BAD_NATIVE_PIXMAP";
        case EGL_BAD_NATIVE_WINDOW: return "EGL_BAD_NATIVE_WINDOW";
        case EGL_BAD_PARAMETER: return "EGL_BAD_PARAMETER";
        case EGL_BAD_SURFACE: return "EGL_BAD_SURFACE";
        default: return "UNKNOWN_EGL_ERROR";
    }
}

static void printGLString(const char *name, GLenum s) {
    const char *v = (const char *) glGetString(s);
    printf("GL %s = %s\n", name, v);
}

static void checkGlError(const char* op) {
    for (GLint error = glGetError(); error; error
            = glGetError()) {
        printf("after %s() glError (0x%x)\n", op, error);
    }
}

#if 1
//original shader
static const char gVertexShader[] = 
    "#version 100\n"
    "attribute vec4 position;\n"
    "attribute float vtxAlpha;\n"
    "uniform mat4 projection;\n"
    "uniform mat4 transform;\n"
    "uniform mat4 roundRectInvTransform;\n"
    "uniform mediump vec4 roundRectInnerRectLTWH;\n"
    "uniform mediump float roundRectRadius;\n"
    "varying float alpha;\n"
    "varying mediump vec2 roundRectPos;\n"
    "void main(void) {\n"
    "    alpha = vtxAlpha;\n"
    "    vec4 transformedPosition = projection * transform * position;\n"
    "    gl_Position = transformedPosition;\n"
    "    roundRectPos = ((roundRectInvTransform * transformedPosition).xy / roundRectRadius) - roundRectInnerRectLTWH.xy;\n"
    "}\n";

static const char gFragmentShader[] = 
    "#version 100\n"
    "precision mediump float;\n"
    "varying float alpha;\n"
    "varying mediump vec2 roundRectPos;\n"
    "uniform vec4 color;\n"
    "uniform mediump vec4 roundRectInnerRectLTWH;\n"
    "uniform mediump float roundRectRadius;\n"
    "void main(void) {\n"
        "vec4 fragColor;\n"
        "fragColor = color;\n"
        "fragColor *= alpha;\n"
        "gl_FragColor = fragColor;\n"
        "mediump vec2 fragToLT = -roundRectPos;\n"
        "mediump vec2 fragFromRB = roundRectPos - roundRectInnerRectLTWH.zw;\n"
        "mediump vec2 dist = clamp(max(fragToLT, fragFromRB), 0.0, 1.0);\n"
        "mediump float linearDist = clamp(roundRectRadius - (length(dist) * roundRectRadius), 0.0, 1.0);\n"
        "gl_FragColor *= linearDist;\n"
    "}\n";

#else
// changed shader
static const char gVertexShader[] = 
    "#version 100\n"
    "attribute vec4 position;\n"
    "attribute float vtxAlpha;\n"
    "uniform mat4 projection;\n"
    "uniform mat4 transform;\n"
    "uniform mat4 roundRectInvTransform;\n"
    "uniform highp vec4 roundRectInnerRectLTWH;\n"
    "uniform highp float roundRectRadius;\n"
    "varying float alpha;\n"
    "varying highp vec2 roundRectPos;\n"
    "void main(void) {\n"
    "    alpha = vtxAlpha;\n"
    "    vec4 transformedPosition = projection * transform * position;\n"
    "    gl_Position = transformedPosition;\n"
    "    roundRectPos = ((roundRectInvTransform * transformedPosition).xy / roundRectRadius) - roundRectInnerRectLTWH.xy;\n"
    "}\n";

static const char gFragmentShader[] = 
    "#version 100\n"
    "precision highp float;\n"
    "varying float alpha;\n"
    "varying highp vec2 roundRectPos;\n"
    "uniform vec4 color;\n"
    "uniform highp vec4 roundRectInnerRectLTWH;\n"
    "uniform highp float roundRectRadius;\n"
    "void main(void) {\n"
        "vec4 fragColor;\n"
        "fragColor = color;\n"
        "fragColor *= alpha;\n"
        "gl_FragColor = fragColor;\n"
        "highp vec2 fragToLT = -roundRectPos;\n"
        "highp vec2 fragFromRB = roundRectPos - roundRectInnerRectLTWH.zw;\n"
        "highp vec2 dist = clamp(max(fragToLT, fragFromRB), 0.0, 1.0);\n"
        "highp float linearDist = clamp(roundRectRadius - (length(dist) * roundRectRadius), 0.0, 1.0);\n"
        "gl_FragColor *= linearDist;\n"
    "}\n";
#endif

GLuint gProgram;
GLint gaPositionLocation;
GLint gaVtxAlphaLocation = 1;
GLint guProjectionLocation;
GLint guTransformLocation;
GLint guRoundRectInvTransformLocation;
GLint guRoundRectInnerRectLTWHLocation;
GLint guRoundRectRadiusLocation;
GLint guColorLocation;

GLuint loadShader(GLenum shaderType, const char* pSource) {
    GLuint shader = glCreateShader(shaderType);
    if (shader) {
        glShaderSource(shader, 1, &pSource, NULL);
        glCompileShader(shader);
        GLint compiled = 0;
        glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
        if (!compiled) {
            GLint infoLen = 0;
            glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infoLen);
            if (infoLen) {
                char* buf = (char*) malloc(infoLen);
                if (buf) {
                    glGetShaderInfoLog(shader, infoLen, NULL, buf);
                    printf("Could not compile shader %d:\n%s\n",
                            shaderType, buf);
                    free(buf);
                }
                glDeleteShader(shader);
                shader = 0;
            }
        }
    }
    return shader; 
}

GLuint createProgram(const char* pVertexSource, const char* pFragmentSource) {
    GLuint vertexShader = loadShader(GL_VERTEX_SHADER, pVertexSource);
    if (!vertexShader) {
        return 0;
    }

    GLuint pixelShader = loadShader(GL_FRAGMENT_SHADER, pFragmentSource);
    if (!pixelShader) {
        return 0;
    }

    GLuint program = glCreateProgram();
    if (program) {
        glAttachShader(program, vertexShader);
        checkGlError("glAttachShader");
        glAttachShader(program, pixelShader);
        checkGlError("glAttachShader");

        // The original api use bind location for attribute vtxAlpha
        glBindAttribLocation(program, gaVtxAlphaLocation, "vtxAlpha");
        checkGlError("glBindAttribLocation");

        glLinkProgram(program);
        GLint linkStatus = GL_FALSE;
        glGetProgramiv(program, GL_LINK_STATUS, &linkStatus);
        if (linkStatus != GL_TRUE) {
            GLint bufLength = 0;
            glGetProgramiv(program, GL_INFO_LOG_LENGTH, &bufLength);
            if (bufLength) {
                char* buf = (char*) malloc(bufLength);
                if (buf) {
                    glGetProgramInfoLog(program, bufLength, NULL, buf);
                    printf("Could not link program:\n%s\n", buf);
                    free(buf);
                }
            }
            glDeleteProgram(program);
            program = 0; 
        }
    }
    return program;
}

bool setupGraphics(int w, int h) {
    printGLString("Version", GL_VERSION);
    printGLString("Vendor", GL_VENDOR);
    printGLString("Renderer", GL_RENDERER);
    printGLString("Extensions", GL_EXTENSIONS);

    printf("setupGraphics(%d, %d)\n", w, h);
    gProgram = createProgram(gVertexShader, gFragmentShader);
    if (!gProgram) {
        printf("Could not create program.\n");
        return false;
    }
    gaPositionLocation = glGetAttribLocation(gProgram, "position");
    printf("glGetAttribLocation(\"position\") = %d\n", gaPositionLocation);

    printf("glGetAttribLocation(\"vtxAlpha\") = %d, should be equal to %d\n",
        glGetAttribLocation(gProgram, "vtxAlpha"), gaVtxAlphaLocation);

    guProjectionLocation = glGetUniformLocation(gProgram, "projection");
    printf("glGetUniformLocation(\"projection\") = %d\n", guProjectionLocation);

    guTransformLocation = glGetUniformLocation(gProgram, "transform");
    printf("glGetUniformLocation(\"transform\") = %d\n", guTransformLocation);
    
    guRoundRectInvTransformLocation = glGetUniformLocation(gProgram, "roundRectInvTransform");
    printf("glGetUniformLocation(\"roundRectInvTransform\") = %d\n", guRoundRectInvTransformLocation);

    guRoundRectInnerRectLTWHLocation = glGetUniformLocation(gProgram, "roundRectInnerRectLTWH");
    printf("glGetUniformLocation(\"roundRectInnerRectLTWH\") = %d\n", guRoundRectInnerRectLTWHLocation);

    guRoundRectRadiusLocation = glGetUniformLocation(gProgram, "roundRectRadius");
    printf("glGetUniformLocation(\"roundRectRadius\") = %d\n", guRoundRectRadiusLocation);

    guColorLocation = glGetUniformLocation(gProgram, "color");
    printf("glGetUniformLocation(\"color\") = %d\n", guColorLocation);

    // use the same setting as MGD
    glViewport(0, 0, w, h);
    checkGlError("glViewport");
    return true;
}

#if MGD_DATA
const GLfloat vertices[] = {
    /* x            y               z       w      alpha */
    -0.5,           886.0355,       0.0,    1.0,    0.0,
    0.5,            885.9645,       0.0,    1.0,    1.0,
    -0.5,           3.964466,       0.0,    1.0,    0.0,
    0.5,            4.035534,       0.0,    1.0,    1.0,
    -0.23069537,    2.0793335,      0.0,    1.0,    0.0,
    0.73069537,     2.4206665,      0.0,    1.0,    1.0,
    0.6355655,      0.6355655,      0.0,    1.0,    0.0,
    1.3644345,      1.3644345,      0.0,    1.0,    1.0,
    2.0793335,      -0.23069537,    0.0,    1.0,    0.0,
    2.4206665,      0.73069537,     0.0,    1.0,    1.0,
    3.964466,       -0.5,           0.0,    1.0,    0.0,
    4.035534,       0.5,            0.0,    1.0,    1.0,
    616.0355,       -0.5,           0.0,    1.0,    0.0,
    615.9645,       0.5,            0.0,    1.0,    1.0,
    617.92065,      -0.23069537,    0.0,    1.0,    0.0,
    617.57935,      0.73069537,     0.0,    1.0,    1.0,
    619.36444,      0.6355655,      0.0,    1.0,    0.0,
    618.63556,      1.3644345,      0.0,    1.0,    1.0,
    620.2307,       2.0793335,      0.0,    1.0,    0.0,
    619.2693,       2.4206665,      0.0,    1.0,    1.0,
    620.5,          3.964466,       0.0,    1.0,    0.0,
    619.5,          4.035534,       0.0,    1.0,    1.0,
    620.5,          886.0355,       0.0,    1.0,    0.0,
    619.5,          885.9645,       0.0,    1.0,    1.0,
    620.2307,       887.92065,      0.0,    1.0,    0.0,
    619.2693,       887.57935,      0.0,    1.0,    1.0,
    619.36444,      889.36444,      0.0,    1.0,    0.0,
    618.63556,      888.63556,      0.0,    1.0,    1.0,
    617.92065,      890.2307,       0.0,    1.0,    0.0,
    617.57935,      889.2693,       0.0,    1.0,    1.0,
    616.0355,       890.5,          0.0,    1.0,    0.0,
    615.9645,       889.5,          0.0,    1.0,    1.0,
    3.964466,       890.5,          0.0,    1.0,    0.0,
    4.035534,       889.5,          0.0,    1.0,    1.0,
    2.0793335,      890.2307,       0.0,    1.0,    0.0,
    2.4206665,      889.2693,       0.0,    1.0,    1.0,
    0.6355655,      889.36444,      0.0,    1.0,    0.0,
    1.3644345,      888.63556,      0.0,    1.0,    1.0,
    -0.23069537,    887.92065,      0.0,    1.0,    0.0,
    0.73069537,     887.57935,      0.0,    1.0,    1.0,
    -0.5,           886.0355,       0.0,    1.0,    0.0,
    0.5,            885.9645,       0.0,    1.0,    1.0,
    0.5,            885.9645,       0.0,    1.0,    1.0,
    0.73069537,     887.57935,      0.0,    1.0,    1.0,
    0.5,            4.035534,       0.0,    1.0,    1.0,
    1.3644345,      888.63556,      0.0,    1.0,    1.0,
    0.73069537,     2.4206665,      0.0,    1.0,    1.0,
    2.4206665,      889.2693,       0.0,    1.0,    1.0,
    1.3644345,      1.3644345,      0.0,    1.0,    1.0,
    4.035534,       889.5,          0.0,    1.0,    1.0,
    2.4206665,      0.73069537,     0.0,    1.0,    1.0,
    615.9645,       889.5,          0.0,    1.0,    1.0,
    4.035534,       0.5,            0.0,    1.0,    1.0,
    617.57935,      889.2693,       0.0,    1.0,    1.0,
    615.9645,       0.5,            0.0,    1.0,    1.0,
    618.63556,      888.63556,      0.0,    1.0,    1.0,
    617.57935,      0.73069537,     0.0,    1.0,    1.0,
    619.2693,       887.57935,      0.0,    1.0,    1.0,
    618.63556,      1.3644345,      0.0,    1.0,    1.0,
    619.5,          885.9645,       0.0,    1.0,    1.0,
    619.2693,       2.4206665,      0.0,    1.0,    1.0,
    619.5,          4.035534,       0.0,    1.0,    1.0,
};

#else

const GLfloat vertices[] = {
    /* x            y               alpha */
    -0.5,           886.0355,       0.0,
    0.5,            885.9645,       1.0,
    -0.5,           3.964466,       0.0,
    0.5,            4.035534,       1.0,
    -0.23069537,    2.0793335,      0.0,
    0.73069537,     2.4206665,      1.0,
    0.6355655,      0.6355655,      0.0,
    1.3644345,      1.3644345,      1.0,
    2.0793335,      -0.23069537,    0.0,
    2.4206665,      0.73069537,     1.0,
    3.964466,       -0.5,           0.0,
    4.035534,       0.5,            1.0,
    616.0355,       -0.5,           0.0,
    615.9645,       0.5,            1.0,
    617.92065,      -0.23069537,    0.0,
    617.57935,      0.73069537,     1.0,
    619.36444,      0.6355655,      0.0,
    618.63556,      1.3644345,      1.0,
    620.2307,       2.0793335,      0.0,
    619.2693,       2.4206665,      1.0,
    620.5,          3.964466,       0.0,
    619.5,          4.035534,       1.0,
    620.5,          886.0355,       0.0,
    619.5,          885.9645,       1.0,
    620.2307,       887.92065,      0.0,
    619.2693,       887.57935,      1.0,
    619.36444,      889.36444,      0.0,
    618.63556,      888.63556,      1.0,
    617.92065,      890.2307,       0.0,
    617.57935,      889.2693,       1.0,
    616.0355,       890.5,          0.0,
    615.9645,       889.5,          1.0,
    3.964466,       890.5,          0.0,
    4.035534,       889.5,          1.0,
    2.0793335,      890.2307,       0.0,
    2.4206665,      889.2693,       1.0,
    0.6355655,      889.36444,      0.0,
    1.3644345,      888.63556,      1.0,
    -0.23069537,    887.92065,      0.0,
    0.73069537,     887.57935,      1.0,
    -0.5,           886.0355,       0.0,
    0.5,            885.9645,       1.0,
    0.5,            885.9645,       1.0,
    0.73069537,     887.57935,      1.0,
    0.5,            4.035534,       1.0,
    1.3644345,      888.63556,      1.0,
    0.73069537,     2.4206665,      1.0,
    2.4206665,      889.2693,       1.0,
    1.3644345,      1.3644345,      1.0,
    4.035534,       889.5,          1.0,
    2.4206665,      0.73069537,     1.0,
    615.9645,       889.5,          1.0,
    4.035534,       0.5,            1.0,
    617.57935,      889.2693,       1.0,
    615.9645,       0.5,            1.0,
    618.63556,      888.63556,      1.0,
    617.57935,      0.73069537,     1.0,
    619.2693,       887.57935,      1.0,
    618.63556,      1.3644345,      1.0,
    619.5,          885.9645,       1.0,
    619.2693,       2.4206665,      1.0,
    619.5,          4.035534,       1.0,
};

#endif

int main() {
    sp<ProcessState> proc(ProcessState::self());
    ProcessState::self()->startThreadPool();

    sp<SurfaceComposerClient> client = new SurfaceComposerClient();
    sp<SurfaceControl> surfaceControl = client->createSurface(String8("my_surface"),
    	812, 1082, HAL_PIXEL_FORMAT_RGBA_8888, 0);
    if (surfaceControl == NULL) {
        printf("Failed to get SurfaceControl\n");
        return -1;
    }
    
    sp<Surface> nativeSurface = surfaceControl->getSurface();
    if (nativeSurface == NULL) {
        printf("Failed to get Surface\n");
        return -1;
    }
    printf("Surface got\n");

    // make sure the surface is on top
    SurfaceComposerClient::openGlobalTransaction();
    surfaceControl->setLayer(0xFFFFFE);
    surfaceControl->setPosition(0, 0);
    //surfaceControl->setAlpha(0.5);
    SurfaceComposerClient::closeGlobalTransaction();

	ANativeWindow* const nativeWindow = nativeSurface.get();

	EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (display == EGL_DEFAULT_DISPLAY) {
        printf("Unable to connect window system: %s\n", eglStatusStr());
        return -1;
    }
    EGLint majorVersion, minorVersion;
    if (!eglInitialize(display, &majorVersion, &minorVersion)) {
        printf("Unable to initialize egl: %s\n", eglStatusStr());
        return -1;
    }

    const EGLint configAttribs[] = {
        EGL_SURFACE_TYPE,       EGL_WINDOW_BIT,
        EGL_BLUE_SIZE,          8,
        EGL_GREEN_SIZE,         8,
        EGL_RED_SIZE,           8,
        EGL_ALPHA_SIZE,         8,
        EGL_DEPTH_SIZE,         0,
        EGL_STENCIL_SIZE,       0,
        EGL_SAMPLE_BUFFERS,     0,
        EGL_RENDERABLE_TYPE,    EGL_OPENGL_ES2_BIT,
        EGL_NONE
    };

    EGLConfig config;
    EGLint numConfigs;
    if (eglChooseConfig(display, configAttribs, &config, 1, &numConfigs) != EGL_TRUE) {
        printf("Unable to choose egl config: %s\n", eglStatusStr());
        return -1;
    }
    printf("eglChooseConfig return %d configs\n", numConfigs);
    
    if (numConfigs < 1) {
    	return -1;
    }

    EGLint format, redSize, greenSize, blueSize, alphaSize;
    eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &format);
    eglGetConfigAttrib(display, config, EGL_RED_SIZE, &redSize);
    eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &greenSize);
    eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &blueSize);
    eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &alphaSize);
    printf("eglGetConfigAttrib format: %d, r: %d, g: %d, b: %d, a: %d\n",
    	format, redSize, greenSize, blueSize, alphaSize);
    //ANativeWindow_setBuffersGeometry(nativeWindow, 0, 0, format);

    EGLSurface surface = eglCreateWindowSurface(display, config, nativeWindow, NULL);
    if (surface == EGL_NO_SURFACE) {
        printf("Unable to create surface: %s\n", eglStatusStr());
        return -1;
    }
    EGLint contextAttrs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 3,
        EGL_NONE
    };

    EGLContext context = eglCreateContext(display, config, NULL, contextAttrs);
    if (context == EGL_NO_CONTEXT) {
        printf("Unable to create context: %s\n", eglStatusStr());
        return -1;
    }
    if (eglMakeCurrent(display, surface, surface, context) == EGL_FALSE) {
        printf("Unable to eglMakeCurrent\n");
        return -1;
    }

    printf("%s\n", glGetString(GL_VENDOR));
    printf("%s\n", glGetString(GL_RENDERER));
    printf("%s\n", glGetString(GL_VERSION));
    printf("%s\n", glGetString(GL_EXTENSIONS));

    EGLint w, h;
    eglQuerySurface(display, surface, EGL_WIDTH, &w);
    eglQuerySurface(display, surface, EGL_HEIGHT, &h);

    printf("eglQuerySurface get width: %d, height: %d\n", w, h);

    setupGraphics(w, h);

    while (true) {
	    glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
	    checkGlError("glClearColor");
	    glClear(GL_COLOR_BUFFER_BIT);
	    checkGlError("glClear");

	    glUseProgram(gProgram);
	    checkGlError("glUseProgram");

        // this call is NOT shown in MGD
	    glEnableVertexAttribArray(gaPositionLocation);
        checkGlError("glEnableVertexAttribArray");

	    //glVertexAttribPointer(gaPositionLocation, 4, GL_FLOAT, GL_FALSE, 20, vertices);
	    glVertexAttribPointer(gaPositionLocation, 2, GL_FLOAT, GL_FALSE, 12, vertices);
        checkGlError("glVertexAttribPointer");

        // this call exists in MGD
	    glEnableVertexAttribArray(gaVtxAlphaLocation);
        checkGlError("glEnableVertexAttribArray");

        //glVertexAttribPointer(gaVtxAlphaLocation, 1, GL_FLOAT, GL_FALSE, 20, vertices + 4);
	    glVertexAttribPointer(gaVtxAlphaLocation, 1, GL_FLOAT, GL_FALSE, 12, vertices + 2);

        glUniform4f(guColorLocation, 1.0, 1.0, 1.0, 1.0);
        checkGlError("glUniform4f a");

        #if MGD_DATA
		GLfloat projection[] = {
            0.0024630541, 0.0, 0.0, -1.0,
            0.0, -0.0018484289, 0.0, 1.0,
            0.0, 0.0, -1.0, -0.0,
            0.0, 0.0, 0.0, 1.0
        };
        #else
        GLfloat projection[] = {
            0.00246305, 0.0, 0.0, 0.0,
            0.0, -0.00184843, 0.0, 0.0,
            0.0, 0.0, -1.0, 0.0,
            -1.0, 1.0, 0.0, 1.0
        };
        #endif
        glUniformMatrix4fv(guProjectionLocation, 1, GL_FALSE, projection);
        checkGlError("glUniformMatrix4fv f");

        #if MGD_DATA
        GLfloat transform[] = {
            1.0, 0.0, 0.0, 96.0,
            0.0, 1.0, 0.0, 96.0,
            0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0
        };
        #else
        GLfloat transform[] = {
            1.0, 0.0, 0.0, 0.0,
            0.0, 1.0, 0.0, 0.0,
            0.0, 0.0, 1.0, 0.0, 
            96.0, 96.0, 0.0, 1.0
        };    
        #endif

        glUniformMatrix4fv(guTransformLocation, 1, GL_FALSE, transform);
        checkGlError("glUniformMatrix4fv b");

        glUniform4f(guRoundRectInnerRectLTWHLocation, 8.0, 8.0, 136.0, 196.0);
        checkGlError("glUniform4f c");

        #if MGD_DATA
        GLfloat roundRectInvTransform[] = {
            406.0,  0.0,        0.0, 342.00003,
            -0.0,   -541.0,     0.0, 476.99997,
            0.0,    0.0,        1.0, 0.0,
            -0.0,   -0.0,       0.0, 1.0
        };
        #else
        GLfloat roundRectInvTransform[] = {
            406.0,  -0.0,       0.0, -0.0,
            0.0,    -541.0,     0.0, -0.0,
            0.0,    0.0,        1.0, 0.0,
            342,    477,        0.0, 1.0
        };
        #endif

        glUniformMatrix4fv(guRoundRectInvTransformLocation, 1, GL_FALSE, roundRectInvTransform);
        checkGlError("glUniformMatrix4fv d");

        glUniform1f(guRoundRectRadiusLocation, 4.5);
        checkGlError("glUniform1f e");

        glDrawArrays(GL_TRIANGLE_STRIP, 0, 62);
	    checkGlError("glDrawArrays");

	    eglSwapBuffers(display, surface);
    }

    IPCThreadState::self()->joinThreadPool();
    
    return 0;
}

