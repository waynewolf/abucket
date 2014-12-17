package com.demo.gfxvideo;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;

import android.opengl.GLES11Ext;
import android.opengl.GLES20;

public class Cube {

	private final String vertexShaderCode =
		"uniform mat4 uMVPMatrix;\n" +
		"attribute vec3 aPosition;\n" +
		"attribute vec2 aUV;\n" +
		"varying vec2 vUV;\n" + 
		"void main() {\n" +
		"	gl_Position = uMVPMatrix * vec4(aPosition, 1.0);\n" +
		"	vUV = aUV;\n" +
		"}";

	private final String fragmentShaderCode =
		"#extension GL_OES_EGL_image_external : require\n" +
		"precision mediump float;\n" +
		"uniform samplerExternalOES sTex;\n" +
		"varying vec2 vUV;\n" +
		"void main() {\n" +
		"	gl_FragColor = texture2D(sTex, vUV);\n" +
		"}";

	private final FloatBuffer mVertexBuffer;
	private int mVBO;
	private final int mProgram;
	private int mPositionHandle;
	private int mUVHandle;
	private int mTexHandle;
	private int mMVPMatrixHandle;
	private Texture mTexture;
	private static final int POS_PER_VERTEX = 3;
	private static final int UV_PER_VERTEX = 2;
	private static final int COORDS_PER_VERTEX = POS_PER_VERTEX + UV_PER_VERTEX;

	static float vertexArray[] = {
		// -0.5f,  0.5f,  0.5f, // 0 - front
		// -0.5f, -0.5f,  0.5f, // 1 - front
		//  0.5f, -0.5f,  0.5f, // 2 - front
		//  0.5f,  0.5f,  0.5f, // 3 - front
		// -0.5f,  0.5f, -0.5f, // 4 - back
		// -0.5f, -0.5f, -0.5f, // 5 - back
		//  0.5f, -0.5f, -0.5f, // 6 - back
		//  0.5f,  0.5f, -0.5f, // 7 - back
		
		// 0, 1, 2 - front
		-0.5f, 0.5f, 0.5f, 0.25f, 0.25f,
		-0.5f,-0.5f, 0.5f, 0.25f, 0.5f,
		 0.5f,-0.5f, 0.5f, 0.5f,  0.5f,
		
		// 0, 2, 3 - front
		-0.5f, 0.5f, 0.5f, 0.25f, 0.25f,
		 0.5f,-0.5f, 0.5f, 0.5f,  0.5f,
		 0.5f, 0.5f, 0.5f, 0.5f,  0.25f,
		
		// 3, 2, 6 - right
		 0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 
		 0.5f,-0.5f, 0.5f, 0.5f, 0.5f,
		 0.5f,-0.5f,-0.5f, 0.75f,0.5f,
		
		// 3, 6, 7 - right
		0.5f, 0.5f, 0.5f, 0.5f, 0.25f, 
		0.5f,-0.5f,-0.5f, 0.75f,0.5f,
		0.5f, 0.5f,-0.5f, 0.75f,0.25f,
		
		// 4, 0, 3 - up
		-0.5f, 0.5f, -0.5f, 0.25f, 0.0f, 
		-0.5f, 0.5f,  0.5f, 0.25f, 0.25f,
		 0.5f, 0.5f,  0.5f,	0.5f,  0.25f,
		
		// 4, 3, 7 - up
		-0.5f, 0.5f, -0.5f, 0.25f, 0.0f,
		 0.5f, 0.5f,  0.5f, 0.5f,  0.25f,
		 0.5f, 0.5f, -0.5f,	0.5f,  0.0f,
		
		// 4, 7, 6 - back
		-0.5f, 0.5f, -0.5f, 0.25f, 1.0f,
		 0.5f, 0.5f, -0.5f, 0.5f,  1.0f,
		 0.5f,-0.5f, -0.5f,	0.5f,  0.75f,
		
		// 4, 6, 5 - back
		-0.5f,  0.5f, -0.5f, 0.25f, 1.0f,
		 0.5f, -0.5f, -0.5f, 0.5f,  0.75f,
		-0.5f, -0.5f, -0.5f, 0.25f,	0.75f,
		
		// 0, 4, 5 - left
		-0.5f, 0.5f,  0.5f, 0.25f, 0.25f, 
		-0.5f, 0.5f, -0.5f, 0.0f,  0.25f,
		-0.5f,-0.5f, -0.5f, 0.0f,  0.5f,
		
		// 0, 5, 1 - left
		-0.5f,  0.5f, 0.5f, 0.25f, 0.25f,
		-0.5f, -0.5f,-0.5f, 0.0f,  0.5f,
		-0.5f, -0.5f, 0.5f, 0.25f, 0.5f,
		
		// 5, 6, 2 - bottom
		-0.5f, -0.5f, -0.5f, 0.25f, 0.75f, 
		 0.5f, -0.5f, -0.5f, 0.5f,  0.75f,
		 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,
		
		// 5, 2, 1 - bottom
		-0.5f, -0.5f, -0.5f, 0.25f, 0.75f,
		 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,
		-0.5f, -0.5f,  0.5f, 0.25f, 0.5f
	};

