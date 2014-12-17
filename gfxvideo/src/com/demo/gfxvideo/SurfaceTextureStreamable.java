package com.demo.gfxvideo;

public interface SurfaceTextureStreamable {
	MySurfaceTexture getOutputSurfaceTexture();
	boolean needUpdateTexImage();
	void updateTexImage();
	void setUpdate(boolean update);
}
