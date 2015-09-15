using UnityEngine;
using System.Collections;

//Provides a wrapper for all calls to external Java plugins, and will store its outputs
//to allow unity access to them.

public class JavaUnityInterface : MonoBehaviour {
#if UNITY_ANDROID
	private AndroidJavaObject m_ImageResizer = null;
	private AndroidJavaObject m_ActivityContext = null;
	private AndroidJavaObject m_GalleryPathGrabber = null;
	
	private byte[] m_ImageBytes;
	private int m_Width, m_Height;
	private string m_Country, m_Date;

	public byte[] Image {
		get { return m_ImageBytes; }
	}

	public int Width {
		get { return m_Width; }
	}

	public int Height {
		get { return m_Height; }
	}

	public string Country {
		get { return m_Country; }
	}

	public string Date { 
		get { return m_Date; }
	}

	void Start() {
		m_ImageBytes = null;
		m_Width = m_Height = -1;
	}

	public bool DecodeImage( string _targetFile ) {
		if ( m_ImageResizer == null ) 
			m_ImageResizer = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.BitmapResizer", GetActivityContext() );

		bool success = m_ImageResizer.Call<bool>( "DecodeSampledBitmapFromFile", _targetFile );
		if (success)
			LoadParams();
		
		m_ImageResizer = null;
		return success;
	}

	public string[] GetImagePaths() {
		string[] ret;
	
		m_GalleryPathGrabber = new AndroidJavaObject( "com.sherif.cardboard3d.bitmaphandler.GalleryInterface", GetActivityContext() );
		ret = m_GalleryPathGrabber.Call<string[]>( "GetGalleryImagePaths" );
	
		return ret;
	}

	private void LoadParams() {
		m_ImageBytes	= m_ImageResizer.Call<byte[]>("GetImage");
		m_Width			= m_ImageResizer.Call<int>("Width");
		m_Height		= m_ImageResizer.Call<int>("Height");
		m_Date			= m_ImageResizer.Call<string>("Date");
		m_Country		= m_ImageResizer.Call<string>("Country");
	}

	private AndroidJavaObject GetActivityContext() {
		if ( m_ActivityContext == null ) {
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			m_ActivityContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
		}
		return m_ActivityContext;
	}
#endif
}