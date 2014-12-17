package com.demo;

import java.util.Iterator;
import java.util.List;

import android.content.Context;
import android.graphics.ImageFormat;
import android.hardware.Camera;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

public class CameraPreview extends SurfaceView implements SurfaceHolder.Callback{
	private static final String TAG = "TransPhone";
	private Camera mCamera = null;
	private byte[] mData = null;
	private int mWidth, mHeight;
	private Context mContext;

	public CameraPreview(Context context, Camera camera) {
		super(context);
		Log.d(TAG, "CameraPreview.CameraPreview");

		mContext = context;
		mCamera = camera;
		getHolder().addCallback(this);
		getHolder().setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
	}

	private void addCallbackBuffer() {
		Camera.Parameters cameraParams = mCamera.getParameters();
		Camera.Size cameraSize = cameraParams.getPreviewSize();
		if(cameraSize.width != mWidth || cameraSize.height != mHeight) {
			mWidth = cameraSize.width;
			mHeight = cameraSize.height;
			int format = cameraParams.getPreviewFormat();
			Log.d(TAG, "CameraPreview.addCallbackBuffer, format: " + format);
			float bytesPerPixel = ImageFormat.getBitsPerPixel(format) / 8.0F;
			Log.d(TAG, "CameraPreview.addCallbackBuffer, mWidth: " + mWidth + 
					", mHeight: " + mHeight + ", BPP: " + bytesPerPixel);
			mData = new byte[(int)(mWidth * mHeight * bytesPerPixel)];
			doAddCallbackBuffer();
		}
	}

	public void doAddCallbackBuffer() {
		if(mCamera != null && mData != null) {
			mCamera.addCallbackBuffer(mData);
		}
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
		Log.d(TAG, "CameraPreview.surfaceChanged");

        if (holder.getSurface() == null){
          Log.d(TAG, "CameraPreview: preview surface does not exist");
          return;
        }

        // stop preview before making changes
        try {
            mCamera.stopPreview();
        } catch (Exception e){
          Log.d(TAG, "CameraPreview: " + e.getMessage());
        }

		Camera.Parameters cameraParams = mCamera.getParameters();
		List<Camera.Size> previewSizeList = cameraParams.getSupportedPreviewSizes();
		Iterator<Camera.Size> iter = previewSizeList.iterator();
		Camera.Size prefSize = previewSizeList.get(0);
		while(iter.hasNext()) {
			prefSize = iter.next();
			Log.i(TAG, "CameraPreview: Supported preview size: (" + prefSize.width + "x" + prefSize.height + ")");
		}
		// choose the last one
		cameraParams.setPreviewSize(720, 480);
		mCamera.setParameters(cameraParams);

        // start preview with new settings
        try {
			mCamera.setPreviewCallbackWithBuffer(((CameraService)mContext).getCameraPreviewCallback());
            mCamera.setPreviewDisplay(holder);
            mCamera.startPreview();
            addCallbackBuffer();
        } catch (Exception e){
            Log.d(TAG, "Error starting camera preview: " + e.getMessage());
        }
	}

	@Override
	public void surfaceCreated(SurfaceHolder holder) {
		Log.d(TAG, "CameraPreview.surfaceCreated");
	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		Log.d(TAG, "CameraPreview.surfaceDestroyed");
		mData = null;
	}
}
