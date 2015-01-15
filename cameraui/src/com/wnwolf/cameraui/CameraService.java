package com.wnwolf.cameraui;

import com.wnwolf.cameraui.R;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.PixelFormat;
import android.hardware.Camera;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.view.LayoutInflater;
import android.widget.RelativeLayout;

public class CameraService extends Service {
	private static final String TAG = "CameraUI";

    private NotificationManager mNM;
    private int NOTIFICATION = 0xaabbcc;

    private WindowManager mWindowManager;
    private Camera mCamera;
    private CameraPreview mPreview;
    private CameraDrawer mDrawer;

	private RelativeLayout mLayout;

	private Camera.PreviewCallback mPreviewCallback = new Camera.PreviewCallback() {
		@Override
		public void onPreviewFrame(byte[] data, Camera camera) {
			int width = camera.getParameters().getPreviewSize().width;
			int height = camera.getParameters().getPreviewSize().height;
			//int stride = camera.getParameters().getPreviewSize().?;

	        try {
				int[] pixels = convertYUV420_NV21toRGB8888(data, width, height);
				//rotate the buffer, because I found in portrait mode, default is 90 degree rotated
				int[] rotatedPixels = rotate90(pixels, width, height);
				int newWidth = height;
				int newHeight = width;
				Bitmap bitmap = Bitmap.createBitmap(newWidth, newHeight, Bitmap.Config.ARGB_8888);
				bitmap.setPixels(rotatedPixels, 0, newWidth, 0, 0, newWidth, newHeight);
				mDrawer.setNewBitmap(bitmap);

			} catch (Exception e) {
				e.printStackTrace();
			}

			// Attention: if not call this function, callback only get called once
			mPreview.doAddCallbackBuffer();
		}

		private int[] rotate90(int[] pixels, int width, int height) {
			int[] rotated = new int[width * height];

			for(int i = 0; i < height; ++i) {
				for(int j = 0; j < width; ++j) {
					int ii = j;
					int jj = height - i - 1;
					rotated[ii * height + jj] = pixels[i * width + j];
				}
			}

			return rotated;
		}

	    public int[] convertYUV420_NV21toRGB8888(byte[] data, int width, int height) {
	        int size = width*height;
	        int offset = size;
	        int[] pixels = new int[size];
	        int u, v, y1, y2, y3, y4;

	        byte alphaComFromUI = (byte)200;
	        byte realAlpha = (byte)(alphaComFromUI % 255);

	        // i: index of Y plane and final pixels
	        // k: index of UV plane of original pixels
	        for(int i=0, k=0; i < size; i+=2, k+=2) {
	            y1 = data[i  ]&0xff;
	            y2 = data[i+1]&0xff;
	            y3 = data[width+i  ]&0xff;
	            y4 = data[width+i+1]&0xff;

	            u = data[offset+k  ]&0xff;
	            v = data[offset+k+1]&0xff;
	            u = u-128;
	            v = v-128;

				pixels[i  ] = convertYUVtoRGB_FormulaFromSpec(y1, u, v);
				pixels[i+1] = convertYUVtoRGB_FormulaFromSpec(y2, u, v);
				pixels[width+i  ] = convertYUVtoRGB_FormulaFromSpec(y3, u, v);
				pixels[width+i+1] = convertYUVtoRGB_FormulaFromSpec(y4, u, v);

	            if (i != 0 && (i+2) % width==0)
	                i += width;
	        }

	        return pixels;
	    }

	    private int convertYUVtoRGB_FormulaFromSpec(int y, int u, int v) {
	        int r,g,b;

	        r = y + (int)1.402f*v;
	        g = y - (int)(0.344f*u +0.714f*v);
	        b = y + (int)1.772f*u;
	        r = r>255? 255 : r<0 ? 0 : r;
	        g = g>255? 255 : g<0 ? 0 : g;
	        b = b>255? 255 : b<0 ? 0 : b;
	        return 0x88000000 | (b<<16) | (g<<8) | r;
	    }

	};

    public Camera.PreviewCallback getCameraPreviewCallback() {
		return this.mPreviewCallback;
    }

    public CameraDrawer getCameraDrawer() {
		return this.mDrawer;
	}

    @Override
    public void onCreate() {
		Log.d(TAG, "CameraService.onCreate");

		mWindowManager = (WindowManager)getSystemService(Context.WINDOW_SERVICE);
		if(mWindowManager == null){
			Log.e(TAG, "Cannot get window manager");
			stopSelf();
			return;
		}

		WindowManager.LayoutParams layoutParams = new WindowManager.LayoutParams(
				ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT,
				WindowManager.LayoutParams.TYPE_SYSTEM_OVERLAY,
				WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE | WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN,
				PixelFormat.TRANSLUCENT);

		Log.d(TAG, "CameraSerivce.onCreate flating");
		mLayout = (RelativeLayout)(
					(LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE))
						.inflate(R.layout.cameraui_activity, null);
		mCamera = Camera.open(0);
		if(mCamera == null) {
			Log.e(TAG, "CameraSerivce: Cannot open camera");
			stopSelf();
			return;
		}
		mDrawer = (CameraDrawer)mLayout.findViewById(R.id.camera_drawer);
		mCamera.setPreviewCallbackWithBuffer(mPreviewCallback);

		mPreview = new CameraPreview(this, mCamera);
		mLayout.addView(mPreview, 0, new RelativeLayout.LayoutParams(1, 1));
		mWindowManager.addView(mLayout, layoutParams);

		mNM = (NotificationManager)getSystemService(NOTIFICATION_SERVICE);
		showNotification();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d(TAG, "CameraService.onStartCommand, received start id " + startId + ": " + intent);
        return START_STICKY;
    }

    @Override
    public void onDestroy() {
		Log.d(TAG, "CameraService.onDestroy");
		if(mCamera != null) {
			mCamera.stopPreview();
			mCamera.setPreviewCallbackWithBuffer(null);
			mCamera.release();
			mCamera = null;
        }
        mLayout.removeView(mPreview);
        mWindowManager.removeView(mLayout);
        mNM.cancel(NOTIFICATION);
    }

    private void showNotification() {
		// Make notification launch the activity just like home launcher,
		// in order to avoid creating multiple activity
		Intent launchMenuActivity = new Intent(this, MenuActivity.class);
		launchMenuActivity.setAction(Intent.ACTION_MAIN);
		launchMenuActivity.addCategory(Intent.CATEGORY_LAUNCHER);

		PendingIntent pendingIntent = PendingIntent.getActivity(
				this, 0,
				launchMenuActivity,
				PendingIntent.FLAG_UPDATE_CURRENT);

		Notification notification = new Notification.Builder(this)
			.setContentTitle("Camera UI")
			.setContentText("Click to control the camera")
			.setSmallIcon(R.drawable.ic_launcher)
			.setLargeIcon(null)
			.setContentIntent(pendingIntent)
			.build();

		mNM.notify(NOTIFICATION, notification);
	}

    private final IBinder mBinder = new Binder(){
		@SuppressWarnings("unused")
		CameraService getService() {
			return CameraService.this;
		}
	};

	@Override
	public IBinder onBind(Intent intent) {
		Log.d(TAG, "CameraService.onBind");
		return mBinder;
	}

}
