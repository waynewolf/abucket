#include <cutils/memory.h>
#include <utils/Log.h>
#include <android/native_window.h>

#include <binder/IPCThreadState.h>
#include <binder/ProcessState.h>
#include <binder/IServiceManager.h>

#include <gui/Surface.h>
#include <gui/SurfaceComposerClient.h>

using namespace android;

#define R10G10B10A2_COLOR_CHANNEL_MAX ((1 << 10) - 1)
#define R10G10B10A2_ALPHA_CHANNEL_MAX ((1 << 2)  - 1)

#if 0
    /* word order */
    #define R10G10B10A2_RED(V)      (V << 22)
    #define R10G10B10A2_GREEN(V)    (V << 12)
    #define R10G10B10A2_BLUE(V)     (V << 2)
    #define R10G10B10A2_ALPHA(V)    (V)
#else
    /* byte order */
    #define R10G10B10A2_RED(V)      (((V) & 0x3FF) << 0)
    #define R10G10B10A2_GREEN(V)    (((V) & 0x3FF) << 10)
    #define R10G10B10A2_BLUE(V)     (((V) & 0x3FF) << 20)
    #define R10G10B10A2_ALPHA(V)    (((V) & 0x3 ) << 30)
#endif

#define PRE_MULTIPLIED_ALPHA 1

static void drawR10G10B10A2VerticalBar(void *data, uint32_t width, uint32_t height, uint32_t bpp, uint32_t stride, uint8_t alpha) {
    alpha &= 0x3;

    uint16_t color = R10G10B10A2_COLOR_CHANNEL_MAX;
    #if PRE_MULTIPLIED_ALPHA == 1
        color = (uint16_t)(color / 4.0f * alpha);
    #endif

    for (uint32_t y = 0; y < height; y++) {
        for (uint32_t x = 0; x < width; x++) {
            uint32_t* pixel = (uint32_t *)((uint8_t *)data + (bpp * ( y * stride + x)));

            *pixel |= R10G10B10A2_ALPHA(alpha);

            if ( x < width / 4)
                *pixel = R10G10B10A2_RED(color);
            else if ( x < width / 2)
                *pixel = R10G10B10A2_GREEN(color);
            else if ( x < width / 4 * 3 )
                *pixel = R10G10B10A2_BLUE(color);
            else
                *pixel = R10G10B10A2_RED(color)
                       | R10G10B10A2_GREEN(color)
                       | R10G10B10A2_BLUE(color);
       }
    }
}

static void drawRGBAVerticalBar(void *data, uint32_t width, uint32_t height, uint32_t bpp, uint32_t stride, uint8_t alpha) {
    alpha &= 0x3;

    // map 2 bit alpha to 8 bit alpha, 0 -> 0, 1 -> 84, 2 -> 169, 3 -> 255
    uint8_t adjusted_alpha = 0;
    if (alpha > 0)
        adjusted_alpha = (alpha * 256 / 3) - 1;
    
    uint8_t color = 255;
    #if PRE_MULTIPLIED_ALPHA == 1
        color = (uint8_t)(color / 255.0f * adjusted_alpha);
    #endif

    for (uint32_t y = 0; y < height; y++) {
        for (uint32_t x = 0; x < width; x++) {
            uint32_t* pixel = (uint32_t *)((uint8_t *)data + (bpp * ( y * stride + x)));

            *pixel |= adjusted_alpha << 24;

            // Red
            if ( x < width / 4)
                *pixel = color;
            // Green
            else if ( x < width / 2)
                *pixel = color << 8;
            // Blue
            else if ( x < width / 4 * 3 )
                *pixel = color << 16;
            // White
            else
                *pixel = color | color << 8 | color << 16;
       }
    }
}

