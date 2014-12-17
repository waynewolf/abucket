package com.demo.gfxvideo;

import java.io.IOException;

import android.content.Context;
import android.content.res.AssetFileDescriptor;
import android.graphics.SurfaceTexture;
import android.media.MediaPlayer;
import android.util.Log;
import android.view.Surface;

/*
 * play video from asset file, into a SurfaceTexture
 */
public class BgVideoPlayer implements SurfaceTextureStreamable {
	private static final String TAG = "gfxvideo";
	private MySurfaceTexture mSurfaceTexture;
	private Context mContext;
	private MediaPlayer mMediaPlayer;
	
	private MediaPlayer.OnPreparedListener mOnPreparedListener =
			new MediaPlayer.OnPreparedListener() {
		@Override
		public void onPrepared(MediaPlayer mediaPlayer) {
			mediaPlayer.start();
		}
	};

	public BgVideoPlayer(Context context, SurfaceTexture.OnFrameAvailableListener listener) {
		mContext = context;
		Texture tex = new Texture(Texture.TEXTURE_EXTERNAL_OES);
		mSurfaceTexture = new MySurfaceTexture(tex);
		mSurfaceTexture.setOnFrameAvailableListener(listener);
		mMediaPlayer = new MediaPlayer();
	}
	
	public void play(String file) {
		Surface surface = new Surface(mSurfaceTexture);
		try {
			AssetFileDescriptor afd = mContext.getAssets().openFd(file);
			mMediaPlayer.setDataSource(afd.getFileDescriptor(), afd.getStartOffset(), afd.getLength());
			mMediaPlayer.setSurface(surface);
			mMediaPlayer.setLooping(true);
			mMediaPlayer.prepareAsync();
			mMediaPlayer.setOnPreparedListener(mOnPreparedListener);

		} catch (IllegalArgumentException e) {
			Log.d(TAG, e.getMessage());
		} catch (SecurityException e) {
			Log.d(TAG, e.getMessage());
		} catch (IllegalStateException e) {
			Log.d(TAG, e.getMessage());
		} catch (IOException e) {
			Log.d(TAG, e.getMessage());
		}
	}
	
	public void release() {
		if (mMediaPlayer != null) {
	        mMediaPlayer.stop();
	        mMediaPlayer.release();
	        mMediaPlayer = null;
	    }
		if (mSurfaceTexture != null) {
			mSurfaceTexture.getTex().release();
			mSurfaceTexture.release();
		}
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
}