	public Cube(MySurfaceTexture tex) {
		mTexture = tex.getTex();
		
		ByteBuffer vertexByteBuffer = ByteBuffer
				.allocateDirect(vertexArray.length * 4);
		vertexByteBuffer.order(ByteOrder.nativeOrder());
		mVertexBuffer = vertexByteBuffer.asFloatBuffer();
		mVertexBuffer.put(vertexArray);
		mVertexBuffer.position(0);
		
		int[] bufferObject = new int[1];
		GLES20.glGenBuffers(1, bufferObject, 0);
		mVBO = bufferObject[0];

		GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVBO);
		mVertexBuffer.position(0);
		GLES20.glBufferData(GLES20.GL_ARRAY_BUFFER, vertexArray.length * 4,
				mVertexBuffer, GLES20.GL_STATIC_DRAW);

		int vertexShader = MyGLRenderer.loadShader(GLES20.GL_VERTEX_SHADER,
				vertexShaderCode);
		int fragmentShader = MyGLRenderer.loadShader(GLES20.GL_FRAGMENT_SHADER,
				fragmentShaderCode);

		mProgram = GLES20.glCreateProgram();
		GLES20.glAttachShader(mProgram, vertexShader);
		GLES20.glAttachShader(mProgram, fragmentShader);
		GLES20.glLinkProgram(mProgram);
	}

	public void draw(float[] mvpMatrix) {
		GLES20.glUseProgram(mProgram);
		GLES20.glEnable(GLES20.GL_CULL_FACE);

		mPositionHandle = GLES20.glGetAttribLocation(mProgram, "aPosition");
		mUVHandle = GLES20.glGetAttribLocation(mProgram, "aUV");

		GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVBO);

		GLES20.glEnableVertexAttribArray(mPositionHandle);
		GLES20.glVertexAttribPointer(mPositionHandle, POS_PER_VERTEX,
				GLES20.GL_FLOAT, false, COORDS_PER_VERTEX * 4, 0);

		GLES20.glEnableVertexAttribArray(mUVHandle);
		GLES20.glVertexAttribPointer(mUVHandle, UV_PER_VERTEX, GLES20.GL_FLOAT,
				false, COORDS_PER_VERTEX * 4, POS_PER_VERTEX * 4);

		mTexHandle = GLES20.glGetUniformLocation(mProgram, "sTex");
		mTexture.bind(GLES20.GL_TEXTURE0);
		GLES20.glUniform1i(mTexHandle, 0);

		mMVPMatrixHandle = GLES20.glGetUniformLocation(mProgram, "uMVPMatrix");
		MyGLRenderer.checkGlError("glGetUniformLocation");

		GLES20.glUniformMatrix4fv(mMVPMatrixHandle, 1, false, mvpMatrix, 0);
		MyGLRenderer.checkGlError("glUniformMatrix4fv");

		GLES20.glDrawArrays(GLES20.GL_TRIANGLES, 0, 36);

		GLES20.glDisableVertexAttribArray(mPositionHandle);
		GLES20.glDisableVertexAttribArray(mUVHandle);

		GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, 0);
		
		mTexture.unbind();
	}
}