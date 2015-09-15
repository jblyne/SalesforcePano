using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

// For Salesforce specifically
using Boomlagoon.JSON;
using SFDC;

//Handles the importing of files external to the application, by interacting with the
//Java-Unity Interface class or the IOS-Unity interface class.

public class FileHandler : MonoBehaviour {

	// For Salesforce
	public Salesforce sf;
	public string username;
	public string password;
	public string securityToken; 
	public string attachmentID;
	public string attachmentIDString;
	public byte[] textureBytes;
	// holder for responses from the REST API
	public string response = null;

	public class Thumbnail {

		private Texture2D m_Thumb;
		private Texture2D m_FullThumb; // Added by me
		private string m_ImageLoc;
		private int m_Width;
		private int m_Height;
		private string m_Country;
		private DateTime m_Date;

		public int GetTexPtr {
			get { return m_Thumb.GetNativeTextureID(); }
		}

		public int GetFullTexPtr {
			get { return m_FullThumb.GetNativeTextureID(); } // Added by me
		}

		public Texture2D Thumb {
			get { return m_Thumb; }
		}

		public Texture2D ThumbFull {
			get { return m_FullThumb; } // Added by me
		}

		public string ImageLoc {
			get { return m_ImageLoc; }
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

		public DateTime Date {
			get { return m_Date; }
		}

		public string DateString {
			get {
				if ( m_Date == new DateTime( 1970, 1, 1 ) ) {
					return "";
				}
				else {
					return m_Date.ToString("d", CultureInfo.CurrentCulture);
				}
			}
		}

		// Added by me
		public Thumbnail( Texture2D _Thumb, Texture2D _ThumbFull, string _country, string _date ) {
			m_Thumb = _Thumb;
			m_FullThumb = _ThumbFull;
			m_Width = _Thumb.width;
			m_Height = _Thumb.height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}

		public Thumbnail( Texture2D _Thumb, string _ImageLoc, string _country, string _date ) {
			m_Thumb = _Thumb;
			m_ImageLoc = _ImageLoc;
			m_Width = _Thumb.width;
			m_Height = _Thumb.height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}
		public Thumbnail( byte[] _Thumb, string _ImageLoc, int _width, int _height, string _date, string _country ) {
			m_Thumb = new Texture2D( _width, _height );
			m_Thumb.LoadImage( _Thumb );
			m_ImageLoc = _ImageLoc;
			m_Width = _width;
			m_Height = _height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime(1970, 1, 1);
			}
			else { 
				m_Date = DateFormat.DateTime( _date );
			}
		}
		public Thumbnail( Texture2D _tex, string _ImageLoc, int _width, int _height, string _date, string _country ) {
			m_Thumb = _tex;
			m_Width = _width;
			m_Height = _height;
			m_Country = _country;
			if ( _date == "" ) {
				m_Date = new DateTime( 1970, 1, 1 );
			}
			else {
				m_Date = DateFormat.DateTime( _date );
			}
		}
	}
	
	[SerializeField]
	private JavaUnityInterface m_JUInterface;

	[SerializeField]
	private iOSUnityInterface m_IUInterface;


	private List<Thumbnail> m_Thumbs;

	public int NumThumbs {
		get { return m_Thumbs.Count; }
	}

	public Thumbnail[] GetThumbs() {
		UpdateImages();
		return m_Thumbs.ToArray();
	}

#if !UNITY_EDITOR
	private string[] m_Textures;

	public int TexCount {
		get { return m_Textures.Length; }
	}
#endif
	/*
	// For testing purposes. Loads images in from the app directly.
	void Start() {
		m_Thumbs = new List<Thumbnail>();
		//#if UNITY_EDITOR
		for (int o = 0; o < 2; o++ ) {
			for ( int i = 0; i < 6; ++i ) {
				Thumbnail temp = new Thumbnail( Resources.Load( "Textures/Photosphere00" + i ) as Texture2D, "Textures/Photosphere00" + i, Country(), Date() );
				
				m_Thumbs.Add( temp );
			}
		}
	}
	*/

