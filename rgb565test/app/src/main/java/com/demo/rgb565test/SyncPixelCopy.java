package com.demo.rgb565test;

import android.graphics.Bitmap;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.view.PixelCopy;
import android.view.PixelCopy.OnPixelCopyFinishedListener;
import android.view.Surface;
import android.view.SurfaceView;

public class SyncPixelCopy implements OnPixelCopyFinishedListener {
    private static final String TAG = "rgb565test";
    private static Handler sHandler;
    static {
        HandlerThread thread = new HandlerThread("PixelCopyHelper");
        thread.start();
        sHandler = new Handler(thread.getLooper());
    }
    private int mStatus = -1;

    public int request(Surface source, Bitmap dest) {
        synchronized (this) {
            PixelCopy.request(source, dest, this, sHandler);
            return getResultLocked();
        }
    }

    public int request(SurfaceView source, Bitmap dest) {
        synchronized (this) {
            PixelCopy.request(source, dest, this, sHandler);
            return getResultLocked();
        }
    }

    private int getResultLocked() {
        try {
            this.wait(1000);
        } catch (InterruptedException e) {
            Log.e(TAG,"PixelCopy request didn't complete within 1s");
        }
        Log.v(TAG, String.valueOf(mStatus));
        return mStatus;
    }

    @Override
    public void onPixelCopyFinished(int copyResult) {
        synchronized (this) {
            mStatus = copyResult;
            this.notify();
        }
    }
}