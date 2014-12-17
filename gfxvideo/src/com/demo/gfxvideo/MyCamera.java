package com.demo.gfxvideo;

import java.io.IOException;

import android.graphics.SurfaceTexture;
import android.hardware.Camera;
import android.util.Log;

public class MyCamera implements SurfaceTextureStreamable {
	private static final String TAG = "gfxvideo";
	private MySurfaceTexture mSurfaceTexture;
	private Camera mCamera;
	
    private Camera.PreviewCallback mPreviewCallback = new Camera.PreviewCallback() {

		@Override
		public void onPreviewFrame(byte[] nv12, Camera camera) {
			//Log.d(TAG, "onPreviewFrame");
		}
    };
    
	public MyCamera(SurfaceTexture.OnFrameAvailableListener listener) {
		mCamera = Camera.open(1);
		Camera.Parameters param = mCamera.getParameters();
		Log.d(TAG, "width: " + param.getPreviewSize().width + 
				", height: " + param.getPreviewSize().height);
		Texture tex = new Texture(Texture.TEXTURE_EXTERNAL_OES);
		mSurfaceTexture = new MySurfaceTexture(tex);
		mSurfaceTexture.setOnFrameAvailableListener(listener);
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
	
	public void start() {
		mCamera.setPreviewCallback(mPreviewCallback);
		try {
			mCamera.setPreviewTexture(mSurfaceTexture);
			mCamera.startPreview();
		} catch (IOException e) {
			Log.e(TAG, ", " + e.getMessage());
		}
		
	}

	public void release() {
		if(mCamera != null) {
			mCamera.stopPreview();
			mCamera.setPreviewCallback(null);
			mCamera.release();
			mCamera = null;
		}		
		if (mSurfaceTexture != null) {
			mSurfaceTexture.getTex().release();
			mSurfaceTexture.release();
		}
	}

}
