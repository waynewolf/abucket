package com.demo.CameraSurfaceView;

import com.demo.CameraSurfaceView.R;

import android.app.Activity;
import android.graphics.Point;
import android.os.Bundle;
import android.view.Display;
import android.view.Gravity;
import android.widget.FrameLayout;

public class MainActivity extends Activity
{
	private FrameLayout mPreviewContainer;
	private CameraPreview mPreview;
	private int mWinWidth;
	private int mWinHeight;
	
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        Display display = getWindowManager().getDefaultDisplay();
        Point size = new Point();
        display.getSize(size);
        mWinWidth = size.x;
        mWinHeight = size.y;
    	
        mPreviewContainer = (FrameLayout)findViewById(R.id.preview_container);
    }

	@Override
	protected void onResume() {
		super.onResume();
		
        mPreview = new CameraPreview(this);
    	int viewSize = mWinWidth < mWinHeight ? mWinWidth - 100 : mWinHeight - 100;
    	mPreviewContainer.addView(mPreview, 0, new FrameLayout.LayoutParams(
    			mWinWidth - 100, mWinHeight - 200, Gravity.CENTER));
	}
    
    @Override
    protected void onPause() {
        super.onPause();
        mPreview.stop();
        mPreviewContainer.removeView(mPreview);
        mPreview = null;
    }
}
