using UnityEngine;
using System.Collections;

public class MeshAnimator : MonoBehaviour {

	[SerializeField]
	private MeshFilter m_Filter;

	[SerializeField]
	private MeshRenderer m_Renderer;

	private Mesh m_Mesh;
	private float m_BorderAlpha = 0.0f;
	private float m_TimeMultiUp = 1.5f;
	private float m_TimeMultiDown = 0.75f;
	private int m_Width = 29;
	private int m_Height = 14;
	private bool m_Paused = false;
	private bool m_IsPlane = true;

	public bool IsPlane {
		get { return m_IsPlane; }
	}

	// Use this for initialization
	void Start() {
		m_Mesh = ProceduralMesh.GeneratePlane( m_Height, m_Width );

		m_Filter.mesh = m_Mesh;
	}

	// Update is called once per frame
	void Update() {
		m_Renderer.material.SetFloat( "_BorderAlpha", m_BorderAlpha );
	}

	public void ToCylinder( Vector3 _targetPos ) {
		StopAllCoroutines();
		StartCoroutine( MoveAndScale( _targetPos, new Vector3( 4.0f, 4.0f, 4.0f ), Vector3.zero ) );
		StartCoroutine( PlaneToCircle() );
	}

	public void ToPlane() {
		StopAllCoroutines();
		StartCoroutine( FixRotation() );
	}

	private IEnumerator CircleToPlane() {
		Mesh target;
		Vector3[] vertices = new Vector3[m_Width * m_Height];
		Vector2[] uvs = new Vector2[m_Width * m_Height];
		target = ProceduralMesh.GeneratePlane( m_Height, m_Width );
		float time = 0.0f;

		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) != 0.0f ) {
			m_BorderAlpha = Mathf.Clamp( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ), 0.0f, 1.0f );
			if ( !m_Paused ) {
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time );
					uvs[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], time );
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv = uvs;
				time = ( Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) ) * m_TimeMultiUp;
			}
			yield return null;
		}
		m_BorderAlpha = 0.0f;
		m_Mesh.vertices = target.vertices;
		m_Mesh.triangles = target.triangles;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		m_Mesh.bounds = target.bounds;
		m_IsPlane = true;
	}

	private IEnumerator PlaneToCircle() {

		Mesh target;
		Vector3[] vertices = new Vector3[m_Width * m_Height];
		Vector2[] uvs = new Vector2[m_Width * m_Height];
		target = ProceduralMesh.GenerateCurvedCylinderSegment( m_Height, m_Width, 1.0f, 0.75f );
		m_Mesh.bounds = target.bounds;
		m_Mesh.triangles = target.triangles;
		float time = 0.0f;

		while ( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) != 0.0f ) {
			if ( !m_Paused ) {
				m_BorderAlpha = 1.0f - Mathf.Clamp( Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ), 0.0f, 1.0f );
				for ( int i = 0; i < m_Mesh.vertexCount; i++ ) {
					vertices[i] = Vector3.Lerp( m_Mesh.vertices[i], target.vertices[i], time );
					uvs[i] = Vector2.Lerp( m_Mesh.uv[i], target.uv[i], time );
				}
				m_Mesh.vertices = vertices;
				m_Mesh.uv = uvs;
				time = ( Time.deltaTime / Vector3.Distance( m_Mesh.vertices[0], target.vertices[0] ) ) * m_TimeMultiUp;
			}
			yield return null;
		}
		m_BorderAlpha = 1.0f;
		m_Mesh.vertices = target.vertices;
		m_Mesh.triangles = target.triangles;
		m_Mesh.uv = target.uv;
		m_Mesh.normals = target.normals;
		m_IsPlane = false;
	}

	private IEnumerator FixRotation() {
		Quaternion target = Quaternion.identity;
		Quaternion current = gameObject.transform.localRotation;
		float time = 0.0f;

		while ( Vector3.Distance(gameObject.transform.localRotation.eulerAngles, Vector3.zero) > 0.001f && time < 1.0f ) {
			gameObject.transform.localRotation = Quaternion.Lerp(current, target, time);
			time += Time.deltaTime * m_TimeMultiUp;
			yield return null;
		}
		gameObject.transform.localRotation = Quaternion.identity;
		StartCoroutine( CircleToPlane() );
		StartCoroutine( MoveAndScaleLocal( Vector3.zero, new Vector3( 2.0f, 2.0f, 1.0f ), Vector3.zero ) );
	}

	private IEnumerator MoveAndScale( Vector3 _targetPos, Vector3 _targetScale, Vector3 _targetRot ) {
		float distance = 1.0f;
		float scale = 1.0f;
		float time = 0.0f;
		float rotation = 1.0f;

		Vector3 startPos = gameObject.transform.position;
		Vector3 startScale = gameObject.transform.localScale;
		Vector3 startRot = gameObject.transform.localRotation.eulerAngles;
		yield return null;
		while ( distance > 0.01f || scale > 0.01f || rotation > 0.01f ) {
			if ( !m_Paused ) {

				time += Time.deltaTime * m_TimeMultiDown;
				distance = Vector3.Distance( gameObject.transform.position, _targetPos );
				scale = Vector3.Distance( gameObject.transform.localScale, _targetScale );
				rotation = Vector3.Distance( gameObject.transform.localRotation.eulerAngles, _targetRot );

				gameObject.transform.position = Vector3.Lerp( startPos, _targetPos, time );
				gameObject.transform.localScale = Vector3.Lerp( startScale, _targetScale, time );
				gameObject.transform.localRotation = Quaternion.Euler( Vector3.Lerp( startRot, _targetRot, time ) );
			}
			yield return null;
		}
		gameObject.transform.position = _targetPos;
		gameObject.transform.localScale = _targetScale;
		gameObject.transform.localRotation = Quaternion.Euler( _targetRot );

	}

	private IEnumerator MoveAndScaleLocal( Vector3 _targetPos, Vector3 _targetScale, Vector3 _targetRot ) {
		float distance = 1.0f;
		float scale = 1.0f;
		float time = 0.0f;
		float rotation = 1.0f;

		Vector3 startPos = gameObject.transform.localPosition;
		Vector3 startScale = gameObject.transform.localScale;
		Vector3 startRot = gameObject.transform.localRotation.eulerAngles;
		yield return null;
		while ( distance > 0.01f || scale > 0.01f || rotation > 0.01f ) {
			if ( !m_Paused ) {

				time += Time.deltaTime * m_TimeMultiDown;
				distance = Vector3.Distance( gameObject.transform.localPosition, _targetPos );
				scale = Vector3.Distance( gameObject.transform.localScale, _targetScale );
				rotation = Vector3.Distance( gameObject.transform.localRotation.eulerAngles, _targetRot );

				gameObject.transform.localPosition = Vector3.Lerp( startPos, _targetPos, time );
				gameObject.transform.localScale = Vector3.Lerp( startScale, _targetScale, time );
				gameObject.transform.localRotation = Quaternion.Euler( Vector3.Lerp( startRot, _targetRot, time ) );
			}
			yield return null;
		}
		gameObject.transform.localPosition = _targetPos;
		gameObject.transform.localScale = _targetScale;
		gameObject.transform.localRotation = Quaternion.Euler( _targetRot );

	}
}