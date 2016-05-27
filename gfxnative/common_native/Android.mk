LOCAL_PATH:= $(call my-dir)
include $(CLEAR_VARS)
LOCAL_SRC_FILES:= \
	AndroidPlatform.cpp \
	EGLRuntime.cpp \
	ETCHeader.cpp \
	HDRImage.cpp \
	JavaClass.cpp \
	Matrix.cpp \
	Platform.cpp \
	Shader.cpp \
	Text.cpp \
	Texture.cpp \
	Timer.cpp \
	models/CubeModel.cpp \
	models/PlaneModel.cpp \
	models/SphereModel.cpp \
	models/SuperEllipsoidModel.cpp \
	models/TorusModel.cpp
LOCAL_SHARED_LIBRARIES := \
	libcutils \
	libutils \
    libui \
    libgui \
	libbinder \
	libGLESv2 \
	libEGL
LOCAL_MODULE := libmalisdk_common_native
LOCAL_CFLAGS += -Wall -D__android__ -DGLES_VERSION=2 -fexceptions
LOCAL_C_INCLUDES += $LOCAL_PATH
include $(BUILD_SHARED_LIBRARY)