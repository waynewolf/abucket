LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)
LOCAL_SRC_FILES:= \
	utils.cpp \
	NativeWinApp.cpp
LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
    libgui \
	libbinder \
	libGLESv2 \
	libEGL
LOCAL_MODULE := libgfxnative
LOCAL_CFLAGS += -Wall -D__android__ -DGLES_VERSION=3 -fexceptions
LOCAL_C_INCLUDES += $LOCAL_PATH $(LOCAL_PATH)/../common_native
include $(BUILD_SHARED_LIBRARY)