	public IEnumerator Start() {

		m_Thumbs = new List<Thumbnail>();
		
		// Set up the Salesforce object
		sf = gameObject.AddComponent<Salesforce>();
		
		username = "jblyne@gmail.com.dev";
		password = "P@ssword888888";
		
		// login
		sf.login(username, password + securityToken);
		
		// wait for Auth Token
		while(sf.token == null)
			yield return new WaitForSeconds(0.1f);
		
		// Query all Pano records so that we can get their attachments later.
		sf.query("SELECT id FROM overpower__Panorama__c");
		
		// wait for query results
		while(sf.response == null)
			yield return new WaitForSeconds(0.1f);
		
		//Debug.Log ("Panos:" + sf.response);
		
		// Extract the JSON encoded value for the Store the ID in a field.
		// We are using the free add-in from the Unity3D Asset STore called BoomLagoon.
		// Using BoomLagoon, create a JSON Object from the Salesforce response.
		JSONObject json = JSONObject.Parse(sf.response);
		
		// Retrieve the records array (only one record is returned) and traverse that record's 
		// attributes to get the case Id and Surgery Type
		JSONArray records = json.GetArray ("records");
		//Debug.Log ("records = " + records);
		
		attachmentIDString = ""; // Reset
		
		foreach (JSONValue row in records) {
			JSONObject rec = JSONObject.Parse(row.ToString()); 
			
			//Debug.Log("Id: " + rec.GetString("Id"));

			// Get the ID
			attachmentID = rec.GetString("Id");
			// Build the id string to use later in a SOQL query.
			attachmentIDString = attachmentIDString + "'" + attachmentID + "'" + ",";
		}

		// Remove trailing comma before using in the SOQL query below.
		attachmentIDString = attachmentIDString.TrimEnd(',');
		//Debug.Log ("attachmentIDString: " + attachmentIDString);
		
		int attachmentLimit = 	10;
		
		// Query Salesforce for all Pano attachments.
		sf.query ("SELECT Id, Name, Body FROM Attachment WHERE ParentId IN (" + attachmentIDString + ") LIMIT " + attachmentLimit);
		
		// wait for query results
		while(sf.response == null)
			yield return new WaitForSeconds(0.1f);
		
		Debug.Log("Pano Attachments = " + sf.response);
		
		// Using BoomLagoon, parse the JSON response .
		json = JSONObject.Parse(sf.response);
		
		// Retrieve the records array 
		// Traverse through each record to obtain the link to the attachment body
		records = json.GetArray ("records");
		Debug.Log ("records = " + records);
		
		int i = 1; // Loop through all returned Pano links and get the body for each.
		foreach (JSONValue row in records) {
			JSONObject rec = JSONObject.Parse(row.ToString()); 
			Debug.Log ("Body Link = " + rec.GetString("Body"));
			
			// get the attachment and store in the Texture Array
			getAttachmentBody (rec.GetString("Body"),i);
			
			i++;
		}
	}

	/* @author Ammar Alammar
		* @date 2014-07-05
		* retriefes an attachment body (Base64 Blob) from salesforce.com. The results are stored 
		* in the response variable.
		* 
		* @param url Executing a SOQL query on Attachmnets that selects the Body field will return 
		*            a URL, which in turn contains the BAse64 body (retrieve via a GET operation)
		*            Review CoRoutines in Unity's documentation. It is a specifically important concept
		* 			  in a game development enviornment, where methods can not block the processing workflow.
		* 			  In Sumamry, a coroutine allows the execution to be returned to the uninty engine and free-up the frame.
		*  		  When the next frame is executed, the code in the coroutine continues from where it left off. 
		* 			  This emulates "context switching" or interleaving of the blocking method's processing and the rest of the envionrment.
		*/
	public void getAttachmentBody(string url, int seq){
		
		WWWForm form = new WWWForm();			
		Hashtable headers = form.headers;
		headers["Authorization"] = "Bearer " + sf.token;
		headers["Content-Type"] = "application/json";
		headers["Method"] = "GET";
		headers["X-PrettyPrint"] = 1;
		WWW www = new WWW(sf.instanceUrl + url, null, headers);

		//Debug.Log ("sf.token: " + sf.token);
		//Debug.Log ("file handler instanceUrl: " + sf.instanceUrl);
		
		StartCoroutine(executeDownload(www,seq));
	}
	
