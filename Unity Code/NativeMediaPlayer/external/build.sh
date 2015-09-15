#!/bin/bash

#config vars
NDK=/opt/android-ndk
HOST_ARCH=linux-x86_64
ANDROID_API_LEVEL=android-9
TARGET_ABIS=(x86 armeabi-v7a)

OUT=`pwd`/out
SOURCE=`pwd`/ffmpeg

function build_ffmpeg
{
./configure \
    --prefix=$OUT/$TARGET_ARCH_ABI \
    --enable-shared \
    --disable-static \
    --disable-doc \
    --disable-programs \
    --disable-symver \
    --cross-prefix=$TOOLCHAIN/bin/$TOOLCHAIN_PREFIX- \
    --target-os=linux \
    --arch=$TARGET_ARCH \
    --cpu=$TARGET_CPU \
    --enable-cross-compile \
    --sysroot=$SYSROOT \
    --extra-cflags="-Os -fpic $ADDI_CFLAGS" \
    --extra-ldflags="$ADDI_LDFLAGS" $ADDI_FFMPEG_FLAGS | tee $WORKINGDIR/logs/configure-$TARGET_ARCH_ABI.txt
make clean
make -j8
make install
}

WORKINGDIR=`pwd`

git submodule --quiet update --init &> /dev/null
cd ffmpeg
git reset --hard
git checkout release/2.0
patch -p1 < ../slibnames.patch
SOURCE=`pwd`

for TARGET_ARCH_ABI in $TARGET_ABIS; do
    case $TARGET_ARCH_ABI in
    armeabi-v7a)
        TOOLCHAIN_PREFIX=arm-linux-androideabi
        TOOLCHAIN_FULLNAME=arm-linux-androideabi-4.8
        ADDI_CFLAGS="-Wl,--fix-cortex-a8 -marm -march=armv7-a -mfpu=vfpv3-d16 -mfloat-abi=softfp"
        ADDI_FFMPEG_FLAGS="--disable-neon"
	TARGET_ARCH=arm
	TARGET_CPU=armv7-a
    ;;
    x86)
        TOOLCHAIN_PREFIX=i686-linux-android
	TOOLCHAIN_FULLNAME=x86-4.8
        ADDI_CFLAGS="-mtune=atom -mssse3 -mfpmath=sse"
        ADDI_FFMPEG_FLAGS="--disable-avx"
	TARGET_ARCH=x86
	TARGET_CPU=atom
    ;;
    esac
    SYSROOT=$NDK/platforms/$ANDROID_API_LEVEL/arch-$TARGET_ARCH/
    TOOLCHAIN=$NDK/toolchains/$TOOLCHAIN_FULLNAME/prebuilt/$HOST_ARCH

    build_ffmpeg
done
