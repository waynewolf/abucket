package com.demo;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.ActivityManager.RunningServiceInfo;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

public class MenuActivity extends Activity implements View.OnClickListener
{
	private static final String TAG = "TransPhone";
	boolean cameraStarted = false;
    Button startCameraButton = null;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
		Log.d(TAG, "MenuActivity.onCreate()");
        super.onCreate(savedInstanceState);
        setContentView(R.layout.menu_activity);
        startCameraButton = (Button)findViewById(R.id.button1);
        startCameraButton.setOnClickListener(this);
    }

	@Override
	protected void onResume() {
		Log.d(TAG, "MenuActivity.onResume()");
		super.onResume();

		cameraStarted = isServiceRunning(CameraService.class);
		if(cameraStarted)
			startCameraButton.setText("Stop Camera");
		else
			startCameraButton.setText("Start Camera");
	}

	@Override
	protected void onDestroy() {
		Log.d(TAG, "MenuActivity.onDestroy()");
		super.onDestroy();
	}

	private boolean isServiceRunning(Class<?> serviceClass) {
	    ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
	    for (RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
	        if (serviceClass.getName().equals(service.service.getClassName())) {
	            return true;
	        }
	    }
	    return false;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		    case R.id.button1:
				if(!cameraStarted) {
					if(startService(new Intent(this, CameraService.class)) != null) {
						cameraStarted = true;
					startCameraButton.setText("Stop Camera");
					} else {
						Log.w(TAG, "start camera service failed");
					}
				} else {
					boolean stopSuccess = stopService(new Intent(this, CameraService.class));
					if(stopSuccess){
						cameraStarted = false;
						startCameraButton.setText("Start Camera");
					} else {
						Log.w(TAG, "stop camera service failed");
					}
				}
				break;
			default:
				break;
		}
	}

}
