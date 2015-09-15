
//Future support for iOS?

#include <GLES2/gl2.h>
#include <math.h>
#include <android/log.h>

#define LOG_TAG "NativeRenderer"
#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)
#define LOGE(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

#define EXPORT_API

//------------------------------------------------------------------------------------------------
// Shader definition + declaration.

#define VPROG_SRC(ver, attr, varying)								\
	ver																\
	attr " highp vec3 pos;\n"										\
	attr " lowp vec4 color;\n"										\
	"\n"															\
	varying " lowp vec4 ocolor;\n"									\
	"\n"															\
	"uniform highp mat4 worldMatrix;\n"								\
	"uniform highp mat4 projMatrix;\n"								\
	"\n"															\
	"void main()\n"													\
	"{\n"															\
	"	gl_Position = (projMatrix * worldMatrix) * vec4(pos,1);\n"	\
	"	ocolor = color;\n"											\
	"}\n"															\

static const char* kGlesVShaderTextGLES2		= VPROG_SRC("\n", "attribute", "varying");
static const char* kGlesVShaderTextGLES3		= VPROG_SRC("#version 300 es\n", "in", "out");

#undef VPROG_SRC

#define FSHADER_SRC(ver, varying, outDecl, outVar)	\
	ver												\
	outDecl											\
	varying " lowp vec4 ocolor;\n"					\
	"\n"											\
	"void main()\n"									\
	"{\n"											\
	"	" outVar " = ocolor;\n"						\
	"}\n"											\

static const char* kGlesFShaderTextGLES2	= FSHADER_SRC("\n", "varying", "\n", "gl_FragColor");
static const char* kGlesFShaderTextGLES3	= FSHADER_SRC("#version 300 es\n", "in", "out lowp vec4 fragColor;\n", "fragColor");

#undef FSHADER_SRC

//----------------------------------------------------------------------------------------------
// Static variable initialisation

struct Vertex {
	float x, y, z;
	unsigned int color;
};

static float unityTime 	= 0.0f;
static void* texPtr 	= 0;
static int texWidth 	= 0;
static int texHeight 	= 0;
static int deviceType	= -1;
static bool renderEv	= false;
static GLuint	g_VProg;
static GLuint	g_FShader;
static GLuint	g_Program;
static int		g_WorldMatrixUniformIndex;
static int		g_ProjMatrixUniformIndex;
static unsigned char* outTex = 0;

//--------------------------------------------------------------------------------------------
// Internal method declarations - is this needed?

static GLuint CreateShader(GLenum _type, const char* _text);
static void SetDefaultGraphicsState();
static void FillTextureFromCode(int _width, int _height, int _stride, unsigned char* _dst);
static void Render(const float* _worldMatrix, const float* _identMatrix, float* _projMatrix, const Vertex* _verts);

//--------------------------------------------------------------------------------------------
// External methods

extern "C" void SetTimeFromUnity(float _t) {
	unityTime = _t;
}

extern "C" void SetTexFromUnity(void* _texPtr, int _w, int _h) {
	LOGD("Texptr from unity is: %i", (int)_texPtr);
	texPtr 		= _texPtr;
	texWidth 	= _w;
	texHeight 	= _h;
}

extern "C" void EXPORT_API UnityRenderEvent(int _eventID) {
	if (!renderEv) {
		LOGD("Render event triggered");
		renderEv = true;
	}
	if(deviceType == -1) {
		LOGE("Incorrect device type found");
		return;
	}

	Vertex verts[3] = {
		{ -0.5f, -0.25f,  0, 0xFFff0000 },
		{  0.5f, -0.25f,  0, 0xFF00ff00 },
		{  0,     0.5f ,  0, 0xFF0000ff },
	};

	float phi = unityTime;
	float cosPhi = cosf(phi);
	float sinPhi = sinf(phi);

	float worldMatrix[16] = {
		cosPhi,-sinPhi,0,0,
		sinPhi,cosPhi,0,0,
		0,0,1,0,
		0,0,0.7f,1,
	};
	float identMatrix[16] = {
		1,0,0,0,
		0,1,0,0,
		0,0,1,0,
		0,0,0,1,
	};
	float projMatrix[16] = {
		1,0,0,0,
		0,1,0,0,
		0,0,1,0,
		0,0,0,1,
	};

	SetDefaultGraphicsState();
	Render(worldMatrix, identMatrix, projMatrix, verts);
}

