/*
 * ported from NDK gles3jni sample
 */

#include <android/native_window.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <Platform.h> // from common_native lib, for LOG facility
#include <vector>
#include "utils.h"
#include <Timer.h>
#include <math.h>
#include "NativeWinApp.h"

using namespace MaliSDK;
using namespace android;
using namespace std;

#define STR(s) #s
#define STRV(s) STR(s)

#define POS_ATTRIB 0
#define COLOR_ATTRIB 1
#define SCALEROT_ATTRIB 2
#define OFFSET_ATTRIB 3

#define MAX_INSTANCES_PER_SIDE 16
#define MAX_INSTANCES   (MAX_INSTANCES_PER_SIDE * MAX_INSTANCES_PER_SIDE)
#define TWO_PI          (2.0 * M_PI)
#define MAX_ROT_SPEED   (0.3 * TWO_PI)


static const char VERTEX_SHADER[] =
    "#version 300 es\n"
    "layout(location = " STRV(POS_ATTRIB) ") in vec2 pos;\n"
    "layout(location=" STRV(COLOR_ATTRIB) ") in vec4 color;\n"
    "layout(location=" STRV(SCALEROT_ATTRIB) ") in vec4 scaleRot;\n"
    "layout(location=" STRV(OFFSET_ATTRIB) ") in vec2 offset;\n"
    "out vec4 vColor;\n"
    "void main() {\n"
    "    mat2 sr = mat2(scaleRot.xy, scaleRot.zw);\n"
    "    gl_Position = vec4(sr*pos + offset, 0.0, 1.0);\n"
    "    vColor = color;\n"
    "}\n";

static const char FRAGMENT_SHADER[] =
    "#version 300 es\n"
    "precision mediump float;\n"
    "in vec4 vColor;\n"
    "out vec4 outColor;\n"
    "void main() {\n"
    "    outColor = vColor;\n"
    "}\n";

struct Vertex {
    GLfloat pos[2];
    GLubyte rgba[4];                                                         
};

const Vertex QUAD[4] = {                                                                                                                                               
    // Square with diagonal < 2 so that it fits in a [-1 .. 1]^2 square
    // regardless of rotation.
    {{-0.7f, -0.7f}, {0x00, 0xFF, 0x00}},
    {{ 0.7f, -0.7f}, {0x00, 0x00, 0xFF}},
    {{-0.7f,  0.7f}, {0xFF, 0x00, 0x00}},
    {{ 0.7f,  0.7f}, {0xFF, 0xFF, 0xFF}},
};

class App : public NativeWinApp
{
public:
    enum {VB_INSTANCE, VB_SCALEROT, VB_OFFSET, VB_COUNT};

    App() :
        mNumInstances(0),                                                          
        mLastFrameNs(0),
        mProgram(0),
        mVBState(0)
    {
        memset(mScale, 0, sizeof(mScale));
        memset(mAngularVelocity, 0, sizeof(mAngularVelocity));
        memset(mAngles, 0, sizeof(mAngles));

        for (int i = 0; i < VB_COUNT; i++)
            mVB[i] = 0;
    }

    ~App() {
        glDeleteVertexArrays(1, &mVBState);
        glDeleteBuffers(VB_COUNT, mVB);
        glDeleteProgram(mProgram);
    }

    virtual bool postInitializeEGL(EGLDisplay display, EGLSurface surface)
    {
        eglSetDamageRegionKHR =
            (PFNEGLSETDAMAGEREGIONKHRPROC)eglGetProcAddress("eglSetDamageRegionKHR");
        if (!eglSetDamageRegionKHR) {
            LOGE("No eglSetDamageRegionKHR symbol found\n");
            return false;
        }
        _fpsTimer.reset();
        return true;
    }

