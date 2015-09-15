using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Handles the meshes related to the viewing of panoramas,
// along with the methods that move and enable parts of the viewer.

public class PanoramaViewer : MonoBehaviour {

#region Variable Declarations

	[SerializeField]
	private GameObject m_PanoButtons;

	private AppController m_Controller;
	private ThumbTile m_ActiveThumb;

#endregion

#region Accessor/Mutator Methods

	public ThumbTile ActiveThumb {
		get { return m_ActiveThumb; }
		set { m_ActiveThumb = value; }
	}

#endregion

#region Monobehaviour Overrides

	void Awake() {
		m_Controller = GameObject.Find( "SceneObjects" ).GetComponent<AppController>() as AppController;
	}

	void Start() 
	{
		//m_ActiveMesh = m_Cylinder.GetComponent( "MeshRenderer" ) as MeshRenderer;
	}

	void Update() {
		if ( m_Controller.State == AppController.AppState.Viewer ) {
			if ( m_Controller.VRMode ) {
				m_PanoButtons.SetActive( false );
			}
			else {
				m_PanoButtons.SetActive( true );
			}

			if ( m_Controller.TC.SwipeDirection[0] == TouchController.Swipe.Positive  ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, Mathf.Min( m_Controller.TC.SwipeSpeed.x * 0.04f, 4.0f ), 0.0f ) );
			}
			else if ( m_Controller.TC.SwipeDirection[0] == TouchController.Swipe.Negative ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, -Mathf.Min( m_Controller.TC.SwipeSpeed.x * 0.04f, 4.0f ), 0.0f ) );
			}
			else if ( Input.GetKey( KeyCode.RightArrow ) ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, 1.0f, 0.0f ) );
			}
			else if ( Input.GetKey( KeyCode.LeftArrow ) ) {
				m_ActiveThumb.MeshTransform.Rotate( new Vector3( 0.0f, -1.0f, 0.0f ) );
			}

		}
		else {
			m_PanoButtons.SetActive( false );
		}
	}

#endregion

}