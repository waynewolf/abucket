LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	win_surface.cpp

#LOCAL_CFLAGS := -DGL_GLEXT_PROTOTYPES=1
LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
	liblog \
    libui \
    libgui \
	libbinder \
	libEGL \
	libGLESv2 \
	libandroid

LOCAL_MODULE:= win_surface

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)


include $(CLEAR_VARS)

LOCAL_SRC_FILES:= \
	pbuffer_surface.cpp

#LOCAL_CFLAGS := -DGL_GLEXT_PROTOTYPES=1
LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
	liblog \
    libui \
    libgui \
	libbinder \
	libEGL \
	libGLESv2 \
	libandroid

LOCAL_MODULE:= pbuffer_surface

LOCAL_MODULE_TAGS := tests

include $(BUILD_EXECUTABLE)