    virtual bool setupGraphics()
    {
        mProgram = GLUtil::createProgram(VERTEX_SHADER, FRAGMENT_SHADER);
        if (!mProgram)
            return false;

        glGenBuffers(VB_COUNT, mVB);
        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_INSTANCE]);
        glBufferData(GL_ARRAY_BUFFER, sizeof(QUAD), &QUAD[0], GL_STATIC_DRAW);
        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_SCALEROT]);
        glBufferData(GL_ARRAY_BUFFER, MAX_INSTANCES * 4*sizeof(float), NULL, GL_DYNAMIC_DRAW);
        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_OFFSET]);
        glBufferData(GL_ARRAY_BUFFER, MAX_INSTANCES * 2*sizeof(float), NULL, GL_STATIC_DRAW);

        glGenVertexArrays(1, &mVBState);
        glBindVertexArray(mVBState);

        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_INSTANCE]);
        glVertexAttribPointer(POS_ATTRIB, 4, GL_FLOAT, GL_FALSE, sizeof(Vertex), (const GLvoid*)offsetof(Vertex, pos));
        glVertexAttribPointer(COLOR_ATTRIB, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(Vertex), (const GLvoid*)offsetof(Vertex, rgba));
        glEnableVertexAttribArray(POS_ATTRIB);
        glEnableVertexAttribArray(COLOR_ATTRIB);

        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_SCALEROT]);
        glVertexAttribPointer(SCALEROT_ATTRIB, 4, GL_FLOAT, GL_FALSE, 4*sizeof(float), 0);
        glEnableVertexAttribArray(SCALEROT_ATTRIB);
        glVertexAttribDivisor(SCALEROT_ATTRIB, 1);

        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_OFFSET]);
        glVertexAttribPointer(OFFSET_ATTRIB, 2, GL_FLOAT, GL_FALSE, 2*sizeof(float), 0);
        glEnableVertexAttribArray(OFFSET_ATTRIB);
        glVertexAttribDivisor(OFFSET_ATTRIB, 1);

        float* offsets = mapOffsetBuf();
        calcSceneParams(_winWidth, _winHeight, offsets);
        unmapOffsetBuf();

        for (unsigned int i = 0; i < mNumInstances; i++) {
            mAngles[i] = drand48() * TWO_PI;
            mAngularVelocity[i] = MAX_ROT_SPEED * (2.0*drand48() - 1.0);
        }

        mLastFrameNs = 0;

        return true;
    }

    void calcSceneParams(unsigned int w, unsigned int h, float* offsets)
    {
        // number of cells along the larger screen dimension
        const float NCELLS_MAJOR = MAX_INSTANCES_PER_SIDE;
        // cell size in scene space
        const float CELL_SIZE = 2.0f / NCELLS_MAJOR;

        // Calculations are done in "landscape", i.e. assuming dim[0] >= dim[1].
        // Only at the end are values put in the opposite order if h > w.
        const float dim[2] = {fmaxf(w,h), fminf(w,h)};
        const float aspect[2] = {dim[0] / dim[1], dim[1] / dim[0]};
        const float scene2clip[2] = {1.0f, aspect[0]};
        const int ncells[2] = {
                NCELLS_MAJOR,
                (int)floorf(NCELLS_MAJOR * aspect[1])
        };

        float centers[2][MAX_INSTANCES_PER_SIDE];
        for (int d = 0; d < 2; d++) {
            float offset = -ncells[d] / NCELLS_MAJOR; // -1.0 for d=0
            for (int i = 0; i < ncells[d]; i++) {
                centers[d][i] = scene2clip[d] * (CELL_SIZE*(i + 0.5f) + offset);
            }
        }

        int major = w >= h ? 0 : 1;
        int minor = w >= h ? 1 : 0;
        // outer product of centers[0] and centers[1]
        for (int i = 0; i < ncells[0]; i++) {
            for (int j = 0; j < ncells[1]; j++) {
                int idx = i*ncells[1] + j;
                offsets[2*idx + major] = centers[0][i];
                offsets[2*idx + minor] = centers[1][j];
            }
        }

        mNumInstances = ncells[0] * ncells[1];
        mScale[major] = 0.5f * CELL_SIZE * scene2clip[0];
        mScale[minor] = 0.5f * CELL_SIZE * scene2clip[1];
    }

    virtual void startFrame()
    {
        EGLint age = -1;
        EGLDisplay display = getEGLDisplay();
        EGLSurface surface = getEGLSurface();
        if (!eglQuerySurface(getEGLDisplay(), getEGLSurface(), EGL_BUFFER_AGE_KHR, &age)) {
            LOGE("query buffer age of surface failed\n");
            return;
        }

        EGLint rects[4]; // x, y, width, height in lower left corner of the surface
        rects[0] = 0;
        rects[1] = 0;
        rects[2] = 1920;
        rects[3] = 1080;
        if (!eglSetDamageRegionKHR(display, surface, rects, 1)) {
            LOGE("Error call eglSetDamageRegionKHR, %s\n", GLUtil::eglStatusStr());
            return;
        }
    }

    virtual void renderFrame()
    {
        timespec now;
        clock_gettime(CLOCK_MONOTONIC, &now);
        uint64_t nowNs = now.tv_sec*1000000000ull + now.tv_nsec;

        if (mLastFrameNs > 0) {
            float dt = float(nowNs - mLastFrameNs) * 0.000000001f;

            for (unsigned int i = 0; i < mNumInstances; i++) {
                mAngles[i] += mAngularVelocity[i] * dt;
                if (mAngles[i] >= TWO_PI) {
                    mAngles[i] -= TWO_PI;
                } else if (mAngles[i] <= -TWO_PI) {
                    mAngles[i] += TWO_PI;
                }
            }

            float* transforms = mapTransformBuf();
            for (unsigned int i = 0; i < mNumInstances; i++) {
                float s = sinf(mAngles[i]);
                float c = cosf(mAngles[i]);
                transforms[4*i + 0] =  c * mScale[0];
                transforms[4*i + 1] =  s * mScale[1];
                transforms[4*i + 2] = -s * mScale[0];
                transforms[4*i + 3] =  c * mScale[1];
            }
            unmapTransformBuf();
        }

        mLastFrameNs = nowNs;

        glClearColor(0.2f, 0.2f, 0.3f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        draw(mNumInstances);
        LOGI("fps: %f\n", _fpsTimer.getFPS());
    }

    float* mapOffsetBuf() {
        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_OFFSET]);
        return (float*)glMapBufferRange(GL_ARRAY_BUFFER,
                0, MAX_INSTANCES * 2*sizeof(float),
                GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
    }

    void unmapOffsetBuf() {
        glUnmapBuffer(GL_ARRAY_BUFFER);
    }

    float* mapTransformBuf() {
        glBindBuffer(GL_ARRAY_BUFFER, mVB[VB_SCALEROT]);
        return (float*)glMapBufferRange(GL_ARRAY_BUFFER,
                0, MAX_INSTANCES * 4*sizeof(float),
                GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
    }

    void unmapTransformBuf() {
        glUnmapBuffer(GL_ARRAY_BUFFER);
    }

    void draw(unsigned int numInstances) {
        glUseProgram(mProgram);
        glBindVertexArray(mVBState);
        glDrawArraysInstanced(GL_TRIANGLE_STRIP, 0, 4, numInstances);
    }

protected:
    unsigned int mNumInstances;
    float mScale[2];
    float mAngularVelocity[MAX_INSTANCES];
    uint64_t mLastFrameNs;
    float mAngles[MAX_INSTANCES];
    GLuint mProgram;
    GLuint mVB[VB_COUNT];
    GLuint mVBState;
    Timer _fpsTimer;
    PFNEGLSETDAMAGEREGIONKHRPROC eglSetDamageRegionKHR;
};

int main() 
{
    App *app = new App();

    if (!app->initialize(0, 0, 1920, 1080)) {
        LOGE("App failed to initialize\n");
        return -1;
    }
    //app->setRenderMode(synchronized_to_vsync)
    while(true) {
        app->run();
    }
    app->release();
    
    return 0;
}

