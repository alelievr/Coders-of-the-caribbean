using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    public int width = 22;
    public int height = 20;

    public HexCell	cellPrefab;

    HexCell[]		cells;
    HexMesh			hexMesh;

	void Awake () {
		cells = new HexCell[height * width];

        hexMesh = GetComponentInChildren<HexMesh>();

        for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void Start () {
		hexMesh.Triangulate(cells);
	}
	
	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = z * (HexMetrics.outerRadius * 1.5f);
		position.z = 0f;
		
		cells[i] = new HexCell(x, z, position);
	}

	void Update () {
		if (Input.GetMouseButton(0)) {
			HandleInput();
		}
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			TouchCell(hit.point);
		}
	}
	
	void TouchCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		Debug.Log("touched at " + coordinates.ToString());

		int index = coordinates.X + coordinates.Y * width + coordinates.Y / 2;
		HexCell cell = cells[index];
		cell.color = Color.red;
		hexMesh.Triangulate(cells);
	}

}
