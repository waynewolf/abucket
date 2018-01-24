#define LOG_TAG "pbuffer_surface"
#include <cutils/memory.h>
#include <utils/Log.h>
#include <android/native_window.h>

#include <binder/IPCThreadState.h>
#include <binder/ProcessState.h>
#include <binder/IServiceManager.h>

#include <gui/Surface.h>
#include <gui/SurfaceComposerClient.h>

#define GL_GLEXT_PROTOTYPES
#include <EGL/egl.h>
#include <GLES3/gl31.h>
#include <GLES2/gl2ext.h>
#include <GLES3/gl3ext.h>

#include <sstream>
#include <fstream>

using namespace android;

const char* eglStatusStr() {
    EGLint error = eglGetError();

    switch (error) {
        case EGL_SUCCESS: return "EGL_SUCCESS";
        case EGL_BAD_DISPLAY: return "EGL_BAD_DISPLAY";
        case EGL_NOT_INITIALIZED: return "EGL_NOT_INITIALIZED";
        case EGL_BAD_ACCESS: return "EGL_BAD_ACCESS";
        case EGL_BAD_ALLOC: return "EGL_BAD_ALLOC";
        case EGL_BAD_ATTRIBUTE: return "EGL_BAD_ATTRIBUTE";
        case EGL_BAD_CONFIG: return "EGL_BAD_CONFIG";
        case EGL_BAD_CONTEXT: return "EGL_BAD_CONTEXT";
        case EGL_BAD_CURRENT_SURFACE: return "EGL_BAD_CURRENT_SURFACE";
        case EGL_BAD_MATCH: return "EGL_BAD_MATCH";
        case EGL_BAD_NATIVE_PIXMAP: return "EGL_BAD_NATIVE_PIXMAP";
        case EGL_BAD_NATIVE_WINDOW: return "EGL_BAD_NATIVE_WINDOW";
        case EGL_BAD_PARAMETER: return "EGL_BAD_PARAMETER";
        case EGL_BAD_SURFACE: return "EGL_BAD_SURFACE";
        default: return "UNKNOWN_EGL_ERROR";
    }
}

static void printGLString(const char *name, GLenum s) {
    const char *v = (const char *) glGetString(s);
    printf("GL %s = %s\n", name, v);
}

static void checkGlError(const char* op) {                                                                                                                             
    for (GLint error = glGetError(); error; error
            = glGetError()) {
        printf("after %s() glError (0x%x)\n", op, error);
    }
}

static const char gVertexShader[] = 
	"#version 310 es\n"
	"in highp vec4 a_position;\n"
	"in highp vec4 a_color;\n"
	"out highp vec4 v_color;\n"
	"void main (void)\n"
	"{\n"
	"	gl_Position = a_position;\n"
	"	v_color = a_color;\n"
	"}\n";

static const char gFragmentShader[] = 
	"#version 310 es\n"
	"layout(location = 0) out highp vec4 fragColor;\n"
	"in highp vec4 v_color;\n"
	"void main (void)\n"
	"{\n"
	"	fragColor = v_color;\n"
	"}\n";

static const char gComputeShader[] =
	"#version 310 es\n"
	"precision highp int;\n"
	"precision highp float;\n"
	"layout(local_size_x = 1, local_size_y = 1) in;\n"
	"layout(std430) buffer;\n"
	"layout(binding = 0) writeonly buffer DataBuffer {\n"
	"	vec4 attribs[];\n"
	"};\n"
	"void main() {\n"
	"	const uint gridSize      = 1000u;\n"
	"	const uint triangleCount = gridSize * gridSize * 2u;\n"
	"	// vertex attribs\n"
	"	const vec4 yellow = vec4(1.0, 1.0, 0.0, 1.0);\n"
	"	const vec4 green = vec4(0.0, 1.0, 0.0, 1.0);\n"
	"	if (gl_GlobalInvocationID.x < gridSize && gl_GlobalInvocationID.y < gridSize && gl_GlobalInvocationID.z == 0u) {\n"
	"	    uint        y           = gl_GlobalInvocationID.x;\n"
	"	    uint        x           = gl_GlobalInvocationID.y;\n"
	"	    float       posX        = (float(x) / float(gridSize)) * 2.0 - 1.0;\n"
	"	    float       posY        = (float(y) / float(gridSize)) * 2.0 - 1.0;\n"
	"	    const float cellSize    = 2.0 / float(gridSize);\n"
	"	    vec4        color       = ((x + y)%2u != 0u) ? (yellow) : (green);\n"
	"	    attribs[((y * gridSize + x) * 6u + 0u) * 2u + 0u] = vec4(posX,            posY,            0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 1u) * 2u + 0u] = vec4(posX + cellSize, posY,            0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 2u) * 2u + 0u] = vec4(posX + cellSize, posY + cellSize, 0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 3u) * 2u + 0u] = vec4(posX,            posY,            0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 4u) * 2u + 0u] = vec4(posX + cellSize, posY + cellSize, 0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 5u) * 2u + 0u] = vec4(posX,            posY + cellSize, 0.0, 1.0);\n"
	"	    attribs[((y * gridSize + x) * 6u + 0u) * 2u + 1u] = color;\n"
	"	    attribs[((y * gridSize + x) * 6u + 1u) * 2u + 1u] = color;\n"
	"	    attribs[((y * gridSize + x) * 6u + 2u) * 2u + 1u] = color;\n"
	"	    attribs[((y * gridSize + x) * 6u + 3u) * 2u + 1u] = color;\n"
	"	    attribs[((y * gridSize + x) * 6u + 4u) * 2u + 1u] = color;\n"
	"	    attribs[((y * gridSize + x) * 6u + 5u) * 2u + 1u] = color;\n"
	"	}\n"
	"}\n";

