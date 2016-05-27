/*
 * This confidential and proprietary software may be used only as
 * authorised by a licensing agreement from ARM Limited
 * (C) COPYRIGHT 2012 ARM Limited
 * ALL RIGHTS RESERVED
 * The entire notice above must be reproduced on all authorised
 * copies and copies may only be made to the extent permitted
 * by a licensing agreement from ARM Limited.
 */

#ifndef ANDROIDPLATFORM_H
#define ANDROIDPLATFORM_H

#include <stdio.h>
#include <jni.h>
#include <android/log.h>

#define  LOG_TAG    __FILE__

#define  LOGI(format, args...) { fprintf(stderr, format, ##args); __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, format, ##args); }
#define  LOGE(format, args...) { fprintf(stderr, format, ##args); __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, format, ##args); }
#define  LOGD(format, args...) { fprintf(stderr, format, ##args); __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, format, ##args); }

#define GL_CHECK(x) \
        x; \
        AndroidPlatform::checkGlesError(#x);

namespace MaliSDK
{
    /**
     * \brief Functions specific to the Android Platform
     */
    class AndroidPlatform
    {
    public:
        /**
         * \brief Extract an asset file from the APK.
         *
         * If the file specified by filename is not avaliable in destinationDirectory, this method will attempt to extract it from the APK into destinationDirectory.
         * Typically used in OpenGL ES applications to extract shader files and textures.
         * \param[in] JNIEnvironment  A pointer to the JNI environment which allows interfacing with the Java Virtual Machine (JVM).
         *                            Allows extensive interaction with the JVM including accessing Java classes, fields and methods.
         *                            This pointer is provided as part of a JNI call from Java to C++.
         * \param[in] destinationDirectory The destination directory where the file should be placed.
         * \param[in] filename Name of the file to extract from the APK. Can be any file placed inside the "assets" directory of the Android project when the APK is built.
         * \return Returns true if the file is avaliable in the destinationDirectory.
         */
        static bool getAndroidAsset(JNIEnv* JNIEnvironment, const char destinationDirectory[], const char filename[]);
        
        /**
         * \brief Checks if OpenGL ES has reported any errors.
         * \param[in] operation The OpenGL ES function that has been called.
         */
        static void checkGlesError(const char* operation);
        
        /**
         * \brief Converts OpenGL ES error codes into the readable strings.
         * \param[in] glErrorCode The OpenGL ES error code to convert.
         * \return The string form of the error code.
         */
        static const char* glErrorToString(int glErrorCode);
        
        /** 
         * \brief Deep copy a string using memcopy().
         * \param[in] string The string to make a copy of.
         * \return A deep copy of the string.
         */
        static char* copyString(const char* string);
    };
}
#endif /* ANDROIDPLATFORM_H */