	/*
		 * @author Ammar ALammar
		 * @date 2014-07-05
		 * @description Wait for a response from the callout & wait for the whole attachment body 
		 *              to be downloaded then assign as a texture to a game object
		 * 
		 * @param www The wwwForm being executed.
		 * 
		 * @modified by Ben Lyne on 8/1/2015 to include flipping and adding the thumbnail to the list.
		 */
	IEnumerator executeDownload(WWW www, int seq){
		yield return www;
		
		if (www.error == null){
			response = www.text;
		} else {
			response = www.error;
		}   
		
		// Obtain the binary byte array of the textures
		textureBytes = www.bytes;
		
		// Assign textures to the images coming in from Salesforce. // Set small and then it will 
		// automatically inherit the right size when it loads in the image immediately afterwards.
		Texture2D tex = new Texture2D(4, 4); 
		tex.LoadImage(www.bytes);
	
		// Flip the image horizontally (for images taken with the BubblePix app)
		Texture2D flip = new Texture2D(tex.width,tex.height);
		int xN = tex.width;
		int yN = tex.height;

		// Loop through each pixel and reverse.
		for(int i = 1; i < xN; i++) {
			for(int j = 1; j < yN; j++) {
				flip.SetPixel(xN-i-1, j, tex.GetPixel(i,j));
			}
		}

		// Execute the flip in the UI.
		flip.Apply();

		// Add the thumbnail to the list of others.
		Thumbnail temp = new Thumbnail (flip, flip, Country (), Date ());
		m_Thumbs.Add( temp );

		//Debug.Log ("END DOWNLOAD");
	}
	
	private void UpdateImages() {
/*
#if UNITY_ANDROID && !UNITY_EDITOR
		string[] tex = m_JUInterface.GetImagePaths();

		if ( tex.Length != m_Textures.Length ) {
			m_Textures = tex;
			m_Thumbs.Clear();
			foreach ( string file in m_Textures ) {
				if ( m_JUInterface.DecodeImage( file ) ) {
					m_Thumbs.Add( new Thumbnail( m_JUInterface.Image, file, m_JUInterface.Width, m_JUInterface.Height, m_JUInterface.Date, m_JUInterface.Country ) );
				}
			}
		}
#elif UNITY_IOS && !UNITY_EDITOR
		string[] tex = m_IUInterface.GetImages();

		if (tex.Length != m_Textures.Length ) {
			m_Textures = tex;
			m_Thumbs.Clear();
			foreach (string file in m_Textures ) {
				m_Thumbs.Add( new Thumbnail( iOSUnityInterface.GetPanoramaData( file ), file, m_IUInterface.GetWidth( file ), m_IUInterface.GetHeight( file ), m_IUInterface.GetDate( file ), m_IUInterface.GetCountry( file ) ) );
			}
		}
#endif
*/
	}

	// Static for now
	private string Country() {
		string[] nations = { "", "", "United States of America", "United States of America", "" };

		return nations[UnityEngine.Random.Range( 0, nations.Length - 1 )];
	}

	// Static for now
	private string Date() {
		string[] dates = { "", "2014:6:6 272762", "2015:7:5 82727", "2014:10:12 28373", "2015:1:3 293812", "2015:3:1 827636" };

		return dates[UnityEngine.Random.Range( 0, dates.Length - 1)];
	}
}