package com.demo.gfxvideo;

import android.graphics.SurfaceTexture;

public class MySurfaceTexture extends SurfaceTexture {
	private Texture mTexture;
	private boolean mNeedUpdate;
	
	public MySurfaceTexture(Texture tex) {
		super(tex.getTexId());
		mTexture = tex;
		mNeedUpdate = false;
	}
	
	public Texture getTex() {
		return mTexture;
	}
	
	public int getTexId() {
		return mTexture.getTexId();
	}
	
	public void releas() {
		super.release();
		if(mTexture != null) {
			mTexture.release();
			mTexture = null;
		}
	}

	public boolean needUpdate() {
		return mNeedUpdate;
	}
	
	public void setUpdate(boolean update) {
		mNeedUpdate = update;
	}	
}
