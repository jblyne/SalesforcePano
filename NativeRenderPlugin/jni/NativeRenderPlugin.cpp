/* Author: Sherif Salem
 * Date: 23-06-2015
 * About: Much of this code has been lifted from the unity rendering plugin example, found at the bottom of this
 * page: http://docs.unity3d.com/Manual/NativePluginInterface.html
 *
 * This plugin will take in a bitmap and display it to the assigned texture on the next UnityRenderEvent() call.
 * Meant to be as standalone as possible, there has been some attempt made to allow for iOS integration, but because
 * Android is the focus, it's likely things are and will be overlooked (no testing done on a iOS device). While this
 * is being built with the NDK, im sure that it can also be built for iOS with whatever toolchain it needs. Hopefully
 * that won't cause too much ballache.
 */

#include <jni.h>
#include "GLContainer.cpp"
#include "FFContainer.cpp"

jint JNI_OnLoad(JavaVM* vm, void* reserved) {
	LOGD("JNI attached.");
  JNIEnv* jni_env = 0;
  vm->AttachCurrentThread(&jni_env, 0);
  return (JNI_VERSION_1_6);
}

//TODO: Need to find out if there is a hook needed for iOS.

extern "C" void EXPORT_API UnitySetGraphicsDevice (void* _device, int _deviceType, int _eventType);

extern "C" void EXPORT_API UnityRenderEvent(int _eventID);

extern "C" int InitNativeVideo(char _fname[]);
