LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	main.cpp \
	nv21_960x540.cpp

LOCAL_SHARED_LIBRARIES := \
	libcutils \
    libutils \
    libui \
    libgui \
    libbinder \
    libEGL \
    libGLESv2 \
    libandroid

LOCAL_MODULE := odd-height-yuv

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)
