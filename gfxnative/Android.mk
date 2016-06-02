LOCAL_PATH := $(call my-dir)
SAVED_LOCAL_PATH := $(LOCAL_PATH)
COMMON_NATIVE_PATH := $(LOCAL_PATH)/common_native
LIBGFXNATIVE_PATH := $(LOCAL_PATH)/libgfxnative

# Include all libs (built and prebuilt)
include $(CLEAR_VARS)
include $(COMMON_NATIVE_PATH)/Android.mk

include $(CLEAR_VARS)
include $(LIBGFXNATIVE_PATH)/Android.mk

LOCAL_PATH := $(SAVED_LOCAL_PATH)
include $(CLEAR_VARS)

include $(CLEAR_VARS)
LOCAL_SRC_FILES:= \
	partial_update.cpp
LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
    libgui \
	libbinder \
	libGLESv2 \
	libEGL \
	libgfxnative \
	libmalisdk_common_native
LOCAL_MODULE := partial_update
LOCAL_CFLAGS += -Wall -D__android__ -DGLES_VERSION=3 -fexceptions
LOCAL_C_INCLUDES += $LOCAL_PATH $(LOCAL_PATH)/common_native $(LOCAL_PATH)/libgfxnative
include $(BUILD_EXECUTABLE)