extern "C" void EXPORT_API UnitySetGraphicsDevice(void* _device, int _deviceType, int _eventType) {
	deviceType = -1;

	if(_deviceType == 8)
	{
		LOGD("OpenGLES 2.0 device");
		deviceType = _deviceType;
		g_VProg		= CreateShader(GL_VERTEX_SHADER, kGlesVShaderTextGLES2);
		g_FShader	= CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES2);
	}
	else if(deviceType == 11)
	{
		LOGD("OpenGLES 3.0 device");
		deviceType = _deviceType;
		g_VProg		= CreateShader(GL_VERTEX_SHADER, kGlesVShaderTextGLES3);
		g_FShader	= CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES3);
	}
	else {
		LOGE("Incorrect device type found");
	}

	g_Program = glCreateProgram();
	glBindAttribLocation(g_Program, 1, "pos");
	glBindAttribLocation(g_Program, 2, "color");
	glAttachShader(g_Program, g_VProg);
	glAttachShader(g_Program, g_FShader);
	glLinkProgram(g_Program);

	g_WorldMatrixUniformIndex	= glGetUniformLocation(g_Program, "worldMatrix");
	g_ProjMatrixUniformIndex	= glGetUniformLocation(g_Program, "projMatrix");
}

extern "C" void SetOutboundTexture(unsigned char* _tex) {
	LOGE("Texptr is: %i", (int)texPtr);
	outTex = _tex;
}

//--------------------------------------------------------------------------------------------
// Internal methods

static GLuint CreateShader(GLenum _type, const char* _text) {
	GLuint ret = glCreateShader(_type);
	glShaderSource(ret, 1, &_text, NULL);
	glCompileShader(ret);

	return (ret);
}

static void SetDefaultGraphicsState() {
	if(deviceType == -1) {
		LOGE("Incorrect device type found");
		return;
	}

	glDisable(GL_CULL_FACE);
	glDisable(GL_BLEND);
	glDepthFunc(GL_LEQUAL);
	glEnable(GL_DEPTH_TEST);
	glDepthMask(GL_FALSE);
}

static void FillTextureFromCode(int _width, int _height, int _stride, unsigned char* _dst) {
	const float t = unityTime * 4.0f;

	for (int y = 0; y < _height; ++y)
	{
		unsigned char* ptr = _dst;
		for (int x = 0; x < _width; ++x)
		{
			// Simple oldskool "plasma effect", a bunch of combined sine waves
			int vv = int(
				(127.0f + (127.0f * sinf(x/7.0f+t))) +
				(127.0f + (127.0f * sinf(y/5.0f-t))) +
				(127.0f + (127.0f * sinf((x+y)/6.0f-t))) +
				(127.0f + (127.0f * sinf(sqrtf(float(x*x + y*y))/4.0f-t)))
				) / 4;
			// Write the texture pixel
			ptr[0] = vv;
			ptr[1] = vv;
			ptr[2] = vv;
			ptr[3] = vv;

			// To next pixel (our pixels are 4 bpp)
			ptr += 4;
		}

		// To next image row
		_dst += _stride;
	}
}

static void Render(const float* _worldMatrix, const float* _identMatrix, float* _projMatrix, const Vertex* _verts) {
	if(deviceType == -1) {
		LOGE("Incorrect device type found");
		return;
	}

	_projMatrix[10] = 2.0f;
	_projMatrix[14] = -1.0f;

	glUseProgram(g_Program);
	glUniformMatrix4fv(g_WorldMatrixUniformIndex, 1, GL_FALSE, _worldMatrix);
	glUniformMatrix4fv(g_ProjMatrixUniformIndex, 1, GL_FALSE, _projMatrix);

	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
	glBindBuffer(GL_ARRAY_BUFFER, 0);

	const int stride = 3*sizeof(float) + sizeof(unsigned int);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, stride, (const float*)_verts);

	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 2, GL_UNSIGNED_BYTE, GL_TRUE, stride, (const float*)_verts + 3);

	glDrawArrays(GL_TRIANGLES, 0, 3);

	// update native texture from code
	if (texPtr)
	{
		GLuint gltex = (GLuint)(size_t)(texPtr);
		glBindTexture(GL_TEXTURE_2D, gltex);

		if (outTex != 0) {
			glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, texWidth, texHeight, GL_RGBA, GL_UNSIGNED_BYTE, outTex);
		}
		else {
			unsigned char* data = new unsigned char[texWidth*texHeight*4];
			FillTextureFromCode(texWidth, texHeight, texHeight*4, data);
			glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, texWidth, texHeight, GL_RGBA, GL_UNSIGNED_BYTE, data);
			delete[] data;
		}
	}
}






