GLuint loadShader(GLenum shaderType, const char* pSource) {                                                                                                            
    GLuint shader = glCreateShader(shaderType);
    if (shader) {
        glShaderSource(shader, 1, &pSource, NULL);
        glCompileShader(shader);
        GLint compiled = 0;
        glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
        if (!compiled) {
            GLint infoLen = 0;
            glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infoLen);
            if (infoLen) {
                char* buf = (char*) malloc(infoLen);
                if (buf) {
                    glGetShaderInfoLog(shader, infoLen, NULL, buf);
                    printf("Could not compile shader %d:\n%s\n", shaderType, buf);
                    free(buf);
                }
                glDeleteShader(shader);
                shader = 0;
            }
        }
    }
    return shader; 
}

GLuint createComputeProgram(const char *pComputeSource) {
	GLuint computeShader = loadShader(GL_COMPUTE_SHADER, pComputeSource);
    if (!computeShader) {
        return 0;
    }

	GLuint program = glCreateProgram();
	if (program) {
		glAttachShader(program, computeShader);
		glLinkProgram(program);

        GLint linkStatus = GL_FALSE;
        glGetProgramiv(program, GL_LINK_STATUS, &linkStatus);
        if (linkStatus != GL_TRUE) {
            GLint bufLength = 0;
            glGetProgramiv(program, GL_INFO_LOG_LENGTH, &bufLength);
            if (bufLength) {
                char* buf = (char*) malloc(bufLength);
                if (buf) {
                    glGetProgramInfoLog(program, bufLength, NULL, buf);
                    printf("Could not link program:\n%s\n", buf);
                    free(buf);
                }
            }
            glDeleteProgram(program);
			program = 0;
        }
	}

	return program;
}


GLuint createProgram(const char* pVertexSource, const char* pFragmentSource) {
    GLuint vertexShader = loadShader(GL_VERTEX_SHADER, pVertexSource);
    if (!vertexShader) {
        return 0;
    }

    GLuint pixelShader = loadShader(GL_FRAGMENT_SHADER, pFragmentSource);
    if (!pixelShader) {
        return 0;
    }

    GLuint program = glCreateProgram();
    if (program) {
        glAttachShader(program, vertexShader);
        checkGlError("glAttachShader");
        glAttachShader(program, pixelShader);
        checkGlError("glAttachShader");
        glLinkProgram(program);
        GLint linkStatus = GL_FALSE;
        glGetProgramiv(program, GL_LINK_STATUS, &linkStatus);
        if (linkStatus != GL_TRUE) {
            GLint bufLength = 0;
            glGetProgramiv(program, GL_INFO_LOG_LENGTH, &bufLength);
            if (bufLength) {
                char* buf = (char*) malloc(bufLength);
                if (buf) {
                    glGetProgramInfoLog(program, bufLength, NULL, buf);
                    printf("Could not link program:\n%s\n", buf);
                    free(buf);
                }
            }
            glDeleteProgram(program);
            program = 0;
        }
    }
    return program;
}

