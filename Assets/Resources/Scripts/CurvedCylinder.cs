using UnityEngine;
using System.Collections;

//Will produce a cylinder with no top or bottom, and curved sides (only shows during runtime)

public class CurvedCylinder : MonoBehaviour {

	[SerializeField]
	private MeshFilter m_Mesh = null;

	[SerializeField]
	private int m_Height = 20;

	[SerializeField]
	private int m_Sides = 40;

	void Start() {
		m_Mesh.mesh = ProceduralMesh.GenerateCurvedCylinder(m_Height, m_Sides);
	}
}
