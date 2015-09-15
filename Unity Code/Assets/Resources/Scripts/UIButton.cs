using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {
	
	[SerializeField]
	private bool m_IsToggle;

	[SerializeField]
	private string m_PointerString = null;

	private Color[] m_Colors = { new Color( 0.34f, 0.48f, 0.72f, 0.47f ),	//Color
								 new Color( 0.34f, 0.48f, 0.72f, 0.74f ),	//Hover Color
								 new Color( 0.32f, 0.7f, 0.4f, 0.47f ),		//Active Color
								 new Color( 0.32f, 0.7f, 0.4f, 0.74f ) };	//Active Hover Color

	private bool m_IsActive;
	private bool m_IsHeld;
	private Pointer m_Pointer = null;
	private MeshRenderer m_Mesh;
	float m_ClickDuration = 0.0f;

	
	delegate void OnCursorHoldDelegate();
	OnCursorHoldDelegate m_Delegate;

	public Material Material {
		get { return m_Mesh.material; }
		set { m_Mesh.material = value; }
	}

	public float ClickTime {
		get { return m_ClickDuration; }
	}
	
	void Awake() {
		m_Mesh = GetComponent<MeshRenderer>() as MeshRenderer;
		m_Pointer = GameObject.Find( "GazePointer" ).GetComponent<Pointer>() as Pointer;

		if ( m_Pointer == null )
			Debug.Log( "Could not find the pointer" );

		if ( m_Mesh == null )
			Debug.Log( "Could not find the mesh" );
	}
	
	public void OnHover() {
		if ( m_IsActive )
			m_Mesh.material.color = m_Colors[3];
		else
			m_Mesh.material.color = m_Colors[1];

		if ( m_PointerString != null )
			m_Pointer.SetText( m_PointerString );
	}

	public void OnNoHover() {
		if ( m_IsActive )
			m_Mesh.material.color = m_Colors[2];
		else
			m_Mesh.material.color = m_Colors[0];

		if ( m_PointerString != null )
			m_Pointer.UnsetText();
	}

	public void OnClick() {
		if ( m_IsToggle ) {
			m_IsActive = !m_IsActive;

			if ( m_IsActive )
				m_Mesh.material.color = m_Colors[3];
			else
				m_Mesh.material.color = m_Colors[1];

		}
	}

	public void PointerDown() {
		m_IsHeld = true;
		StartCoroutine("TimeClick");
	}

	public void PointerUp() {
		m_IsHeld = false;
		StopCoroutine("TimeClick");
	}

	public void SetClicked( bool _arg ) {
		if ( _arg ) {
			m_Mesh.material.color = m_Colors[2];
			m_IsActive = true;
		}
		else {
			m_Mesh.material.color = m_Colors[0];
			m_IsActive = false;
		}
	}

	public void SetAlpha( float _value ) {
		Color color = m_Mesh.material.color;
		color.a = _value;
		m_Mesh.material.color = color;
	}

	public IEnumerator AlphaFade( float _target, float _rate ) {
		Color color = m_Mesh.material.color;
		bool subtract = _target < color.a ? true : false;

		while ( Mathf.Abs( _target - color.a ) > 0.01f ) {
			color = m_Mesh.material.color;

			if ( subtract )
				color.a -= _rate;
			else
				color.a += _rate;

			m_Mesh.material.color = color;

			yield return null;
		}
		color.a = _target;
		m_Mesh.material.color = color;

		if ( _target == 0.0f ) {
			gameObject.SetActive( false );
		}
	}

	public IEnumerator TimeClick() {
		m_ClickDuration = 0.0f;

		while ( m_IsHeld ) {
			m_ClickDuration += Time.deltaTime;
			yield return null;
		}
	}
}
