package com.demo.rgb565test;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.VectorDrawable;
import android.os.Build;
import android.os.Handler;
import android.support.v4.content.ContextCompat;
import android.util.AttributeSet;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.widget.Toast;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;

public class MySurfaceView extends SurfaceView implements SurfaceHolder.Callback {
    private static final String TAG = "rgb565test";
    static boolean dumpPNG = true;
    static Handler handler = new Handler();

    private Bitmap mGolden = null;
    private Bitmap mGoldenRGB565 = null;

    public MySurfaceView(Context context) {
        super(context);
        getHolder().addCallback(this);
        mGolden = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden);
        BitmapFactory.Options opts = new BitmapFactory.Options();
        opts.inPreferredConfig = Bitmap.Config.RGB_565;
        mGoldenRGB565 = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden, opts);
    }

    public MySurfaceView(Context context, AttributeSet attrs) {
        super(context, attrs);
        getHolder().addCallback(this);
        mGolden = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden);
        BitmapFactory.Options opts = new BitmapFactory.Options();
        opts.inPreferredConfig = Bitmap.Config.RGB_565;
        mGoldenRGB565 = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden, opts);

    }

    public MySurfaceView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        getHolder().addCallback(this);
        mGolden = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden);
        BitmapFactory.Options opts = new BitmapFactory.Options();
        opts.inPreferredConfig = Bitmap.Config.RGB_565;
        mGoldenRGB565 = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.vector_drawable_scale_golden, opts);
    }

    private void updateStatus(final String str) {
        handler.post(new Runnable(){
            public void run(){
                Toast.makeText(MySurfaceView.this.getContext(), str, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private Bitmap getBitmapFromDrawable(VectorDrawable vectorDrawable) {
        Bitmap bm = Bitmap.createBitmap(64, 64, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bm);
        canvas.drawColor(Color.WHITE);
        vectorDrawable.setBounds(0, 0, 64, 64);
        vectorDrawable.draw(canvas);
        return bm;
    }

    protected void myDraw(Canvas canvas) {
        super.draw(canvas);
        Drawable drawable = ContextCompat.getDrawable(getContext(), R.drawable.vector_icon_create);
        Bitmap icon = getBitmapFromDrawable((VectorDrawable)drawable);
        canvas.drawBitmap(icon, 0, 0, new Paint());
    }

    private Bitmap screenShot(SurfaceView surfaceView) {
        SyncPixelCopy copy = new SyncPixelCopy();
        Bitmap dstBitmap = Bitmap.createBitmap(64, 64, Bitmap.Config.ARGB_8888);

        int copyResult = copy.request(surfaceView, dstBitmap);
        Log.e(TAG, String.valueOf(copyResult));
        return dstBitmap;
    }

    private void saveGoldenBitmapARGBToRawData(Bitmap bitmap, String outputFolder, String filename) throws IOException {
        FileOutputStream out = null;
        try {
            File folder = new File (outputFolder);
            if (!folder.exists()) folder.mkdir();
            String outputFilename = outputFolder + "/" + filename + ".data";

            File outputFile = new File(outputFilename);
            if (!outputFile.exists()) outputFile.createNewFile();
            else outputFile.delete();

            out = new FileOutputStream(outputFile, false);

            int imageWidth = bitmap.getWidth();
            int imageHeight = bitmap.getHeight();

            Log.e(TAG, "golden width: " + imageWidth + ", height: " + imageHeight);
            for (int y = 0; y < imageHeight; y++) {
                for (int x = 0; x < imageWidth; x++) {
                    int pixel = bitmap.getPixel(x, y);
                    out.write(Color.blue(pixel));
                    out.write(Color.green(pixel));
                    out.write(Color.red(pixel));
                    out.write(Color.alpha(pixel));
                }
            }
            out.flush();

            Log.e(TAG, "Save golden bmp argb to "+ outputFilename + " successfully");

        } catch (Exception e) {
            Log.e(TAG, "Save golden bmp argb: " + e.getMessage());
        } finally {
            if (out != null) {
                out.close();
            }
        }
    }

    private void selfie() {

        final SurfaceView surfaceView = (SurfaceView) findViewById(R.id.surfaceView);

        if (android.os.Build.VERSION.SDK_INT <= Build.VERSION_CODES.N_MR1) {
            try {
                Thread.sleep(500);
            } catch (Exception e) {
                Log.e(TAG, e.getMessage());
            }
        }

        new Thread() {
            public void run() {
                try {
                    // sleep a bit to wait for rendering
                    if (android.os.Build.VERSION.SDK_INT > Build.VERSION_CODES.N_MR1) {
                        Thread.sleep(1000);

                        updateStatus("Selfie");

                        Thread.sleep(3000); // wait for 3 sec for the toast dismissing
                        Log.e(TAG, "Capturing");
                    }

                    Bitmap screenShotBitmap = screenShot(surfaceView);
                    if (mGolden == null)
                        Log.e(TAG, "golden is null");
                    if (screenShotBitmap == null)
                        Log.e(TAG, "captured bitmap is null");

                    DrawableTestUtils.compareImages("golden png decoded bmp888 vs bmp888 from surface", mGolden, screenShotBitmap,
                            DrawableTestUtils.PIXEL_ERROR_THRESHOLD,
                            DrawableTestUtils.PIXEL_ERROR_COUNT_THRESHOLD,
                            DrawableTestUtils.PIXEL_ERROR_TOLERANCE);
                    if (dumpPNG) {
                        String outputFolder = MySurfaceView.this.getContext().getExternalFilesDir(null).getAbsolutePath();
                        saveBitmapToPng(screenShotBitmap, outputFolder, "SurfaceView");
                        saveGoldenBitmapARGBToRawData(mGolden, outputFolder, "golden_bmp_argb");
                    }

                } catch (Exception e) {
                    Log.e(TAG, e.getMessage());
                }
            }
        }.start();

    }

    @Override
    public void surfaceCreated(SurfaceHolder holder) {
        Canvas canvas = null;
        try {
            canvas = holder.lockCanvas(null);
            synchronized (holder) {
                myDraw(canvas);
            }
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if (canvas != null) {
                holder.unlockCanvasAndPost(canvas);
                selfie();
            }
        }
    }

    @Override
    public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {

    }

    @Override
    public void surfaceDestroyed(SurfaceHolder holder) {

    }

    private void saveBitmapToPng(Bitmap bitmap, String outputFolder, String filename) throws IOException {
        FileOutputStream out = null;
        try {
            File folder = new File (outputFolder);
            if (!folder.exists()) folder.mkdir();
            String outputFilename = outputFolder + "/" + filename + ".png";

            File outputFile = new File(outputFilename);
            if (!outputFile.exists()) outputFile.createNewFile();
            else outputFile.delete();

            out = new FileOutputStream(outputFile, false);
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, out);
            Log.e(TAG, "Save to "+ outputFilename + " successfully");

        } catch (Exception e) {
            Log.e(TAG, "Save png: " + e.getMessage());
        } finally {
            if (out != null) {
                out.close();
            }
        }
    }
}
