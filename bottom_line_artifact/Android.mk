LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	main.cpp

LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
	liblog \
    libgui \
	libbinder \
	libEGL \
    libGLESv2 \
    libandroid


LOCAL_MODULE:= bottom_line_artifact

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)

