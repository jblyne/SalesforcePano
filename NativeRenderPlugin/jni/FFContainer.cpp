/*
 * This file will handle the talking between the C++ 'meat' of the library
 * and the C bridge file for Unity. It's a bit of abstraction hell, but the
 * advantage of using classes outweighs this little downside.
 *
 * NativeMediaPlayer.c <=> c_cppBridge.cpp <=> <the modern world>
 *
 */

#include <android/log.h>
#include <string>

extern "C" {
	#include <libavcodec/avcodec.h>
	#include <libavformat/avformat.h>
	#include <libavutil/avutil.h>
}

#define LOG_TAG "FFPlayer"
#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)
#define LOGE(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

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

static int PlayVideo(const char *_fName);

//----------------------------------------------------------------------------
// External methods

extern "C" int InitNativeVideo(char _fname[]) {

	const char* name = "/storage/emulated/0/DCIM/Camera/VID_20150528_144533.mp4";

	if (init == 0) {
		LOGD("Initialising player with string: %s", name);
		return (PlayVideo(name));
	}
	else {
		LOGE("Player already initialised. Destroy current player before creating a new one.");
		return (-1);
	}
	return (0);
}

extern int Width() {
	if (info != NULL)
		return (info->width);
	else
		return (-1);
}
extern int Height() {
	if (info != NULL)
			return (info->height);
		else
			return (-1);
}
extern int Duration() {
	if (info != NULL)
			return (info->duration);
		else
			return (-1);
}
extern int FrameRate() {
	if (info != NULL)
		return (info->frameRate);
	else
		return (-1);
}
extern int Init() {
	return (init);
}

//---------------------------------------------------------------------------
// Static Methods

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
		LOGE("Failed to find stream.");
		break;
	case (-4):
		LOGE("Codec not found.");
		break;
	case (-5):
		LOGE("Couldn't open codec.");
		break;
	case (-6):
		LOGE("Failed to allocate video frames.");
		break;
	default:
		LOGE(" ");
		break;
	}

	return (_ident);
}

static int PlayVideo(const char *_fName) {
	AVFormatContext *pFormatCtx;
	unsigned int    i, videoStream;
	AVCodecContext  *pCodecCtx;
	AVCodec         *pCodec;
	AVFrame         *pFrame;
	AVFrame         *pFrameRGB;
	int             numBytes, totalFrames, frameLocation;
	uint8_t         *buffer;

	av_register_all();

	//Open video file
	pFormatCtx = avformat_alloc_context();
	if (avformat_open_input(&pFormatCtx, _fName, NULL, NULL) != 0) {
		return (DebugMsg(-1)); //Couldn't open file
	}

	//Retrieve stream information
	if (avformat_find_stream_info(pFormatCtx, NULL) < 0) {
		return (DebugMsg(-2)); //Couldn't find stream information.
	}

	av_dump_format(pFormatCtx, 0, _fName, false);

	videoStream = -1;

	//Find the first video stream.
	for (i = 0; i < pFormatCtx->nb_streams; i++) {
		if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {
			videoStream = i;
			break;
		}
	}
	if (videoStream == -1) {
		return (DebugMsg(-3)); //Failed to find stream
	}

	//Get a pointer to the codec context for the stream.
	pCodecCtx = pFormatCtx->streams[videoStream]->codec;

	//Get the duration
	double duration = double(pFormatCtx->duration) / AV_TIME_BASE;
	LOGD("Video duration: %i", (int)duration);

	//Find the stream decoder
	pCodec = avcodec_find_decoder(pCodecCtx->codec_id);
	if (pCodec == NULL) {
		return (DebugMsg(-4));
	}

	//Inform codec we can handle truncated bitstreams (can we?)
	//bitsreams with frame boundaries in the middle of packets.
	if(pCodec->capabilities & CODEC_CAP_TRUNCATED)
		pCodecCtx->flags|=CODEC_FLAG_TRUNCATED;

	//Open codec
	if (avcodec_open2(pCodecCtx, pCodec, NULL) < 0) {
		return (DebugMsg(-5));
	}

	LOGD("Using codec: %s", pCodec->name);

	//Allocate video frame + AVFrame structure
	pFrame = avcodec_alloc_frame();
	pFrameRGB = avcodec_alloc_frame();

	if (pFrameRGB == NULL || pFrame == NULL) {
		return (DebugMsg(-6));
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
			if (frameFinished) {
				LOGD("Got a frame");
			}
			else {
				LOGD("Didn't get a frame");
			}
			av_free_packet(&packet);
		}
	}

	delete [] buffer;
	av_free(pFrameRGB);

	// Free the YUV frame
	av_free(pFrame);

	// Close the codec
	avcodec_close(pCodecCtx);

	// Close the video file
	avformat_close_input( (AVFormatContext **)pFormatCtx);

	return (DebugMsg(0));

}


