package com.demo.gfxvideo;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.SurfaceTexture;
import android.graphics.Paint.Style;
import android.graphics.Rect;
import android.view.Surface;

public class CanvasDrawSurface implements SurfaceTextureStreamable {
	private MySurfaceTexture mSurfaceTexture;
	private Surface mSurface;

	public CanvasDrawSurface(SurfaceTexture.OnFrameAvailableListener listener) {
		Texture tex = new Texture(Texture.TEXTURE_EXTERNAL_OES);
		mSurfaceTexture = new MySurfaceTexture(tex);
		mSurfaceTexture.setDefaultBufferSize(640, 480);
		mSurfaceTexture.setOnFrameAvailableListener(listener);
		mSurface = new Surface(mSurfaceTexture);
	}

	public void setSize(int width, int height) {
		mSurfaceTexture.setDefaultBufferSize(width, height);
	}
	
	public void draw() {
		Canvas canvas = mSurface.lockCanvas(null);
		Paint paint = new Paint();
		paint.setColor(Color.WHITE);
		paint.setStyle(Style.FILL);
		paint.setTextSize(90);
		canvas.drawCircle(100, 100, 100, paint);
		canvas.drawRect(new Rect(300, 0, 500, 200), paint);
		canvas.scale(1.0f, 1.5f);
		canvas.rotate(-30, 0, 550);
		canvas.drawText("Drawn by Canvas", 0, 550, paint);		
		mSurface.unlockCanvasAndPost(canvas);
	}

	@Override
	public MySurfaceTexture getOutputSurfaceTexture() {
		return mSurfaceTexture;
	}
	
	@Override
	public boolean needUpdateTexImage() {
		return mSurfaceTexture.needUpdate();
	}

	@Override
	public void updateTexImage() {
		mSurfaceTexture.updateTexImage();		
	}

	@Override
	public void setUpdate(boolean update) {
		mSurfaceTexture.setUpdate(update);		
	}
	
	public void release() {
		if(mSurface != null) {
			mSurface.release();
			mSurface = null;
		}
		if (mSurfaceTexture != null) {
			mSurfaceTexture.getTex().release();
			mSurfaceTexture.release();
		}
		
	}

}
