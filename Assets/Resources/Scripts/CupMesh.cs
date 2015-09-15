using UnityEngine;
using System.Collections;

public class CupMesh : MonoBehaviour {

	[SerializeField]
	private MeshFilter m_Mesh = null;

	[SerializeField]
	private int m_Height = 5;

	[SerializeField]
	private int m_Sides = 20;

	void Start() {
		m_Mesh.mesh = ProceduralMesh.GenerateCup( m_Height, m_Sides );
	}
}
