#define EGL_EGLEXT_PROTOTYPES
#define GL_GLEXT_PROTOTYPES

#include <stdlib.h>
#include <android/native_window.h>
#include <binder/IPCThreadState.h>
#include <binder/ProcessState.h>
#include <binder/IServiceManager.h>
#include <gui/Surface.h>
#include <gui/SurfaceComposerClient.h>

#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>

using namespace android;

extern unsigned char rgb565_data[];
extern unsigned int rgb565_data_len;
extern unsigned char golden_bmp_argb_data[];
extern unsigned int golden_bmp_argb_data_len;

static const float PIXEL_ERROR_THRESHOLD = 0.03f;
static const float PIXEL_ERROR_COUNT_THRESHOLD = 0.005f;
static const int PIXEL_ERROR_TOLERANCE = 3;

/**
 * Use the same format as in cts
 */
#define red(c)   ((c >> 16) & 0xFF)
#define green(c) ((c >>  8) & 0xFF)
#define blue(c)  ((c >>  0) & 0xFF)
#define alpha(c) ((c >> 24) & 0xFF)

/**
 * @param message Error message
 * @param expected Expected bitmap
 * @param actual Actual bitmap
 * @param pixelThreshold The total difference threshold for a single pixel
 * @param pixelCountThreshold The total different pixel count threshold
 * @param pixelDiffTolerance The pixel value difference tolerance
 *
 */
static void compareImagesARGB(
        const char* message,
        unsigned char* expected,
        unsigned char* actual,
        int idealWidth,
        int idealHeight,
        float pixelThreshold, float pixelCountThreshold, int pixelDiffTolerance)
{
    int totalDiffPixelCount = 0;
    float totalPixelCount = idealWidth * idealHeight;
    for (int x = 0; x < idealWidth; x++) {
        for (int y = 0; y < idealHeight; y++) {
            int idealColor = expected[y * idealWidth + x];
            int givenColor = actual[y * idealWidth + x];
            if (idealColor == givenColor)
                continue;
 
            float totalError = 0;
            totalError += abs(red(idealColor) - red(givenColor));
            totalError += abs(green(idealColor) - green(givenColor));
            totalError += abs(blue(idealColor) - blue(givenColor));
            totalError += abs(alpha(idealColor) - alpha(givenColor));

            if ((totalError / 1024.0f) >= pixelThreshold) {
                printf("%s: totalError is %f\n", message, totalError);
            }

            if (totalError > pixelDiffTolerance) {
                totalDiffPixelCount++;
            }
        }
    }
    if ((totalDiffPixelCount / totalPixelCount) >= pixelCountThreshold) {
        printf("compare failed, %s: totalDiffPixelCount is %d\n", message, totalDiffPixelCount);

        FILE *fp = fopen("/data/local/tmp/readbackFromSurface.data", "w+");
        if (fp == NULL) {
            printf("/data/local/tmp: no permission or doesn't exist\n");
            return;
        }
        fwrite(actual, 1, 64 * 64 * 4, fp);
        fflush(fp);
        fclose(fp);
    } else {
        printf("compare success, cts should pass\n");
    }
}

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
    for (GLint error = glGetError(); error; error = glGetError()) {
        printf("after %s() glError (0x%x)\n", op, error);
    }
}

static const char gVertexShader[] = 
    "attribute vec2 aPosition;\n"
    "attribute vec2 aTexCoord;\n"
    "varying vec2 vTexCoord;\n"
    "void main() {\n"
    "  gl_Position = vec4(aPosition, 0, 1);\n"
    "  vTexCoord = aTexCoord;\n"
    "}\n";

static const char gFragmentShader[] = 
    "#extension GL_OES_EGL_image_external : require\n"
    "precision mediump float;\n"
    "varying vec2 vTexCoord;\n"
    "uniform samplerExternalOES sTexture;\n"
    "void main() {\n"
    "  gl_FragColor = texture2D(sTexture, vTexCoord);\n"
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

GLuint gProgram;
GLint gaPositionLocation;
GLint gaTexCoordLocation;
GLint guSamplerLocation;
GLuint gTexId;

#define FLIPPED_TEXCOORDS

