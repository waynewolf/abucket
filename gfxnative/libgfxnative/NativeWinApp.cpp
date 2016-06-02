#include <utils/Log.h>
#include <android/native_window.h>

#include <binder/IPCThreadState.h>
#include <binder/IServiceManager.h>

#include <Platform.h>
#include "utils.h"
#include "NativeWinApp.h"

using namespace MaliSDK;

NativeWinApp::NativeWinApp() :
    _winWidth(-1),
    _winHeight(-1),
	_processState(ProcessState::self()),
    _client(new SurfaceComposerClient())
{
    ProcessState::self()->startThreadPool();
}

NativeWinApp::~NativeWinApp()
{
}

bool NativeWinApp::initialize(int left, int top, int winWidth, int winHeight)
{
	bool ret = false;

    ANativeWindow* nativeWindow = createNativeWindow(left, top, winWidth, winHeight);
    if (nativeWindow == NULL) {
        LOGE("Error createNativeWindow\n");
        return false;
    }

	ret = initializeEGL(winWidth, winHeight, nativeWindow);
	if (!ret) return false;

    _winWidth = winWidth;
    _winHeight = winHeight;

    ret = setupGraphics();
	if (!ret) return false;

	return true;
}

bool NativeWinApp::initializeEGL(int winWidth, int winHeight, ANativeWindow* nativeWindow)
{
	EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (display == EGL_DEFAULT_DISPLAY) {
        LOGE("Unable to connect window system: %s\n", GLUtil::eglStatusStr());
        return false;
    }
    EGLint majorVersion, minorVersion;
    if (!eglInitialize(display, &majorVersion, &minorVersion)) {
        LOGE("Unable to initialize egl: %s\n", GLUtil::eglStatusStr());
        return false;
    }

    _eglConfig = chooseEGLConfig(display);
    _eglDisplay = display;

    _eglSurface = createEGLSurface(_eglDisplay, _eglConfig, nativeWindow);
    _eglContext = createEGLContext(_eglDisplay, _eglConfig);
    if (!bindEGLContext(_eglDisplay, _eglSurface, _eglSurface, _eglContext)) {
        LOGE("bind egl context\n");
        return false;
    }

    return postInitializeEGL(_eglDisplay, _eglSurface);
}

ANativeWindow* NativeWinApp::createNativeWindow(int left, int top, int w, int h)
{
    _surfaceControl = _client->createSurface(String8("native window"),
    	w, h, HAL_PIXEL_FORMAT_RGBA_8888, 0);
    if (_surfaceControl == NULL) {
        LOGE("Failed to get SurfaceControl\n");
        return NULL;
    }
    _nativeSurface = _surfaceControl->getSurface();
    if (_nativeSurface == NULL) {
        LOGE("Failed to get Surface\n");
        return NULL;
    }

    // make sure the surface is on top
    SurfaceComposerClient::openGlobalTransaction();
    _surfaceControl->setLayer(0xFFFFFE);
    _surfaceControl->setPosition(left, top);
    SurfaceComposerClient::closeGlobalTransaction();

	return _nativeSurface.get();
}

EGLSurface NativeWinApp::createEGLSurface(EGLDisplay display, EGLConfig config, ANativeWindow* nativeWindow)
{
    EGLSurface surface = eglCreateWindowSurface(display, config, nativeWindow, NULL);
    if (surface == EGL_NO_SURFACE) {
        LOGE("Unable to create surface: %s\n", GLUtil::eglStatusStr());
    }

    return surface;
}

EGLContext NativeWinApp::createEGLContext(EGLDisplay display, EGLConfig config)
{
	EGLint contextAttrs[] = { 
        EGL_CONTEXT_CLIENT_VERSION, 3,
        EGL_NONE
    };  

    EGLContext context = eglCreateContext(display, config, NULL, contextAttrs);
    if (context == EGL_NO_CONTEXT) {
        LOGE("Unable to create context: %s\n", GLUtil::eglStatusStr());
    }

    return context;
}

bool NativeWinApp::bindEGLContext(
	EGLDisplay display, EGLSurface drawSurface, EGLSurface readSurface, EGLContext context)
{
    if (eglMakeCurrent(display, drawSurface, readSurface, context) == EGL_FALSE) {
        LOGE("Unable to eglMakeCurrent\n");
        return false;
    }
    return true;
}

bool NativeWinApp::postInitializeEGL(EGLDisplay display, EGLSurface surface)
{
    return true;
}

void NativeWinApp::startFrame()
{
}

bool NativeWinApp::getNativeWindowSize(int *winWidth, int *winHeight)
{
	if (_eglDisplay == EGL_NO_DISPLAY || _eglSurface == EGL_NO_SURFACE)
		return false;
    if (_winWidth != -1 && _winHeight != -1) {
        *winWidth = _winWidth;
        *winHeight = _winHeight;
        return true;
    }
    eglQuerySurface(_eglDisplay, _eglSurface, EGL_WIDTH, winWidth);
    eglQuerySurface(_eglDisplay, _eglSurface, EGL_HEIGHT, winHeight);
    return true;
}

EGLConfig NativeWinApp::chooseEGLConfig(EGLDisplay display)
{
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
        LOGE("Unable to choose egl config: %s\n", GLUtil::eglStatusStr());
        goto exit;
    }
    
    if (numConfigs < 1) {
    	LOGI("eglChooseConfig return %d configs\n", numConfigs);
    	goto exit;
    }

    EGLint format, redSize, greenSize, blueSize, alphaSize;
    eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &format);
    eglGetConfigAttrib(display, config, EGL_RED_SIZE, &redSize);
    eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &greenSize);
    eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &blueSize);
    eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &alphaSize);
    LOGI("eglGetConfigAttrib format: %d, r: %d, g: %d, b: %d, a: %d\n",
        format, redSize, greenSize, blueSize, alphaSize);
    //ANativeWindow_setBuffersGeometry(nativeWindow, 0, 0, format);

exit:
    return config;
}

void NativeWinApp::release()
{
	IPCThreadState::self()->joinThreadPool();
	delete this;
}

void NativeWinApp::run()
{
    startFrame();
    renderFrame();
    eglSwapBuffers(_eglDisplay, _eglSurface);
}
