package com.demo.gfxvideo;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

import android.graphics.SurfaceTexture;
import android.graphics.SurfaceTexture.OnFrameAvailableListener;
import android.opengl.GLES20;
import android.opengl.GLSurfaceView;
import android.opengl.Matrix;
import android.os.SystemClock;
import android.util.Log;

public class MyGLRenderer implements GLSurfaceView.Renderer, OnFrameAvailableListener {
	private static final String TAG = "gfxvideo";
	private MyGLSurfaceView mView;
	private Cube mCube;
	private Square[] mSquares;
	private MyCamera mCamera;
	private CanvasDrawSurface mCanvasDrawSurface;
	private BgVideoPlayer mVideoPlayer;
	
	private final float[] mMVPMatrix = new float[16];
	private final float[] mProjectionMatrix = new float[16];
	private final float[] mViewMatrix = new float[16];
	private final float[] mRotationYMatrix = new float[16];
	private final float[] mRotationXMatrix = new float[16];
	private float mXAngle;
	
	public MyGLRenderer(MyGLSurfaceView view) {
		mView = view;
	}

	@Override
	public void onSurfaceCreated(GL10 unused, EGLConfig config) {
		Log.d(TAG, "onSurfaceCreated");
		GLES20.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
		GLES20.glFrontFace(GLES20.GL_CCW);
		
		// allocate objects
		mCamera = new MyCamera(this);
		
		mCanvasDrawSurface = new CanvasDrawSurface(this);
		mVideoPlayer = new BgVideoPlayer(mView.getContext(), this);
		
		// attach surface texture to different producers
		mCube = new Cube(mCamera.getOutputSurfaceTexture());
		
		mSquares = new Square[6];
		mSquares[0] = new Square(mCanvasDrawSurface.getOutputSurfaceTexture(), Square.FACE_FRONT);
		mSquares[1] = new Square(mVideoPlayer.getOutputSurfaceTexture(), Square.FACE_BACK);
		mSquares[2] = new Square(mCamera.getOutputSurfaceTexture(), Square.FACE_LEFT);
		mSquares[3] = new Square(mCamera.getOutputSurfaceTexture(), Square.FACE_RIGHT);
		mSquares[4] = new Square(mCamera.getOutputSurfaceTexture(), Square.FACE_UP);
		mSquares[5] = new Square(mCamera.getOutputSurfaceTexture(), Square.FACE_DOWN);
		
		mCamera.start();
		mVideoPlayer.play("bbb.mp4");
	}

	@Override
	public void onSurfaceChanged(GL10 unused, int width, int height) {
		mCanvasDrawSurface.setSize(width, height);
		GLES20.glViewport(0, 0, width, height);
		float ratio = (float) width / height;
		Matrix.frustumM(mProjectionMatrix, 0, -ratio, ratio, -1, 1, 3, 10);
	}
	
	public void close() {
		if (mCamera != null) {
			mCamera.release();
			mCamera = null;
		}
		
		if (mCanvasDrawSurface != null) {
			mCanvasDrawSurface.release();
			mCanvasDrawSurface = null;
		}
		
		if (mVideoPlayer != null) {
			mVideoPlayer.release();
			mVideoPlayer = null;
		}
	}
	
	@Override
	public void onDrawFrame(GL10 unused) {
		if(mCamera.needUpdateTexImage()){
			mCamera.updateTexImage();
			mCamera.setUpdate(false);
		}
		if(mCanvasDrawSurface.needUpdateTexImage()){
			mCanvasDrawSurface.updateTexImage();
			mCanvasDrawSurface.setUpdate(false);
		}
		if(mVideoPlayer.needUpdateTexImage()){
			mVideoPlayer.updateTexImage();
			mVideoPlayer.setUpdate(false);
		}
		// Draw background color
		GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT | GLES20.GL_DEPTH_BUFFER_BIT);

		// Set the camera position (View matrix)
		Matrix.setLookAtM(mViewMatrix, 0, 0, 2, 5, 0f, 0f, 0f, 0f, 1.0f, 0.0f);
		
		float[] vpMatrix = new float[16];

		// Calculate the projection and view transformation
		Matrix.multiplyMM(vpMatrix, 0, mProjectionMatrix, 0, mViewMatrix, 0);

		long time = SystemClock.uptimeMillis() % 4000L;
		float yAngle = 0.090f * ((int)time);
		Matrix.setRotateM(mRotationYMatrix, 0, yAngle, 0, 1.0f, 0);
		Matrix.setRotateM(mRotationXMatrix, 0, mXAngle, 1.0f, 0, 0);
		float[] rotMatrix = new float[16];
		Matrix.multiplyMM(rotMatrix, 0, mRotationXMatrix, 0, mRotationYMatrix, 0);
		
		Matrix.multiplyMM(mMVPMatrix, 0, vpMatrix, 0, rotMatrix, 0);
		
		// Draw cube with an offset
		for(int i=0; i<6; i++)
			mSquares[i].draw(mMVPMatrix);

		float[] translateMatrix = new float[16];
		Matrix.setIdentityM(translateMatrix, 0);
		Matrix.translateM(translateMatrix, 0, 0.5f, 0.5f, -5.0f);
		
		float[] modelMatrix = new float[16];
		Matrix.multiplyMM(modelMatrix, 0, translateMatrix, 0, rotMatrix, 0);
		
		Matrix.multiplyMM(mMVPMatrix, 0, vpMatrix, 0, modelMatrix, 0);
		
		// Draw cube
		mCube.draw(mMVPMatrix);
		
		// Draw Surface with Canvas
		mCanvasDrawSurface.draw();
	}

	public static int loadShader(int type, String shaderCode) {
		int shader = GLES20.glCreateShader(type);

		GLES20.glShaderSource(shader, shaderCode);
		GLES20.glCompileShader(shader);

		return shader;
	}

	public static void checkGlError(String glOperation) {
		int error;
		while ((error = GLES20.glGetError()) != GLES20.GL_NO_ERROR) {
			Log.e(TAG, glOperation + ": glError " + error);
			throw new RuntimeException(glOperation + ": glError " + error);
		}
	}

	@Override
	public void onFrameAvailable(SurfaceTexture surfaceTexture) {
		((MySurfaceTexture)surfaceTexture).setUpdate(true);
		mView.requestRender();
	}

	public float getXAngle() {
		return mXAngle;
	}
	
	public void setXAngle(float angle) {
		mXAngle = angle;
	}
}