#define NUM_GRID_X  	1000
#define NUM_GRID_Y		1000
#define NUM_GRIDS		(NUM_GRID_X * NUM_GRID_Y)
#define NUM_TRIANGLES	(NUM_GRIDS * 2)
#define NUM_VERTICES	(NUM_TRIANGLES * 3)

GLuint gProgram;
GLuint gComputeProgram;
GLuint gaPositionHandle;
GLuint gaColorHandle;
GLuint gDataBufferHandle = 0;
GLuint gVBO;
//GLuint gIndirectCommandBO;

static float gIndicesData[] = {
	NUM_TRIANGLES
};

static GLuint gIndirectCommandData[] = {
	NUM_TRIANGLES,  //count
	1,				//primCount
	0,				//first
	0				//baseInstance
};

bool setupGraphics(int w, int h) {
    gProgram = createProgram(gVertexShader, gFragmentShader);
    if (!gProgram) {
        ALOGE("Could not create program.");
        return false;
    }
    gaPositionHandle = glGetAttribLocation(gProgram, "a_position");
    checkGlError("glGetAttribLocation");
    printf("glGetAttribLocation(\"a_position\") = %d\n", gaPositionHandle);

    gaColorHandle = glGetAttribLocation(gProgram, "a_color");
    checkGlError("glGetAttribLocation");
    printf("glGetAttribLocation(\"a_color\") = %d\n", gaColorHandle);

    gComputeProgram = createComputeProgram(gComputeShader);
    if (!gComputeProgram) {
      	printf("Could not create compute program.\n");
        return false;
    }

	glGenBuffers(1, &gVBO);
	checkGlError("gen vbo");
	//glGenBuffers(1, &gIndirectCommandBO);
	//checkGlError("gen indirect command BO");

	glClearColor(0.0, 0.0, 0.0, 1.0);
    glViewport(0, 0, w, h);
    checkGlError("glViewport");
    return true;
}

void checkArrayBuffer(char *buffer) {
	std::ofstream output_bin("/sdcard/array_buffer.bin", std::ios::out);
	output_bin.write((char *)buffer, NUM_VERTICES * 32);

	std::ofstream output("/sdcard/array_buffer.txt", std::ios::out);
	std::ostringstream os;

	for (int v=0; v<NUM_VERTICES; v++) {
		os << v << ": ";
		int vertexOffset = v * 32;
		float pos_x = *(float *)(&buffer[vertexOffset]);
		float pos_y = *(float *)(&buffer[vertexOffset+4]);
		float nouse = *(float *)(&buffer[vertexOffset+8]);
		      nouse = *(float *)(&buffer[vertexOffset+12]);
		float colorR = *(float *)(&buffer[vertexOffset+16]);
		float colorG = *(float *)(&buffer[vertexOffset+20]);
		float colorB = *(float *)(&buffer[vertexOffset+24]);
		float colorA = *(float *)(&buffer[vertexOffset+28]);

		os << pos_x << " " << pos_y << " "
			<< colorR << " " << colorG << " " << colorB << "\n";
	}

	output << os.str();
}

