using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	Mesh			hexMesh;
	List< Vector3 >	vertices;
	List< int >		triangles;
	List< Color >	colors;
	List< Vector2 >	uvs;
	
	MeshCollider	meshCollider;

	int				width, height;

	void Awake () {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();

		hexMesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
		triangles = new List<int>();
		colors = new List< Color >();
		uvs = new List< Vector2 >();
	}

	void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		uvs.Add(new Vector2(0.1f, 0.1f));
		uvs.Add(Vector2.one * HexMetrics.innerRadius);
		uvs.Add(Vector2.one * HexMetrics.innerRadius);
	}

	public void Triangulate (HexCell[,] cells) {
		hexMesh.Clear();
		vertices.Clear();
		uvs.Clear();
		triangles.Clear();
		colors.Clear();
		foreach (var cell in cells)
			Triangulate(cell);
		hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.colors = colors.ToArray();
		hexMesh.uv = uvs.ToArray();
		hexMesh.RecalculateNormals();

		meshCollider.sharedMesh = hexMesh;
	}
	
	void Triangulate (HexCell cell) {
		Vector3 center = cell.center;
		for (int i = 0; i < 6; i++) {
			AddTriangle(
				center,
				center + HexMetrics.corners[i],
				center + HexMetrics.corners[i + 1]
			);
			AddTriangleColor(cell.color);
		}
	}

	void AddTriangleColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}
}