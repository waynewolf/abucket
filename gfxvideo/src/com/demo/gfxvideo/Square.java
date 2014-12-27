package com.demo.gfxvideo;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;
import java.nio.ShortBuffer;

import android.opengl.GLES20;

public class Square {
	public static final int FACE_FRONT = 0;
	public static final int FACE_LEFT  = 1;
	public static final int FACE_RIGHT = 2;
	public static final int FACE_UP    = 3;
	public static final int FACE_DOWN  = 4;
	public static final int FACE_BACK  = 5;
	
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

    private final FloatBuffer vertexBuffer;
    private final ShortBuffer drawListBuffer;
    private float[] mVertexArray;
    private int mVBO;
    private int mIBO;
    private final int mProgram;
    private int mPositionHandle;
    private int mUVHandle;
    private int mTexHandle;
    private int mMVPMatrixHandle;
    private Texture mTexture;
	private static final int POS_PER_VERTEX = 3;
	private static final int UV_PER_VERTEX = 2;
	private static final int COORDS_PER_VERTEX = POS_PER_VERTEX + UV_PER_VERTEX;

    private final short drawOrder[] = { 0, 1, 2, 0, 2, 3 }; // order to draw vertices

    private final int vertexStride = COORDS_PER_VERTEX * 4; // 4 bytes per vertex

    public Square(MySurfaceTexture surfaceTexture, int flag) {
    	mTexture = surfaceTexture.getTex();
    	
    	switch(flag) {
    	case FACE_FRONT:
    		mVertexArray = new float[] {
				-0.5f,  0.5f,  0.5f, 0.0f, 0.0f, // 0 - front
				-0.5f, -0.5f,  0.5f, 0.0f, 1.0f, // 1 - front
				 0.5f, -0.5f,  0.5f, 1.0f, 1.0f, // 2 - front
				 0.5f,  0.5f,  0.5f, 1.0f, 0.0f  // 3 - front
    		};
    		break;
    	case FACE_BACK:
    		mVertexArray = new float[] {
				-0.5f, -0.5f, -0.5f, 0.0f, 0.0f, // 5 - back
				-0.5f,  0.5f, -0.5f, 0.0f, 1.0f, // 4 - back
				 0.5f,  0.5f, -0.5f, 1.0f, 1.0f, // 7 - back
				 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, // 6 - back	
    		};
    		break;
    	case FACE_UP:
    		mVertexArray = new float[] {
				-0.5f,  0.5f, -0.5f, 0.0f, 0.0f, // 4 - up
				-0.5f,  0.5f,  0.5f, 0.0f, 1.0f, // 0 - up
				 0.5f,  0.5f,  0.5f, 1.0f, 1.0f, // 3 - up
				 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, // 7 - up
    		};
    		break;
    	case FACE_DOWN:
    		mVertexArray = new float[] {
				-0.5f, -0.5f,  0.5f, 0.0f, 0.0f, // 1 - bottom
				-0.5f, -0.5f, -0.5f, 0.0f, 1.0f, // 5 - bottom
				 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, // 6 - bottom
				 0.5f, -0.5f,  0.5f, 1.0f, 0.0f, // 2 - bottom
    		};
    		break;
    	case FACE_LEFT:
    		mVertexArray = new float[] {
				-0.5f,  0.5f, -0.5f, 0.0f, 0.0f, // 4 - left  
				-0.5f, -0.5f, -0.5f, 0.0f, 1.0f, // 5 - left 
				-0.5f, -0.5f,  0.5f, 1.0f, 1.0f, // 1 - left
				-0.5f,  0.5f,  0.5f, 1.0f, 0.0f, // 0 - left
    		};
    		break;
    	case FACE_RIGHT:
    		mVertexArray = new float[] {
				 0.5f,  0.5f,  0.5f, 0.0f, 0.0f, // 3 - right
				 0.5f, -0.5f,  0.5f, 0.0f, 1.0f, // 2 - right
				 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, // 6 - right
				 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, // 7 - right
    		};
    		break;
    	}
    	
        ByteBuffer bb = ByteBuffer.allocateDirect(mVertexArray.length * 4);
        bb.order(ByteOrder.nativeOrder());
        vertexBuffer = bb.asFloatBuffer();
        vertexBuffer.put(mVertexArray);
        vertexBuffer.position(0);

        ByteBuffer dlb = ByteBuffer.allocateDirect(drawOrder.length * 2);
        dlb.order(ByteOrder.nativeOrder());
        drawListBuffer = dlb.asShortBuffer();
        drawListBuffer.put(drawOrder);
        drawListBuffer.position(0);

		int[] bufferObject = new int[2];
		GLES20.glGenBuffers(2, bufferObject, 0);
		mVBO = bufferObject[0];
		mIBO = bufferObject[1];
		GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVBO);
		vertexBuffer.position(0);
		GLES20.glBufferData(GLES20.GL_ARRAY_BUFFER, mVertexArray.length * 4,
				vertexBuffer, GLES20.GL_STATIC_DRAW);
		
		GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, mIBO);
		GLES20.glBufferData(GLES20.GL_ELEMENT_ARRAY_BUFFER, drawOrder.length * 2,
				drawListBuffer, GLES20.GL_STATIC_DRAW);
		
        int vertexShader = MyGLRenderer.loadShader(
                GLES20.GL_VERTEX_SHADER,
                vertexShaderCode);
        int fragmentShader = MyGLRenderer.loadShader(
                GLES20.GL_FRAGMENT_SHADER,
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
        
        GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, mIBO);
        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVBO);
        GLES20.glEnableVertexAttribArray(mPositionHandle);
        GLES20.glVertexAttribPointer(
                mPositionHandle, POS_PER_VERTEX,
                GLES20.GL_FLOAT, false,
                vertexStride, 0);
        
		GLES20.glEnableVertexAttribArray(mUVHandle);
        vertexBuffer.position(POS_PER_VERTEX);
		GLES20.glVertexAttribPointer(mUVHandle, UV_PER_VERTEX, GLES20.GL_FLOAT,
				false, vertexStride, 12);
		
		mTexHandle = GLES20.glGetUniformLocation(mProgram, "sTex");
		mTexture.bind(GLES20.GL_TEXTURE0);
		GLES20.glUniform1i(mTexHandle, 0);

        mMVPMatrixHandle = GLES20.glGetUniformLocation(mProgram, "uMVPMatrix");
        MyGLRenderer.checkGlError("glGetUniformLocation");

        GLES20.glUniformMatrix4fv(mMVPMatrixHandle, 1, false, mvpMatrix, 0);
        MyGLRenderer.checkGlError("glUniformMatrix4fv");

        GLES20.glDrawElements(
                GLES20.GL_TRIANGLES, drawOrder.length,
                GLES20.GL_UNSIGNED_SHORT, 0);

        GLES20.glDisableVertexAttribArray(mPositionHandle);
		GLES20.glDisableVertexAttribArray(mUVHandle);
		
        GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, 0);
        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, 0);
		mTexture.unbind();
    }

}