/*
static int initPlayer(char _fName[]) {

	if ( access(_fName, F_OK) == -1) {
		LOGE("File not found");
		return (-1);
	}

	char msg[255];
	LOGD("Beggining intialisation");
	info = malloc(sizeof(VideoInfo) + (sizeof(char) * strlen(_fName)));
	strncpy(info->fileName, _fName, strlen(_fName));
	av_register_all();
	VideoState *vs;
	vs = av_mallocz(sizeof(VideoState));
	gvs = vs;
	//Open Video File
	LOGD("File name: %s", _fName);
	const char* fPtr = &info->fileName[0];
	int res = avformat_open_input(&vs->pFormatCtx, fPtr, NULL, NULL);
	if (res != 0) {
		av_make_error_string(msg, 255, res);
		LOGE("Error opening video file: %s", msg);
		return (res);
	}
	LOGD("File opened, retrieving stream info...");
	//Retrieve stream info
	res = avformat_find_stream_info(vs->pFormatCtx, NULL);
	if (res != 0) {
		LOGE("Error retirieving stream info");
		return (res);
	}
	LOGD("Stream info found, dumping info...");
	//Dump file info into standard error
	av_dump_format(vs->pFormatCtx, 0, msg, 0);
	LOGD("%s", msg);
	LOGD("Finding video stream...");
	//Find video stream
	int i;
	vs->pCodecCtxOrig = NULL;
	//Find first stream in the list
	vs->videoStreamIdx = -1;
	for (i = 0; i < vs->pFormatCtx->nb_streams; ++i) {
		if (vs->pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {
			vs->videoStreamIdx = i;
			vs->pVideoStream = vs->pFormatCtx->streams[i];
			break;
		}
	}
	//if we dont find a video stream, exit.
	if (vs->videoStreamIdx == -1) {
		LOGE("No stream found");
		return (-1);
	}
	LOGD("Stream found, fetching decoder...");
	//Get pointer to the codec context from stream
	vs->pCodecCtx = vs->pFormatCtx->streams[vs->videoStreamIdx]->codec;

	//Get decoder for the video stream
	vs->pCodec = avcodec_find_decoder(vs->pCodecCtx->codec_id);
	if (vs->pCodec == NULL) {
		LOGE("No supported codec found");
		return (-1);
	}
	LOGD("Decoder found, copying codec context...");
	//Copy context
	vs->pCodecCtxOrig = avcodec_alloc_context3(vs->pCodec);
	if (avcodec_copy_context(vs->pCodecCtx, vs->pCodecCtxOrig) != 0) {
		LOGE("Failed to copy codec context");
		return (-1);
	}
	LOGD("Codec context copied.");
	if(vs->pCodec->capabilities & CODEC_CAP_TRUNCATED)
		vs->pCodecCtx->flags|=CODEC_FLAG_TRUNCATED;

	LOGD("Using codec: %s", vs->pCodec->name);


	//Get video info.
	info->height = vs->pVideoStream->codec->height = 1080;
	info->width = vs->pVideoStream->codec->width = 1920;
	info->duration = vs->pFormatCtx->duration / AV_TIME_BASE;
	int fr;
		 if(vs->pVideoStream->avg_frame_rate.den && vs->pVideoStream->avg_frame_rate.num) {
		 fr = av_q2d(vs->pVideoStream->avg_frame_rate);
	 }
	 else if(vs->pVideoStream->r_frame_rate.den && vs->pVideoStream->r_frame_rate.num) {
		 fr = av_q2d(vs->pVideoStream->r_frame_rate);
	 }
	 else if(vs->pVideoStream->time_base.den && vs->pVideoStream->time_base.num) {
		 fr = 1/av_q2d(vs->pVideoStream->time_base);
	 }
	 else if(vs->pVideoStream->codec->time_base.den && vs->pVideoStream->codec->time_base.num) {
		 fr = 1/av_q2d(vs->pVideoStream->codec->time_base);
	 }
	 info->frameRate = fr;
	 LOGD("Video dimensions: %i x %i", vs->pVideoStream->codec->width, vs->pVideoStream->codec->height);
	 LOGD("Video duration: %i", (int)(vs->pFormatCtx->duration / 1000));
	 LOGD("Frame rate: %i", fr);
	 LOGD("Video player is ready to play.");
	 init = 1;
	 return (0);
}


extern int DecodeFrame() {
	VideoState *vs;
	vs = gvs;
	//open the codec
	if (avcodec_open2(vs->pCodecCtx, vs->pCodec, NULL) < 0) {
		LOGE("Failed to open codec");
		return (-1);
	}
	LOGD("Codec opened, allocating frame storage...");

	AVFrame *pFrame = NULL;
	AVFrame *pFrameRGB = NULL;

	//Allocate frames
	pFrame = av_frame_alloc();
	pFrameRGB = av_frame_alloc();

	if (pFrame == NULL || pFrameRGB == NULL) {
		LOGE("av_frame_malloc() has failed.");
		return (-1);
	}

	uint8_t *buffer = NULL;
	int numBytes;

	//Determine the required size and allocate buffer.
	numBytes = avpicture_get_size(PIX_FMT_RGB24, info->width, info->height);
	buffer = (uint8_t *)av_malloc(numBytes * sizeof(uint8_t));

	//Assign appropriate parts of buffer to image planes in pFrameRGB
	//pFrameRGB is an AVFrame, but AVFrame is a superset of AVPicture
	avpicture_fill((AVPicture *)pFrameRGB, buffer, PIX_FMT_RGB24, info->width, info->height);

	LOGD("Buffers ready. Preparing to read frames...");

	struct SwsContext *sws_ctx = NULL;
	int frameFinished = 0;
	AVPacket packet;

	//init SWS context for software scaling
	//sws_ctx = sws_getCachedContext(sws_ctx, info->width, info->height, vs->pCodecCtx->pix_fmt, info->width, info->height, PIX_FMT_RGB24, SWS_BILINEAR, NULL, NULL, NULL);

	LOGD("Reading frames...");
	while (av_read_frame(vs->pFormatCtx, &packet) >=0) {

		//is this a packet from the stream?
		if (packet.stream_index == vs->videoStreamIdx) {
			//LOGD("Stream packet found, decoding...");
			//Decode frame
			LOGD("%i | %i", avcodec_decode_video2(vs->pCodecCtx, pFrame, &frameFinished, &packet), packet.flags);

			//Did we get a video frame?
			if (frameFinished) {
				//LOGD("Got a frame");
				//Convert to RGB
				//sws_scale(sws_ctx, (uint8_t const * const *) pFrame->data,
				//		pFrame->linesize,
				//		0,
				//		info->height,
				//		pFrameRGB->data,
				//		pFrameRGB->linesize);

				//This is the bit where the frame gets passed to a GL tex unit.
				//unsigned char* data = (unsigned char*)pFrame->data;
				//SetOutboundTexture(data);
			}
			else {
				//LOGD("Not got a frame");
			}
			av_free_packet(&packet);
		}
	}
	av_free(buffer);
	av_free(pFrameRGB);
	av_free(pFrame);
	LOGD("Playing finished");
	return (0);
}


extern void DestroyPlayer() {
	if (init == 0) {
		LOGE("No active player to destroy");
	}
	else {
		av_free(gvs);
		gvs = NULL;
		free(info);
		info = NULL;
		init = 0;
	}
}

*/








