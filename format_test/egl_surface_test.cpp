#define LOG_TAG "egl_surface_test"
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
    ALOGI("GL %s = %s\n", name, v);
}

static void checkGlError(const char* op) {                                                                                                                             
    for (GLint error = glGetError(); error; error
            = glGetError()) {
        ALOGI("after %s() glError (0x%x)\n", op, error);
    }
}

static const char gVertexShader[] = 
    "attribute vec4 vPosition;\n"
    "void main() {\n"                                                                                                                                                  
    "  gl_Position = vPosition;\n"
    "}\n";

static const char gFragmentShader[] = 
    "precision mediump float;\n"
    "void main() {\n"
    "  gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);\n"
    "}\n";

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
                    ALOGE("Could not compile shader %d:\n%s\n",
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
                    ALOGE("Could not link program:\n%s\n", buf);
                    free(buf);
                }
            }
            glDeleteProgram(program);
            program = 0;                                                                                                                                               
        }
    }
    return program;
}

GLuint gProgram;
GLuint gvPositionHandle;

bool setupGraphics(int w, int h) {
    printGLString("Version", GL_VERSION);
    printGLString("Vendor", GL_VENDOR);
    printGLString("Renderer", GL_RENDERER);
    printGLString("Extensions", GL_EXTENSIONS);

    ALOGI("setupGraphics(%d, %d)", w, h);
    gProgram = createProgram(gVertexShader, gFragmentShader);
    if (!gProgram) {
        ALOGE("Could not create program.");
        return false;
    }
    gvPositionHandle = glGetAttribLocation(gProgram, "vPosition");
    checkGlError("glGetAttribLocation");
    ALOGI("glGetAttribLocation(\"vPosition\") = %d\n", gvPositionHandle);

    glViewport(0, 0, w, h);
    checkGlError("glViewport");
    return true;
}

const GLfloat gTriangleVertices[] = { 0.0f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f };

int main() {
    sp<ProcessState> proc(ProcessState::self());
    ProcessState::self()->startThreadPool();

    sp<SurfaceComposerClient> client = new SurfaceComposerClient();
    sp<SurfaceControl> surfaceControl = client->createSurface(String8("egl_surface_test"),
    	640, 480, HAL_PIXEL_FORMAT_R10G10B10A2, 0);
    if (surfaceControl == NULL) {
        ALOGE("Failed to get SurfaceControl");
        return -1;
    }
    
    sp<Surface> nativeSurface = surfaceControl->getSurface();
    if (nativeSurface == NULL) {
        ALOGE("Failed to get Surface");
        return -1;
    }
    ALOGI("Surface got");

    // make sure the surface is on top
    SurfaceComposerClient::openGlobalTransaction();
    surfaceControl->setLayer(0xFFFFFE);
    surfaceControl->setPosition(300, 300);
    surfaceControl->setAlpha(0.5);
    SurfaceComposerClient::closeGlobalTransaction();

	ANativeWindow* const nativeWindow = nativeSurface.get();

	EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (display == EGL_DEFAULT_DISPLAY) {
        ALOGE("Unable to connect window system: %s", eglStatusStr());
        return false;
    }
    EGLint majorVersion, minorVersion;
    if (!eglInitialize(display, &majorVersion, &minorVersion)) {
        ALOGE("Unable to initialize egl: %s", eglStatusStr());
        return false;
    }

    const EGLint configAttribs[] = {
        EGL_SURFACE_TYPE,       EGL_WINDOW_BIT,
        EGL_BLUE_SIZE,          10,
        EGL_GREEN_SIZE,         10,
        EGL_RED_SIZE,           10,
        EGL_ALPHA_SIZE,         2,
        EGL_DEPTH_SIZE,         0,
        EGL_STENCIL_SIZE,       0,
        EGL_SAMPLE_BUFFERS,     0,
        EGL_RENDERABLE_TYPE,    EGL_OPENGL_ES2_BIT,
        EGL_NONE
    };

    EGLConfig config;
    EGLint numConfigs;
    if (eglChooseConfig(display, configAttribs, &config, 1, &numConfigs) != EGL_TRUE) {
        ALOGE("Unable to choose egl config: %s", eglStatusStr());
        return false;
    }
    ALOGI("eglChooseConfig return %d configs", numConfigs);
    
    if (numConfigs < 1) {
    	return false;
    }

    EGLint format, redSize, greenSize, blueSize, alphaSize;
    eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &format);
    eglGetConfigAttrib(display, config, EGL_RED_SIZE, &redSize);
    eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &greenSize);
    eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &blueSize);
    eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &alphaSize);
    ALOGI("eglGetConfigAttrib format: %d, r: %d, g: %d, b: %d, a: %d",
    	format, redSize, greenSize, blueSize, alphaSize);
    //ANativeWindow_setBuffersGeometry(nativeWindow, 0, 0, format);

    EGLSurface surface = eglCreateWindowSurface(display, config, nativeWindow, NULL);
    if (surface == EGL_NO_SURFACE) {
        ALOGE("Unable to create surface: %s", eglStatusStr());
        return false;
    }
    EGLint contextAttrs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 3,
        EGL_NONE
    };

    EGLContext context = eglCreateContext(display, config, NULL, contextAttrs);
    if (context == EGL_NO_CONTEXT) {
        ALOGE("Unable to create context: %s", eglStatusStr());
        return false;
    }
    if (eglMakeCurrent(display, surface, surface, context) == EGL_FALSE) {
        ALOGE("Unable to eglMakeCurrent");
        return false;
    }

    ALOGI("%s", glGetString(GL_VENDOR));
    ALOGI("%s", glGetString(GL_RENDERER));
    ALOGI("%s", glGetString(GL_VERSION));
    ALOGI("%s", glGetString(GL_EXTENSIONS));

    EGLint w, h;
    eglQuerySurface(display, surface, EGL_WIDTH, &w);
    eglQuerySurface(display, surface, EGL_HEIGHT, &h);

    ALOGI("eglQuerySurface get width: %d, height: %d", w, h);

    setupGraphics(w, h);

    while (true) {
    	static float grey;
	    grey += 0.01f;
	    if (grey > 1.0f) {
	        grey = 0.0f;
	    }
	    glClearColor(grey, grey, grey, 1.0f);
	    checkGlError("glClearColor");
	    glClear( GL_DEPTH_BUFFER_BIT | GL_COLOR_BUFFER_BIT);
	    checkGlError("glClear");

	    glUseProgram(gProgram);
	    checkGlError("glUseProgram");

	    glVertexAttribPointer(gvPositionHandle, 2, GL_FLOAT, GL_FALSE, 0, gTriangleVertices);
	    checkGlError("glVertexAttribPointer");
	    glEnableVertexAttribArray(gvPositionHandle);
	    checkGlError("glEnableVertexAttribArray");
	    glDrawArrays(GL_TRIANGLES, 0, 3);
	    checkGlError("glDrawArrays");

	    eglSwapBuffers(display, surface);
    }

    IPCThreadState::self()->joinThreadPool();
    
    return 0;
}

