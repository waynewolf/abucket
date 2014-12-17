package com.demo.gfxvideo;

import java.io.IOException;
import java.io.InputStream;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.opengl.ETC1Util;
import android.opengl.GLES11Ext;
import android.opengl.GLES20;
import android.opengl.GLUtils;
import android.util.Log;

public class Texture {
	private final static String TAG = "gfxvideo";

	public static final int TEXTURE_2D = 0x1;
	public static final int TEXTURE_EXTERNAL_OES = 0x2;

	private int mTexId;
	private int mType;
	public Texture(int type) {
		int[] textures = new int[1];
		GLES20.glGenTextures(1, textures, 0);
		mTexId = textures[0];

		mType = type == TEXTURE_2D ? GLES20.GL_TEXTURE_2D
				: GLES11Ext.GL_TEXTURE_EXTERNAL_OES;
		GLES20.glBindTexture(mType, mTexId);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
	}
	
	public Texture() {
		int[] textures = new int[1];
		GLES20.glGenTextures(1, textures, 0);
		mTexId = textures[0];

		mType = GLES20.GL_TEXTURE_2D;
		GLES20.glBindTexture(mType, mTexId);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
		GLES20.glTexParameterf(mType, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
	}
	
	public int getTexId() {
		return mTexId;
	}
	
	public void bind(int texUnit) {
		GLES20.glActiveTexture(texUnit);
		GLES20.glBindTexture(mType, mTexId);
	}
	
	public void unbind() {
		GLES20.glBindTexture(mType, 0);
	}
	
	public void release() {
		Texture.freeTexId(mTexId);
	}
	
	public static int getTexId(int type) {
		int[] textures = new int[1];
		GLES20.glGenTextures(1, textures, 0);
		int textureId = textures[0];

		int t = type == TEXTURE_2D ? GLES20.GL_TEXTURE_2D
				: GLES11Ext.GL_TEXTURE_EXTERNAL_OES;
		GLES20.glBindTexture(t, textureId);
		GLES20.glTexParameterf(t, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(t, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
		GLES20.glTexParameterf(t, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
		GLES20.glTexParameterf(t, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
		
		return textureId;
	}
	
	public static void freeTexId(int texId) {
		int[] textures = {texId};
		GLES20.glDeleteTextures(1, textures, 0);
	}
	
	public static int loadETC1(Context context, int resId) {

		int textureId = getTexId(TEXTURE_2D);

		Log.w(TAG, "ETC1 texture support: " + ETC1Util.isETC1Supported());
		InputStream inputStream = context.getResources().openRawResource(resId);
		try {
			ETC1Util.loadTexture(GLES20.GL_TEXTURE_2D, 0, 0, GLES20.GL_RGB,
					GLES20.GL_UNSIGNED_SHORT_5_6_5, inputStream);
		} catch (IOException e) {
			Log.w(TAG, "Could not load etc1 texture");
		}

		try {
			inputStream.close();
		} catch (IOException e) {

		}
		return textureId;
	}

	public static int loadBMP(Context context, int resId) {
		int textureId = getTexId(TEXTURE_2D);

		InputStream inputStream = context.getResources().openRawResource(resId);
		Bitmap bitmap;
		try {
			bitmap = BitmapFactory.decodeStream(inputStream);
			GLUtils.texImage2D(GLES20.GL_TEXTURE_2D, 0, bitmap, 0);
			bitmap.recycle();
		} catch (Exception e) {
			Log.w(TAG, "Could not load bmp texture");
			return 0;
		}

		try {
			inputStream.close();
		} catch (IOException e) {

		}

		return textureId;
	}
}
