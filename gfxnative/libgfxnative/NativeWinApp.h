#ifndef NATIVEWINAPP_H
#define NATIVEWINAPP_H

#include <binder/ProcessState.h>
#include <gui/Surface.h>
#include <gui/SurfaceComposerClient.h>
#include <EGL/egl.h>

using namespace android;

class NativeWinApp
{
public:
	NativeWinApp();
	virtual ~NativeWinApp();

	ANativeWindow* createNativeWindow(int left, int top, int w, int h);
	bool bindEGLContext(
		EGLDisplay display, EGLSurface drawSurface, EGLSurface readSurface, EGLContext context);
	bool initialize(int left, int top, int winWidth, int winHeight);
	void release();
	void run();
	bool getNativeWindowSize(int *winWidth, int *winHeight);

	EGLDisplay getEGLDisplay() {return _eglDisplay;};
	EGLSurface getEGLSurface() {return _eglSurface;};
	EGLContext getEGLContext() {return _eglContext;};
	EGLConfig getEGLConfig() {return _eglContext;};

protected:
	virtual EGLConfig chooseEGLConfig(EGLDisplay display);
	
	// implementing one time GL related initialization code
	virtual bool setupGraphics() = 0;

	// called before each frame rendering
	virtual void startFrame();
	virtual void renderFrame() = 0;

	// implementing one time EGL related intialization code
	virtual EGLContext createEGLContext(EGLDisplay display, EGLConfig config);
	virtual EGLSurface createEGLSurface(EGLDisplay display, EGLConfig config, ANativeWindow* nativeWindow);
	virtual bool postInitializeEGL(EGLDisplay display, EGLSurface surface);

	bool initializeEGL(int winWidth, int winHeight, ANativeWindow* nativeWindow);

protected:
	EGLDisplay _eglDisplay;
	EGLSurface _eglSurface;
	EGLContext _eglContext;
	EGLConfig _eglConfig;

	int _winWidth;
	int _winHeight;

	sp<ProcessState> _processState;
	sp<SurfaceComposerClient> _client;            // must live in the lifecycle of NativeApp
	sp<Surface> _nativeSurface;                   // must live in the lifecycle of ANativeWindow
	sp<SurfaceControl> _surfaceControl;           // must live in the lifecycle of ANativeWindow
};

#endif //NATIVEWINAPP_H