using UnityEngine;
using System.Collections.Generic;

public class ProceduralMesh : MonoBehaviour {

	private static int numVertices;
	private static int numIndices;

	private static Vector3[] vertices;
	private static Vector2[] texCoords;
	private static Vector3[] normals;
	private static int[] indices;

	private struct Quad {
		public List<Vector3> Vertices;
		public List<Vector2> UVs;
		public List<Vector3> Normals;
		public int[] Indices;
	};

	public static Mesh GenerateCurvedCylinder( int _height, int _sides ) {
		//float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)(_height - 2);
		float sideInc = 1.0f / (float)(_sides - 1);
		float degInc = 360.0f / ( (float)_sides - 1 );
		float bulgeInc = 90.0f / (float)_height - 1;
		float bulge = 15 * bulgeInc;
		float deg = -90.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge );

				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc);
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = -90.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height - 1; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();
		ret.RecalculateNormals();


		//float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Verticies: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateCylinder( int _height, int _sides ) {
		//float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)_height;
		float sideInc = 1.0f / (float)_sides;
		float height = -heightInc;
		float degInc = 360.0f / ( (float)_sides - 1 );
		float deg = 0.0f;

		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			height += heightInc;
			for ( int j = 0; j < _sides; ++j ) {
				unitPos.y = i;
				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = unitPos;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				deg += degInc;
			}					
			deg = 0.0f;
		}
		
		int offset = 0;
		
		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;

		ret.RecalculateBounds();
		ret.RecalculateNormals();

		//float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateCup( int _height, int _sides ) {
		//float startTime = Time.realtimeSinceStartup;

		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)_height;
		float sideInc = 1.0f / ( (float)_sides - 1 );
		float height = -heightInc;
		float degInc = 360.0f / ( (float)_sides - 1 );
		float bulgeInc = 45.0f / (float)_height - 1;
		float bulge = 0.0f;
		float deg = 0.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			height += heightInc;
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );
				rad = Mathf.Sin( Mathf.Deg2Rad * bulge );

				unitPos.x = Mathf.Cos( Mathf.Deg2Rad * deg );
				unitPos.z = Mathf.Sin( Mathf.Deg2Rad * deg );

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = 0.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();

		//float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GeneratePlane( int _height, int _width ) {
		//float startTime = Time.realtimeSinceStartup;
		
		numVertices = _width * _height;
		numIndices = 6 * ( _width - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / ( (float)_height - 1);
		float sideInc = 1.0f / ( (float)_width - 1);

		float xInc = 1.0f / _height;
		float yInc = 1.0f / _width;

		float halfHeight = (_height - 1) / 2;
		float halfWidth = (_width - 1) /2;

		for ( int i = 0; i < _height; ++i ) {
			for ( int j = 0; j < _width; ++j ) {
				vertices[i * _width + j] = new Vector3( -(j - halfWidth) * xInc, (i - halfHeight) * yInc, 0.0f );
				texCoords[i * _width + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _width + j] = Vector3.back;
			}
		}

		int offset = 0;

		for ( int i = 1; i < _height; ++i ) {
			for ( int j = 0; j < _width - 1; ++j ) {
				int a = i * _width + j;
				int b = ( i - 1 ) * _width + j;
				int c = ( i - 1 ) * _width + j + 1;
				int d = i * _width + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;

		ret.RecalculateBounds();
		ret.RecalculateNormals();

		//float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Plane mesh generated in " + diff.ToString() + " seconds. Vertex count: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

	public static Mesh GenerateCurvedCylinderSegment( int _height, int _sides, float _deg, float _curve ) {
		//float startTime = Time.realtimeSinceStartup;

		float curveOffset = _deg > 2.0f ? 2.0f : _deg;
		numVertices = _sides * _height;
		numIndices = 6 * ( _sides - 1 ) * _height;

		vertices = new Vector3[numVertices];
		texCoords = new Vector2[numVertices];
		normals = new Vector3[numVertices];
		indices = new int[numIndices];

		float heightInc = 1.0f / (float)( _height - 2 );
		float sideInc = 1.0f / (float)( _sides - 1 );
		float degInc = (360.0f / _deg) / ( (float)_sides - 1 );
		float bulgeInc = 90.0f / (float)_height; //4.7
		float bulge = (_height * 0.5f) * bulgeInc;
		float deg = -90.0f;
		float rad = 0.0f;

		Vector3 centrePos;
		Vector3 unitPos;

		for ( int i = 0; i < _height; ++i ) {
			bulge += bulgeInc;
			for ( int j = 0; j < _sides; ++j ) {
				centrePos = unitPos = Vector3.zero;

				centrePos.y = -Mathf.Cos( Mathf.Deg2Rad * bulge );

				rad = Mathf.Sin( Mathf.Deg2Rad * bulge ); //Defines vertical bulge
				rad = Mathf.Clamp(rad, _curve, 1.2f);

				unitPos.x += Mathf.Cos( Mathf.Deg2Rad * deg) * curveOffset;
				unitPos.z += Mathf.Sin( Mathf.Deg2Rad * deg) / curveOffset;

				vertices[i * _sides + j] = centrePos + unitPos * rad;
				texCoords[i * _sides + j] = new Vector2( j * sideInc, i * heightInc );
				normals[i * _sides + j] = Vector3.Normalize( centrePos + unitPos * rad );
				deg += degInc;
			}
			deg = -90.0f;
		}

		int offset = 0;

		for ( int i = 1; i < _height - 1; ++i ) {
			for ( int j = 0; j < _sides - 1; ++j ) {
				int a = i * _sides + j;
				int b = ( i - 1 ) * _sides + j;
				int c = ( i - 1 ) * _sides + j + 1;
				int d = i * _sides + j + 1;

				indices[offset++] = c;
				indices[offset++] = b;
				indices[offset++] = a;

				indices[offset++] = a;
				indices[offset++] = d;
				indices[offset++] = c;
			}
		}

		Mesh ret = new Mesh();

		ret.vertices = vertices;
		ret.uv = texCoords;
		ret.triangles = indices;
		ret.normals = normals;
		ret.RecalculateBounds();
		ret.RecalculateNormals();


		//float diff = Time.realtimeSinceStartup - startTime;
		//Debug.Log( "Cylinder mesh generated in " + diff.ToString() + " seconds. Verticies: " + numVertices + ". Indicies: " + numIndices + "." );
		return ret;
	}

}