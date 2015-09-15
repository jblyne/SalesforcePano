LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE    := NativeMediaPlayer
LOCAL_SRC_FILES := NativeMediaPlayer.cpp
LOCAL_LDLIBS := -llog -lz -lEGL -lGLESv2 
LOCAL_SHARED_LIBRARIES := libavformat libavcodec libavutil liblog libswscale
LOCAL_CXXFLAGS += -D__STDC_CONSTANT_MACROS

ifndef NDK_ROOT
include external/stlport/libstlport.mk
endif

LOCAL_SHARED_LIBRARIES += libstlport

include $(BUILD_SHARED_LIBRARY)

$(call import-add-path, ../)
$(call import-module, NativeMediaPlayer/external)