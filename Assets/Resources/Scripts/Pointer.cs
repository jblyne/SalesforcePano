using UnityEngine;
using System.Collections;

/* Provides methods for altering the pointer colors,
 * along with notification text attached to it. 
 */

public class Pointer : MonoBehaviour {
	
	[SerializeField]
	private MeshRenderer m_Pointer;

	[SerializeField]
	private MeshRenderer m_Background;

	[SerializeField]
	private TextMesh m_Text;

	[SerializeField]
	private Camera m_Cam;

	[SerializeField]
	private float m_ObjScale = 0.5f;

	private Color[] m_Colors = { Color.white, Color.green, Color.yellow };
	private Vector3 m_InitScale;
	private AppController m_AppController;

	void Start() {
		m_InitScale = m_Text.gameObject.transform.localScale;
		m_AppController = GameObject.Find("SceneObjects").GetComponent<AppController>() as AppController;
	}

	void Update() {
		ScaleToCamera();
		if ( !m_AppController.VRMode || m_AppController.State == AppController.AppState.Viewer ) {
			m_Text.gameObject.SetActive( false );
			m_Background.gameObject.SetActive( false );
		}
	}

	public void SetText( string _arg ) {
		m_Text.gameObject.SetActive( true );
		m_Text.text = _arg;
	}

	public void UnsetText() {
		m_Text.gameObject.SetActive( false );
	}

	public void SetColor( int _arg ) {
		m_Pointer.material.color = m_Colors[_arg];
	}

	private void ScaleToCamera() {
		float dist = Vector3.Distance( m_Cam.gameObject.transform.position, m_Text.gameObject.transform.position );
		m_Text.gameObject.transform.localScale = m_InitScale * dist * m_ObjScale;
	}

}
