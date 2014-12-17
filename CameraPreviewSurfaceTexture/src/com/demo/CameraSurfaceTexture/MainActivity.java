package com.demo.CameraSurfaceTexture;

import java.io.IOException;

import com.demo.CameraSurfaceTexture.R;

import android.app.Activity;
import android.graphics.Point;
import android.graphics.SurfaceTexture;
import android.hardware.Camera;
import android.os.Bundle;
import android.util.Log;
import android.view.Display;
import android.view.Gravity;
import android.view.TextureView;
import android.widget.FrameLayout;

public class MainActivity extends Activity implements TextureView.SurfaceTextureListener
{
	private static final String TAG = "CameraPreview";
    private Camera mCamera;
    private TextureView mTextureView;
    private int mWinWidth;
    private int mWinHeight;
    private FrameLayout mTextureViewContainer;
    
    private Camera.PreviewCallback mPreviewCallback = new Camera.PreviewCallback() {

		@Override
		public void onPreviewFrame(byte[] nv12, Camera camera) {
			//Log.d(TAG, "onPreviewFrame");			
		}
    };
    
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        mTextureView = new TextureView(this);
        mTextureView.setSurfaceTextureListener(this);
        Display display = getWindowManager().getDefaultDisplay();
        Point size = new Point();
        display.getSize(size);
        mWinWidth = size.x;
        mWinHeight = size.y;
    	
    	mTextureViewContainer = (FrameLayout)findViewById(R.id.texture_view_container);
    	
    	int viewSize = mWinWidth < mWinHeight ? mWinWidth - 100 : mWinHeight - 100;
    	mTextureViewContainer.addView(mTextureView, 0, new FrameLayout.LayoutParams(
    			viewSize, viewSize, Gravity.CENTER));
    }

    @Override
    protected void onDestroy() {
    	super.onDestroy();
		if (mTextureView != null) {
			mTextureViewContainer.removeView(mTextureView);
			mTextureView = null;
		}
	}

	@Override
	public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
		int numCameras = Camera.getNumberOfCameras();
		Log.d(TAG, numCameras + " camera(s) found");
		mCamera = Camera.open(numCameras - 1);
		mCamera.setPreviewCallback(mPreviewCallback);
		
		try {
			mCamera.setPreviewTexture(surface);
			mCamera.startPreview();
		} catch (IOException e) {
			Log.e(TAG, ", " + e.getMessage());
		}
	}

	@Override
	public boolean onSurfaceTextureDestroyed(SurfaceTexture surface) {
		mCamera.setPreviewCallback(null);
		mCamera.stopPreview();
		mCamera.release();
		return true;
	}

	@Override
	public void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {
		// Ignore, Camera does all the work for us
	}

	@Override
	public void onSurfaceTextureUpdated(SurfaceTexture surface) {
		// Invoked every time there's a new Camera preview frame
		//Log.d(TAG, "onSurfaceTextureUpdated");
	}
}
