package com.wnwolf.cameraui;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.PixelFormat;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.SurfaceView;

public class CameraDrawer extends SurfaceView {
	public Bitmap mBitmap = null;
	private Rect mBitmapSize = new Rect(0, 0, 0, 0);
	private Rect mWindowSize = new Rect(0, 0, 0, 0);

	public CameraDrawer(Context context) {
		super(context);
		init();
	}

	public CameraDrawer(Context context, AttributeSet attrs) {
		super(context, attrs);
		init();
	}

	public CameraDrawer(Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
		init();
	}

	private void init() {
		setWillNotDraw(false);
		getHolder().setFormat(PixelFormat.TRANSLUCENT);
	}

	@Override
	protected void onDraw(Canvas canvas) {
		if(mBitmap != null) {
			mBitmapSize.right = mBitmap.getWidth();
			mBitmapSize.bottom = mBitmap.getHeight();
			mWindowSize.right = getWidth();
			mWindowSize.bottom = getHeight();
			canvas.drawBitmap(mBitmap, mBitmapSize, mWindowSize, null);
		}
	}

	public void setNewBitmap(Bitmap bitmap) {
		mBitmap = bitmap;
		invalidate();
	}
}
