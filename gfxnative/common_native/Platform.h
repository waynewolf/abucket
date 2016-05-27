/*
 * This confidential and proprietary software may be used only as
 * authorised by a licensing agreement from ARM Limited
 * (C) COPYRIGHT 2012 ARM Limited
 * ALL RIGHTS RESERVED
 * The entire notice above must be reproduced on all authorised
 * copies and copies may only be made to the extent permitted
 * by a licensing agreement from ARM Limited.
 */

#ifndef PLATFORM_H
#define PLATFORM_H

#if !defined(__android__)

#include "EGLRuntime.h"
#include "VectorTypes.h"

#include <cstdio>

namespace MaliSDK
{
    /**
     * \brief Abstract class to hide the complexity of multiple build targets.
     */
    class Platform
    {
    public:
        /**
         * \brief An enum to define the status of a window.
         */
        enum WindowStatus {/** Window has nothing to report */
                           WINDOW_IDLE, 
                           /** The window has been closed by the user */
                           WINDOW_EXIT, 
                           /** The user has clicked on the window */
                           WINDOW_CLICK};
        Vec2 mouseClick;
        /*
         * The following variables are platform specific handles/pointers to 
         * displays/devices/windows. Used to create and manage the window 
         * to which OpenGL ES 2.0 graphics are rendered.
         */
    #if defined(_WIN32)
        HWND window;
        HDC deviceContext;
    #elif defined(__arm__) && defined(__linux__)
        fbdev_window *window;
    #elif defined(__linux__)
        Window window;
        Display* display;
    #endif    
        /**
         * \brief Create a native window on the target device.
         * \param[in] width The required width of the window.
         * \param[in] height The required height of the window.
         */
        virtual void createWindow(int width, int height) = 0;
        
        /**
         * \brief Check status of the window.
         * \return The status of the window.
         */
        virtual WindowStatus checkWindow(void) = 0;
        
        /**
         * \brief Close and clean-up the native window.
         */
        virtual void destroyWindow(void) = 0;
        
        /**
         * \brief Print a log message to the terminal.
         * \param[in] format The format the log message should take. Equivilent to printf.
         * \param[in] ... Variable length input to specify variables to print. They will be formatted as specified in format.
         */
        static void log(const char* format, ...);
        
        /**
         * Get the instance of Platform specific to the target.
         * \return An instance of a subclass of Platform which will work on the target platform.
         */
        static Platform* getInstance(void);
    };
}
#if defined(_WIN32)
#include "WindowsPlatform.h"

#elif defined(__arm__) && defined(__linux__)
#include "LinuxOnARMPlatform.h"

#elif defined(__linux__)
#include "DesktopLinuxPlatform.h"

#endif

#define GL_CHECK(x) \
    x; \
    { \
        GLenum glError = glGetError(); \
        if(glError != GL_NO_ERROR) { \
            LOGD("glGetError() = %i (0x%.8x) at %s:%i\n", glError, glError, __FILE__, __LINE__); \
            exit(1); \
        } \
    }
    
#define LOGI Platform::log 
#define LOGE fprintf (stderr, "Error: "); Platform::log
#ifdef DEBUG
#define LOGD fprintf (stderr, "Debug: "); Platform::log
#else
#define LOGD
#endif

#else
#include "AndroidPlatform.h"
#endif /* !defined(__android__) */
#endif /* PLATFORM_H */