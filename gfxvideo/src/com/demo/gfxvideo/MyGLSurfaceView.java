package com.demo.gfxvideo;

import android.content.Context;
import android.opengl.GLSurfaceView;
import android.view.MotionEvent;
import android.view.SurfaceHolder;

public class MyGLSurfaceView extends GLSurfaceView {
	private MyGLRenderer mRenderer;

	public MyGLSurfaceView(Context context) {
		super(context);

		// Create an OpenGL ES 2.0 context.
		setEGLContextClientVersion(2);

		// Set the Renderer for drawing on the GLSurfaceView
		mRenderer = new MyGLRenderer(this);
		setRenderer(mRenderer);

		// Render the view only when there is a change in the drawing data
		setRenderMode(GLSurfaceView.RENDERMODE_WHEN_DIRTY);
	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		super.surfaceDestroyed(holder);
		mRenderer.close();
	}

	private final float TOUCH_SCALE_FACTOR = 180.0f / 320;
    private float mPreviousY;
    @Override
    public boolean onTouchEvent(MotionEvent e) {
        float y = e.getY();

        switch (e.getAction()) {
            case MotionEvent.ACTION_MOVE:
                float dy = y - mPreviousY;
                mRenderer.setXAngle(mRenderer.getXAngle() + dy* TOUCH_SCALE_FACTOR);
                requestRender();
        }

        mPreviousY = y;
        return true;
    }
}