int main() {
    sp<ProcessState> proc(ProcessState::self());
    ProcessState::self()->startThreadPool();

    sp<SurfaceComposerClient> client = new SurfaceComposerClient();
    sp<SurfaceControl> surfaceControl[8];
    sp<Surface> surface[8];

    /***********************************************************
     * draw 4 RGBA8888 surfaces 
     **********************************************************/
    for (int i=0; i<4; i++) {
        surfaceControl[i] = client->createSurface(String8("format_test_rgba8888"), 320, 240, HAL_PIXEL_FORMAT_RGBA_8888, 0);
        if (surfaceControl[i] == NULL) {
            ALOGE("Failed to get SurfaceControl");
            return -1;
        }
        ALOGI("Surface created");
        surface[i] = surfaceControl[i]->getSurface();
        if (surface[i] == NULL) {
            ALOGE("Failed to get Surface");
            return -1;
        }
        ALOGI("Surface got");

        SurfaceComposerClient::openGlobalTransaction();
        surfaceControl[i]->setLayer(0xFFFFFF);
        SurfaceComposerClient::closeGlobalTransaction();
     }

    for (uint32_t i = 0; i < 4; i++) {
        ANativeWindow_Buffer outBuffer;
        surface[i]->lock(&outBuffer, NULL);
        uint32_t width = outBuffer.width;
        uint32_t height = outBuffer.height;
        uint32_t stride = outBuffer.stride;
        uint32_t format = outBuffer.format;
        uint32_t bpp = bytesPerPixel(outBuffer.format);
        uint32_t alpha = (uint32_t)i;
        ALOGI("f: %d, w: %d, h: %d, bpp: %d, stride: %d, alpha: %d", format, width, height, bpp, stride, alpha);
        drawRGBAVerticalBar(outBuffer.bits, width, height, bpp, stride, alpha);
        surface[i]->unlockAndPost();

        SurfaceComposerClient::openGlobalTransaction();
        surfaceControl[i]->setPosition(i * 350, 0);
        surfaceControl[i]->show();
        SurfaceComposerClient::closeGlobalTransaction();
    }

    /***********************************************************
     * draw 4 R10G10B10A2 surfaces 
     **********************************************************/
    for (int i=4; i<8; i++) {
        surfaceControl[i] = client->createSurface(String8("format_test_rgb10a2"), 320, 240, HAL_PIXEL_FORMAT_R10G10B10A2, 0);
        if (surfaceControl[i] == NULL) {
            ALOGE("Failed to get SurfaceControl");
            return -1;
        }
        ALOGI("R10G10B10A2 Surface created");
        
        surface[i] = surfaceControl[i]->getSurface();
        if (surface[i] == NULL) {
            ALOGE("Failed to get Surface");
            return -1;
        }
        ALOGI("R10G10B10A2 Surface got");

        SurfaceComposerClient::openGlobalTransaction();
        surfaceControl[i]->setLayer(0xFFFFFE);
        SurfaceComposerClient::closeGlobalTransaction();
    }

    for (uint32_t i = 0; i < 4; i++) {
        ANativeWindow_Buffer outBuffer;
        surface[i+4]->lock(&outBuffer, NULL);
        uint32_t width = outBuffer.width;
        uint32_t height = outBuffer.height;
        uint32_t stride = outBuffer.stride;
        uint32_t format = outBuffer.format;
        uint32_t bpp = bytesPerPixel(outBuffer.format);
        uint32_t alpha = (uint32_t)i;
        ALOGI("f: %d, w: %d, h: %d, bpp: %d, stride: %d, alpha: %d", format, width, height, bpp, stride, alpha);
        drawR10G10B10A2VerticalBar(outBuffer.bits, width, height, bpp, stride, alpha);
        surface[i+4]->unlockAndPost();

        SurfaceComposerClient::openGlobalTransaction();
        surfaceControl[i+4]->setPosition(i * 350, 500);
        surfaceControl[i+4]->show();
        SurfaceComposerClient::closeGlobalTransaction();
    }

    IPCThreadState::self()->joinThreadPool();
    
    return 0;
}

