using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextPanel : MonoBehaviour {

	[SerializeField]
	private Text m_TextMesh;

	[SerializeField]
	private MeshRenderer m_Panel;

	public Text Text {
		get { return m_TextMesh; }
	}

	public MeshRenderer Mesh {
		get { return m_Panel; }
	}

	void Update() {
		Vector3 pos = gameObject.transform.position;

		float dist = Mathf.Clamp( ( Mathf.Abs( pos.x ) - 2.0f ) * 0.1f, 0.0f, 1.0f );

		Vector4 color = m_TextMesh.color;
		Vector4 panelColor = m_Panel.renderer.material.color;

		color.w = 1.0f - dist;
		panelColor.w = 1.0f - dist;

		m_TextMesh.color = color;
		m_Panel.renderer.material.color = panelColor;
	}

	public void SetPos( Vector3 _pos ) {
		this.gameObject.transform.localPosition = _pos;
	}

	public void SetText( string _text ) {
		m_TextMesh.text = _text;
	}
}
