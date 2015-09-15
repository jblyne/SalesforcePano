using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

/* Prefab script that defines each thumbnail and
 * it's individual behaviour within the browser.
 */

public class ThumbTile : MonoBehaviour {

#region Variable Declarations

	[SerializeField]
	private MeshRenderer m_Mesh;

	[SerializeField]
	private MeshRenderer m_InfoPanel;

	[SerializeField]
	private MeshRenderer m_LargeInfoPanel;

	[SerializeField]
	private MeshAnimator m_Animator;

	[SerializeField]
	private Text m_Text;

	[SerializeField]
	private Text m_LargeText;

	private AppController m_AppController;
	private FileHandler.Thumbnail m_Thumb;
	private bool m_Selected = false;
	private bool m_Focus = false;
	private bool m_Viewing = false;
	private int m_CarouselPos;
	private Pointer m_Pointer = null;
	private string m_PointerString = "View Panorama";

	#endregion

	#region Properties
	public FileHandler.Thumbnail Image {
		get { return m_Thumb; }
	}

	public MeshAnimator Animator {
		get { return m_Animator; }
	}

	public Transform MeshTransform {
		get { return m_Mesh.transform; }
	}

	public int CarouselPos {
		get { return m_CarouselPos; }
		set { m_CarouselPos = value; }
	}

	public string ImageString {
		get { return m_Thumb.ImageLoc; }
	}

	public Pointer Pointer {
		set { m_Pointer = value; }
	}

	#endregion

	#region MonoBehaviour Overrides

	void Awake() {
		m_AppController = GameObject.Find( "SceneObjects" ).GetComponent<AppController>() as AppController;
	}

	void Update() {
		if ( m_AppController.TC.Swiping ) {
			m_Selected = false;
		}

		if ( m_AppController.VRMode ) {
			m_InfoPanel.gameObject.SetActive( true );
			m_LargeInfoPanel.gameObject.SetActive( false );
		}
		else {
			m_InfoPanel.gameObject.SetActive( false );
			m_LargeInfoPanel.gameObject.SetActive( true );
		}

		Vector3 pos = m_Mesh.gameObject.transform.position;

		float xdist = ( Mathf.Abs( pos.x ) - 2.0f ) * 0.1f;
		float ydist = Mathf.Abs( pos.y - 0.93f ) - 1.0f;

		Vector4 color = m_Mesh.material.color;
		Vector4 infoColor = m_InfoPanel.material.color;
		Vector4 textColor = m_Text.color;

		xdist = Mathf.Clamp( xdist, 0.0f, 1.0f );
		ydist = Mathf.Clamp( ydist, 0.0f, 1.0f );

		float fade = Mathf.Max( xdist, ydist );

		if ( m_Selected ) {
			color.w = 1.0f;
			textColor.w = 1.0f;
			infoColor.w = 0.6f;
		}
		else {
			color.w = 1.0f - fade;
			textColor.w = 0.0f;
			infoColor.w = 0.0f;
		}

		m_Mesh.material.color = color;
		m_InfoPanel.material.color = infoColor;
		m_LargeInfoPanel.material.color = infoColor;
		m_Text.color = textColor;
		m_LargeText.color = textColor;
	
		if ( pos.x < 2.5f && pos.x > -2.5f && pos.y < 2.0f && pos.y > -0.3f ) {
			m_PointerString = "View Panorama";
			m_Focus = true;
		}
		else {
			m_PointerString = "Scroll To";
			m_Focus = false;
		}
	}

	#endregion

	#region Mutator Methods

	public void SetThumb( FileHandler.Thumbnail _thumb ) {
		m_Thumb = _thumb;
		m_Mesh.material.mainTexture = m_Thumb.Thumb;
		m_Text.text = InfoString( m_Thumb );
		m_LargeText.text = InfoString( m_Thumb );
	}

	public void SetPos( Vector3 _pos ) {
		this.gameObject.transform.localPosition = _pos;
	}

	#endregion

	#region User-Interface Methods

	public void Hover() {
		if ( m_AppController.VRMode && !m_Viewing ) {
			m_Selected = true;
			if ( m_Pointer != null ) {
				m_Pointer.SetColor( 1 );
				m_Pointer.SetText( m_PointerString );
			}
		}
	}

	public void NoHover() {
		if ( m_AppController.VRMode && !m_Viewing ) {
			m_Selected = false;
			if ( m_Pointer != null ) {
				m_Pointer.SetColor( 0 );
				m_Pointer.UnsetText();
			}
		}
	}

	public void DeClicked() {
		m_Selected = false;
	}

	public void Clicked() {
		if ( !m_Viewing ) {
			if ( m_AppController.VRMode ) {
				if ( m_Focus && !m_AppController.TB.Sweeping ) {
					NoHover();
					m_AppController.BrowserToPano( this );
					m_Viewing = true;
				}
				else {
					m_AppController.TB.SweepToCentre( this.gameObject );
					m_Selected = true;
				}
			}
			else {
				if ( m_Selected && !m_AppController.TB.Sweeping ) {
					m_Selected = false;
					NoHover();
					m_AppController.BrowserToPano( this );
					m_Viewing = true;
				}
				else {
					m_AppController.TB.DeselectThumbs();
					m_Selected = true;
				}
			}
		}
	}

	public void SetComponentsActive( bool _arg ) {
		m_LargeInfoPanel.gameObject.SetActive( _arg );
		m_InfoPanel.gameObject.SetActive( _arg );
	}

	public void StopViewing() {
		m_Viewing = false;
	}


	#endregion

	private string InfoString( FileHandler.Thumbnail _thumb ) {
		return "<b>Name: </b>" + Path.GetFileNameWithoutExtension( _thumb.ImageLoc ) +
				" \n <b>Resolution: </b>" + _thumb.Width + "x" + _thumb.Height +
				" \n <b>Location: </b>" + _thumb.Country +
				" \n <b>Date Taken: </b>" + _thumb.DateString;
	}

	private void SetPointer() {
		if ( m_AppController.VRMode && m_Pointer != null )
			m_Pointer = GameObject.Find( "GazePointer" ).GetComponent<Pointer>() as Pointer;
	}
}