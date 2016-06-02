#include <android/native_window.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <Platform.h> // from common_native lib, for LOG facility
#include <vector>
#include "utils.h"
#include <Timer.h>
#include "NativeWinApp.h"

using namespace MaliSDK;
using namespace android;
using namespace std;

static const char gVertexShader[] =
                "#version 300 es\n"
                "in vec4 aPosition;\n"
                "out vec4 vPosition;\n"
                "void main() {\n"
                "   vPosition = aPosition;\n"
                "   gl_Position = aPosition;\n"
                "}\n";

static const char gFragmentShader[] =
                "#version 300 es\n"
                "precision mediump float;\n"
                "in vec4 vPosition;\n"
                "out vec4 fragColor;\n"
                "void main() {\n"
                "   vec4 red = vec4(1.0, 0.0, 0.0, 1.0);\n"
                "   vec4 green = vec4(0.0, 1.0, 0.0, 1.0);\n"
                "   vec4 blue = vec4(0.0, 0.0, 1.0, 1.0);\n"
                "   int xmod = int(abs(vPosition.x) * 20.0) % 3;\n"
                "   if (xmod == 0)\n"
                "       fragColor = red;\n"
                "   else if (xmod == 1)\n"
                "       fragColor = green;\n"
                "   else\n"
                "       fragColor = blue;\n"
                "}\n";

static const GLfloat gSquareVertices[] = {
        -1.0f, 1.0f,
        -1.0f, -1.0f,
        0.0f, -1.0f,
        -1.0f, 1.0f,
        0.0f, -1.0f,
        0.0f, 1.0f
};


class App : public NativeWinApp
{
public:
    App() {
        testCaseNo = 0;
    }

    virtual bool postInitializeEGL(EGLDisplay display, EGLSurface surface)
    {
        eglSetDamageRegionKHR =
            (PFNEGLSETDAMAGEREGIONKHRPROC)eglGetProcAddress("eglSetDamageRegionKHR");
        if (!eglSetDamageRegionKHR) {
            LOGE("No eglSetDamageRegionKHR symbol found\n");
            return false;
        }

        return true;
    }

    virtual bool setupGraphics()
    {
        _program = GLUtil::createProgram(gVertexShader, gFragmentShader);
        if (!_program) {
            LOGE("Could not create program.");
            return false;
        }
        GL_CHECK(_positionHandle = glGetAttribLocation(_program, "aPosition"));
        GL_CHECK(glViewport(0, 0, _winWidth, _winHeight));
        //LOGI("%dx%d", _winWidth, _winHeight);
        _buf.resize(_winWidth * _winHeight * 4);

        _fpsTimer.reset();
        return true;
    }

    virtual void startFrame()
    {
        static float count = 0;
        count += 0.005f;
        if (count > 1.0f) {
            count = 0.0f;
            testCaseNo++;
            if (testCaseNo >= 4)
                testCaseNo = 0;
        }

        EGLint age = -1;
        EGLDisplay display = getEGLDisplay();
        EGLSurface surface = getEGLSurface();
        if (!eglQuerySurface(getEGLDisplay(), getEGLSurface(), EGL_BUFFER_AGE_KHR, &age)) {
            LOGE("query buffer age of surface failed\n");
            return;
        }

        EGLint rects[4]; // x, y, width, height in lower left corner of the surface
        rects[0] = 0;
        rects[1] = 0;
        rects[2] = 1920;
        rects[3] = 1080;
        if (!eglSetDamageRegionKHR(display, surface, rects, 1)) {
            LOGE("Error call eglSetDamageRegionKHR, %s\n", GLUtil::eglStatusStr());
            return;
        }
    }

    virtual void renderFrame()
    {
        static float grey;
        grey += 0.005f;
        if (grey > 1.0f) {
            grey = 0.0f;
        }
        GL_CHECK(glClearColor(grey, grey, grey, 1.0f));
        GL_CHECK(glClear( GL_DEPTH_BUFFER_BIT | GL_COLOR_BUFFER_BIT));

        GL_CHECK(glUseProgram(_program));

        GL_CHECK(glVertexAttribPointer(_positionHandle, 2, GL_FLOAT, GL_FALSE, 0, gSquareVertices));
        GL_CHECK(glEnableVertexAttribArray(_positionHandle));
        GL_CHECK(glDrawArrays(GL_TRIANGLES, 0, 6));

        _intervalTimer.reset();
        //glReadPixels(0, 0, _winWidth, _winHeight, GL_RGBA, GL_UNSIGNED_BYTE, &_buf[0]);
        float interval = _intervalTimer.getInterval();
        LOGI("glReadPixels time: %f, fps: %f\n", interval, _fpsTimer.getFPS());
    }

protected:
    PFNEGLSETDAMAGEREGIONKHRPROC eglSetDamageRegionKHR;
    GLuint _program;
    GLuint _positionHandle;
    std::vector<char> _buf;
    Timer _fpsTimer;
    Timer _intervalTimer;
    int testCaseNo;
};

int main() 
{
    App *app = new App();

    if (!app->initialize(0, 0, 1920, 1080)) {
        LOGE("App failed to initialize\n");
        return -1;
    }
    //app->setRenderMode(synchronized_to_vsync)
    while(true) {
        app->run();
    }
    app->release();
    
    return 0;
}

