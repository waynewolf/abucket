LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	rgb565_texture.cpp \
	rgb565.cpp \
	golden_bmp_argb.cpp

LOCAL_SHARED_LIBRARIES := \
	libcutils \
    libutils \
    libui \
    libgui \
    libbinder \
    libEGL \
    libGLESv2 \
    libandroid

LOCAL_MODULE := test-rgb565-texture-readback

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)
