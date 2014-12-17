package com.demo.CameraSurfaceView;

import android.content.Context;
import android.hardware.Camera;
import android.hardware.Camera.CameraInfo;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

public class CameraPreview extends SurfaceView implements SurfaceHolder.Callback {

	private Camera mCamera;

	private Camera.PreviewCallback mPreviewCallback = new Camera.PreviewCallback() {
		
		@Override
		public void onPreviewFrame(byte[] data, Camera camera) {
			Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
			Camera.getCameraInfo(0, cameraInfo);
		}
	};
	
	public CameraPreview(Context context) {
		super(context);
		
		mCamera = Camera.open(1);
		getHolder().addCallback(this);
		getHolder().setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
		try {
			mCamera.setPreviewCallback(mPreviewCallback);
			mCamera.setPreviewDisplay(holder);
			mCamera.startPreview();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public void surfaceCreated(SurfaceHolder holder) {

	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		
	}

	public void stop() {
		mCamera.stopPreview();
		mCamera.release();
		mCamera = null;
	}
}
