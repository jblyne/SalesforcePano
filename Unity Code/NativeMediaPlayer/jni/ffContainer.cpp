/* Need to find an audi decdoer for this.
 *
 *
 */

#include <android/log.h>
#include <string>

#include "GLContainer.cpp"

extern "C" {
	#include <libavcodec/avcodec.h>
	#include <libavformat/avformat.h>
	#include <libavutil/avutil.h>
}

#include <thread>

#undef LOG_TAG
#define LOG_TAG "FFPlayer"

#if LIBAVCODEC_VERSION_INT < AV_VERSION_INT(55,28,1)
#define av_frame_alloc avcodec_alloc_frame
#define av_frame_free avcodec_free_frame
#endif

using namespace std;

typedef struct VideoState {
	AVFormatContext *pFormatCtx;
	AVStream *pVideoStream;
	AVCodecContext *pCodecCtx;
	AVCodecContext *pCodecCtxOrig;
	AVCodec *pCodec;
	int videoStreamIdx;
}VideoState;

typedef struct VideoInfo {
	int width;
	int height;
	int duration;
	int frameRate;
	char fileName[];
}VideoInfo;

VideoState* gvs;
static VideoInfo* info = NULL;
static int init = 0;

static int DebugMsg(int _ident) {

	switch(_ident) {
	case (0):
		LOGD("Finished.");
		break;
	case (-1):
		LOGE("Failed to open file.");
		break;
	case (-2):
		LOGE("Couldn't find stream information.");
		break;
	case (-3):
		LOGE("Failed to find video stream.");
		break;
	case (-4):
		LOGE("Failed to find audio stream.");
		break;
	case (-5):
		LOGE("Video codec not found.");
		break;
	case (-6):
		LOGE("Audio codec not found");
		break;
	case (-7):
		LOGE("Couldn't open video codec.");
		break;
	case (-8):
		LOGE("Couldn't open audio codec.");
		break;
	case (-9):
		LOGE("Failed to allocate video frames.");
		break;
	default:
		LOGE(" ");
		break;
	}

	return (_ident);
}

//From GLContainer.cpp
extern void SetOutboundTexture(unsigned char* _tex);

extern "C" int PlayVideo(const char* _fname) {
	AVFormatContext *pFormatCtx;
	unsigned int    i, videoStream, audioStream;
	AVCodecContext  *pCodecCtx, *aCodecCtx;
	AVCodec         *pCodec, aCodec;
	AVFrame         *pFrame;
	AVFrame         *pFrameRGB;
	int             numBytes, totalFrames, frameLocation;
	uint8_t         *buffer;
	//char			fileName[1024];

	//strncpy(fileName, _fName.c_str(), sizeof(char) * 1024);
	//fileName[sizeof(fileName) - 1] = 0;

	av_register_all();

	//Open video file
	pFormatCtx = avformat_alloc_context();
	if (avformat_open_input(&pFormatCtx, _fname, NULL, NULL) != 0) {
		return (DebugMsg(-1)); //Couldn't open file
	}

	//Retrieve stream information
	if (avformat_find_stream_info(pFormatCtx, NULL) < 0) {
		return (DebugMsg(-2)); //Couldn't find stream information.
	}

	av_dump_format(pFormatCtx, 0, _fname, false);

	videoStream = audioStream = -1;

	//Find the first video stream.
	for (i = 0; i < pFormatCtx->nb_streams; i++) {
		if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {
			videoStream = i;
		}
		else if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_AUDIO) {
			audioStream = i;
		}
		if (videoStream != -1 && audioStream != -1) {
			break;
		}
	}
	if (videoStream == -1) {
		return (DebugMsg(-3)); //Failed to find video stream.
	}
	if (audioStream == -1) {
		return (DebugMsg(-4)); //Failed to find audio stream.
	}

	//Get a pointer to the codec contexts for the streams.
	pCodecCtx = pFormatCtx->streams[videoStream]->codec;
	aCodecCtx = pFormatCtx->streams[videoStream]->codec;

	//Get the duration
	double duration = double(pFormatCtx->duration) / AV_TIME_BASE;
	LOGD("Video duration: %i", (int)duration);

	//Find the video stream decoder
	pCodec = avcodec_find_decoder(pCodecCtx->codec_id);
	if (pCodec == NULL) {
		return (DebugMsg(-5));
	}

	//Find the audio stream decoder
	aCodec = avcodec_find_decoder(aCdecCtx->codec_id);
	if (aCodec == NULL) {
		return (DebugMsg(-6));
	}

	//Inform codec we can handle truncated bitstreams (can we?)
	//bitsreams with frame boundaries in the middle of packets.
	if(pCodec->capabilities & CODEC_CAP_TRUNCATED)
		pCodecCtx->flags|=CODEC_FLAG_TRUNCATED;

	//Open codec
	if (avcodec_open2(pCodecCtx, pCodec, NULL) < 0) {
		return (DebugMsg(-7));
	}

	LOGD("Using video codec: %s", pCodec->name);

	if (avcodec_open2(aCodecCtx, aCodec, NULL) < 0) {
		return (DebugMsg(-8));
	}

	LOGD("Using audio codec: %s", aCodec->name);

	//Allocate video frame + AVFrame structure
	pFrame = avcodec_alloc_frame();
	pFrameRGB = avcodec_alloc_frame();

	if (pFrameRGB == NULL || pFrame == NULL) {
		return (DebugMsg(-9));
	}

	// Determine required buffer size and allocate buffer
	numBytes = avpicture_get_size(PIX_FMT_RGB24, pCodecCtx->width, pCodecCtx->height);
	buffer = (uint8_t *)av_malloc(numBytes * sizeof(uint8_t));

	//Assign buffer parts to image planes in pFrameRGB
	avpicture_fill((AVPicture *)pFrameRGB, buffer, PIX_FMT_RGB24, pCodecCtx->width, pCodecCtx->height);

	totalFrames = pFormatCtx->streams[videoStream]->nb_frames;

	AVPacket packet;
	int frameFinished = 0;
	int pixelScanCount = 0;

	while (av_read_frame(pFormatCtx, &packet) >= 0) {
		LOGD("Reading frame");

		if (packet.stream_index == videoStream) {
			avcodec_decode_video2(pCodecCtx, pFrame, &frameFinished, &packet);
			if (frameFinished && init == 0) {
				SetOutboundTexture(pFrame->data[0]);
				init = 1;
			}
			av_free_packet(&packet);
		}
	}

	delete [] buffer;
	//av_free(pFrame);

	// Free the RGB frame
	//av_free(pFrameRGB);

	// Close the codec
	//avcodec_close(pCodecCtx);

	// Close the video file
	//avformat_close_input( (AVFormatContext **)pFormatCtx);

	return (DebugMsg(0));

}

extern int GetWidth() {
	if (info != NULL)
		return (info->width);
	else
		return (-1);
}
extern int GetHeight() {
	if (info != NULL)
			return (info->height);
		else
			return (-1);
}
extern int GetDuration() {
	if (info != NULL)
			return (info->duration);
		else
			return (-1);
}
extern int GetFrameRate() {
	if (info != NULL)
		return (info->frameRate);
	else
		return (-1);
}
extern int Init() {
	return (init);
}