int main() {
    sp<ProcessState> proc(ProcessState::self());
    ProcessState::self()->startThreadPool();

    EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (display == EGL_DEFAULT_DISPLAY) {
        printf("Unable to connect window system: %s\n", eglStatusStr());
        return false;
    }
    EGLint majorVersion, minorVersion;
    if (!eglInitialize(display, &majorVersion, &minorVersion)) {
        printf("Unable to initialize egl: %s\n", eglStatusStr());
        return false;
    }

    const EGLint configAttribs[] = {
        EGL_SURFACE_TYPE,       EGL_PBUFFER_BIT,
        EGL_BLUE_SIZE,          8,
        EGL_GREEN_SIZE,         8,
        EGL_RED_SIZE,           8,
        EGL_ALPHA_SIZE,         8,
        EGL_DEPTH_SIZE,         0,
        EGL_STENCIL_SIZE,       0,
        EGL_SAMPLE_BUFFERS,     0,
        EGL_RENDERABLE_TYPE,    EGL_OPENGL_ES2_BIT,
        EGL_NONE
    };

    EGLConfig config;
    EGLint numConfigs;
    if (eglChooseConfig(display, configAttribs, &config, 1, &numConfigs) != EGL_TRUE) {
        printf("Unable to choose egl config: %s\n", eglStatusStr());
        return false;
    }
    printf("eglChooseConfig return %d configs\n", numConfigs);
    
    if (numConfigs < 1) {
    	return false;
    }

    EGLint format, redSize, greenSize, blueSize, alphaSize;
    eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &format);
    eglGetConfigAttrib(display, config, EGL_RED_SIZE, &redSize);
    eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &greenSize);
    eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &blueSize);
    eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &alphaSize);
    printf("eglGetConfigAttrib format: %d, r: %d, g: %d, b: %d, a: %d\n",
    	format, redSize, greenSize, blueSize, alphaSize);

    const EGLint surfaceAttr[] = {
        EGL_WIDTH, 480,
        EGL_HEIGHT, 782,
        EGL_NONE    
    };

    EGLSurface surface = eglCreatePbufferSurface(display, config, surfaceAttr);
    if (surface == EGL_NO_SURFACE) {
        printf("Unable to create surface: %s\n", eglStatusStr());
        return false;
    }
    EGLint contextAttrs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 3,
        EGL_NONE
    };

    EGLContext context = eglCreateContext(display, config, NULL, contextAttrs);
    if (context == EGL_NO_CONTEXT) {
        printf("Unable to create context: %s\n", eglStatusStr());
        return false;
    }
    if (eglMakeCurrent(display, surface, surface, context) == EGL_FALSE) {
        printf("Unable to eglMakeCurrent\n");
        return false;
    }


    EGLint w, h;
    eglQuerySurface(display, surface, EGL_WIDTH, &w);
    eglQuerySurface(display, surface, EGL_HEIGHT, &h);

    printf("eglQuerySurface get width: %d, height: %d\n", w, h);

    setupGraphics(w, h);

	//glBindBuffer(GL_DRAW_INDIRECT_BUFFER, gIndirectCommandBO);
	//checkGlError("bind draw indirect buffer");
	//glBufferData(GL_DRAW_INDIRECT_BUFFER, sizeof(gIndirectCommandData), gIndirectCommandData, GL_STATIC_DRAW);
	//checkGlError("buffer data to indirect buffer");

	glUseProgram(gComputeProgram);
	checkGlError("bind compute program");
	// Bind the VBO to the SSBO, that is filled in the compute shader.
	// gDataBufferHandle is equal to 0. This is the same as the compute shader binding.
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, gDataBufferHandle, gVBO);
	checkGlError("bind vbo to ssbo");
	// 1000x1000 grid, 2000000 triangles, 6000000 vertices, 32 bytes for each vertex
	glBufferData(GL_SHADER_STORAGE_BUFFER, NUM_VERTICES * 32, NULL, GL_DYNAMIC_DRAW);
	checkGlError("set vbo size");
	// 1000X1000 grid, follow cts value
	glDispatchCompute(NUM_GRID_X, NUM_GRID_X, 1);
	checkGlError("compute shader dispatch");
	// Unbind the SSBO buffer.
	// gDataBufferHandle is equal to 0. This is the same as the compute shader binding.
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, gDataBufferHandle, 0);
	glMemoryBarrier(GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT);
	checkGlError("memory barrier");

//    while (true) {
		glClear(GL_COLOR_BUFFER_BIT);

	    glUseProgram(gProgram);
	    checkGlError("glUseProgram");

		glBindBuffer(GL_ARRAY_BUFFER, gVBO);
		checkGlError("bind vbo");

	    glVertexAttribPointer(gaPositionHandle, 4, GL_FLOAT, GL_FALSE, 32, (const void *)0);
	    glEnableVertexAttribArray(gaPositionHandle);
	    glVertexAttribPointer(gaColorHandle, 4, GL_FLOAT, GL_FALSE, 32, (const void *)16);
	    glEnableVertexAttribArray(gaColorHandle);

		//glBindBuffer(GL_DRAW_INDIRECT_BUFFER, gIndirectCommandBO);
		//checkGlError("bind indirect buffer bo");
	    //glDrawArraysIndirect(GL_TRIANGLES, 0);
		glDrawArrays(GL_TRIANGLES, 0, NUM_VERTICES);
		checkGlError("glDrawArrays");

	    eglSwapBuffers(display, surface);
//    }

    char *pixels = (char *)malloc(w * h * 4);
    glReadPixels(0, 0, w, h, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
    
    std::ofstream output_pixel("/sdcard/pbuffer_pixels.bin", std::ios::out);
    output_pixel.write(pixels, w * h * 4);
    output_pixel.flush();

    free(pixels);
    printf("ready\n");

    IPCThreadState::self()->joinThreadPool();
    
    return 0;
}

