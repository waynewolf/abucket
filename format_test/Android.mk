LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	egl_image_test.cpp

LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
    libgui \
	libbinder

LOCAL_MODULE:= egl_image_test

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)

include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	egl_surface_test.cpp

LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
    libgui \
	libbinder \
	libEGL \
	libGLESv2 \
	libandroid

LOCAL_MODULE:= egl_surface_test

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)