#ifdef FLIPPED_TEXCOORDS

// vertices
// 0      3
// 1      2
//
// 0,1,2
// 0,2,3
//
// flipped in tex coordinates, so that we can compare with golden in raster scan order 
const GLfloat gQuadVertices[] = { 
    // x,  y,   s,   t
    -1.f,  1.f, 0.f, 0.f,
    -1.f, -1.f, 0.f, 1.f,
     1.f, -1.f, 1.f, 1.f,

    -1.f,  1.f, 0.f, 0.f,
     1.f, -1.f, 1.f, 1.f,
     1.f,  1.f, 1.f, 0.f
};

#else

// non-flipped tex coordinates
const GLfloat gQuadVertices[] = { 
    // x,  y,   s,   t
    -1.f,  1.f, 0.f, 1.f,
    -1.f, -1.f, 0.f, 0.f,
     1.f, -1.f, 1.f, 0.f,

    -1.f,  1.f, 0.f, 1.f,
     1.f, -1.f, 1.f, 0.f,
     1.f,  1.f, 1.f, 1.f
};

#endif

int main() {
    sp<ProcessState> proc(ProcessState::self());
    ProcessState::self()->startThreadPool();

    sp<SurfaceComposerClient> client = new SurfaceComposerClient();
    sp<SurfaceControl> surfaceControl = client->createSurface(String8("rgba8888"),
    	64, 64, HAL_PIXEL_FORMAT_RGBA_8888, 0);
    if (surfaceControl == NULL) {
        printf("Failed to get SurfaceControl\n");
        return -1;
    }
    
    sp<Surface> nativeSurface = surfaceControl->getSurface();
    if (nativeSurface == NULL) {
        printf("Failed to get Surface\n");
        return -1;
    }

    // make sure the surface is on top
    SurfaceComposerClient::openGlobalTransaction();
    surfaceControl->setLayer(0xFFFFFE);
    surfaceControl->setPosition(0, 0);
    surfaceControl->setAlpha(1.0);
    SurfaceComposerClient::closeGlobalTransaction();

	ANativeWindow* const nativeWindow = nativeSurface.get();

    ////////////////////////////////
    // setup EGL                  //
    ////////////////////////////////
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

    // prepare EGLImage which doesn't need context

    //PFNEGLCREATEIMAGEKHRPROC eglCreateImageKHR = (PFNEGLCREATEIMAGEKHRPROC)eglGetProcAddress("eglCreateImageKHR");
    //PFNEGLDESTROYIMAGEKHRPROC eglDestroyImageKHR = (PFNEGLDESTROYIMAGEKHRPROC)eglGetProcAddress("eglDestoryImageKHR");
    //PFNGLEGLIMAGETARGETTEXTURE2DOESPROC glEGLImageTargetTexture2DOES =
    //    (PFNGLEGLIMAGETARGETTEXTURE2DOESPROC)eglGetProcAddress("glEGLImageTargetTexture2DOES");
    
    sp<GraphicBuffer> sourceGraphicBuffer = new GraphicBuffer(
            64, 64, PIXEL_FORMAT_RGB_565,
            GraphicBuffer::USAGE_SW_WRITE_OFTEN | GraphicBuffer::USAGE_HW_TEXTURE);

    unsigned char *rawImageData = NULL;
    sourceGraphicBuffer->lock(GraphicBuffer::USAGE_SW_WRITE_OFTEN, (void **)&rawImageData);
    memcpy(rawImageData, rgb565_data, rgb565_data_len);
    sourceGraphicBuffer->unlock();
   
    EGLClientBuffer clientBuffer = (EGLClientBuffer) sourceGraphicBuffer->getNativeBuffer();

    EGLint eglImageAttributes[] = {
    //    EGL_WIDTH, 64,
    //    EGL_HEIGHT, 64,
    //    EGL_MATCH_FORMAT_KHR, EGL_FORMAT_RGB_565_KHR,
        EGL_IMAGE_PRESERVED_KHR, EGL_TRUE,
        EGL_NONE
    };

    EGLImageKHR sourceEglImage = eglCreateImageKHR(
        display,
        EGL_NO_CONTEXT,
        EGL_NATIVE_BUFFER_ANDROID,
        clientBuffer,
        eglImageAttributes);

    if (sourceEglImage == EGL_NO_IMAGE_KHR) {
        printf("eglCreateImageKHR failed. %s\n", eglStatusStr());
        return -1;
    }

    // prepare surfaces and context to draw on
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
    
    if (numConfigs < 1) {
        printf("eglChooseConfig failed\n");
    	return -1;
    }

    EGLSurface surface = eglCreateWindowSurface(display, config, nativeWindow, NULL);
    if (surface == EGL_NO_SURFACE) {
        printf("Unable to create surface: %s\n", eglStatusStr());
        return -1;
    }
    EGLint contextAttrs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 2,
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

    EGLint w, h;
    eglQuerySurface(display, surface, EGL_WIDTH, &w);
    eglQuerySurface(display, surface, EGL_HEIGHT, &h);

    //printf("eglQuerySurface get width: %d, height: %d\n", w, h);

    ////////////////////////////////
    // setup GL                  //
    ////////////////////////////////
    //printGLString("Version", GL_VERSION);
    //printGLString("Vendor", GL_VENDOR);
    //printGLString("Renderer", GL_RENDERER);
    //printGLString("Extensions", GL_EXTENSIONS);

    gProgram = createProgram(gVertexShader, gFragmentShader);
    if (!gProgram) {
        printf("Could not create program\n");
        return -1;
    }
    gaPositionLocation = glGetAttribLocation(gProgram, "aPosition");
    checkGlError("glGetAttribLocation aPosition\n");
    //printf("glGetAttribLocation(\"aPosition\") = %d\n", gaPositionLocation);

    gaTexCoordLocation = glGetAttribLocation(gProgram, "aTexCoord");
    checkGlError("glGetAttribLocation aTexCoord");
    //printf("glGetAttribLocation(\"aTexCoord\") = %d\n", gaTexCoordLocation);

    glViewport(0, 0, w, h);
    checkGlError("glViewport");

    glGenTextures(1, &gTexId);
    glBindTexture(GL_TEXTURE_EXTERNAL_OES, gTexId);
    glEGLImageTargetTexture2DOES(GL_TEXTURE_EXTERNAL_OES, sourceEglImage);
    checkGlError("glEGLImageTargetTexture2DOES");
    eglDestroyImageKHR(display, sourceEglImage);

    guSamplerLocation = glGetUniformLocation(gProgram, "sTexture");
    checkGlError("glGetUniformLocation sTexture");
    //printf("glGetUniformLocation(\"sTexture\") = %d\n", guSamplerLocation);
	
    glClearColor(0, 0, 0, 1.0f);

    while (true) {

        static int dumpOnce = 0;
	    glClear(GL_COLOR_BUFFER_BIT);

	    glUseProgram(gProgram);
	    checkGlError("glUseProgram");

        glActiveTexture(GL_TEXTURE0);
        glUniform1i(guSamplerLocation, 0);
        glBindTexture(GL_TEXTURE_EXTERNAL_OES, gTexId);

	    glVertexAttribPointer(gaPositionLocation, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(GL_FLOAT), gQuadVertices);
	    glEnableVertexAttribArray(gaPositionLocation);

        glVertexAttribPointer(gaTexCoordLocation, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(GL_FLOAT), &gQuadVertices[2]);
	    glEnableVertexAttribArray(gaTexCoordLocation);

	    glDrawArrays(GL_TRIANGLES, 0, 6);
	    checkGlError("glDrawArrays");

        if (!dumpOnce) {
            unsigned char readBackData[64 * 64 * 4];
            glReadPixels(0, 0, 64, 64, GL_RGBA, GL_UNSIGNED_BYTE, (void *)readBackData);
            compareImagesARGB(
                    "compare golden with surface readback", golden_bmp_argb_data, readBackData, 64, 64,
                    PIXEL_ERROR_THRESHOLD, PIXEL_ERROR_COUNT_THRESHOLD, PIXEL_ERROR_TOLERANCE);
            dumpOnce++;
        }
	    eglSwapBuffers(display, surface);
    }

    IPCThreadState::self()->joinThreadPool();
    
    return 0;
}


