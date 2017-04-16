using UnityEngine;
using UnityEditor;

public class HexGrid : MonoBehaviour {

    int width;
    int height;

    public HexCell	cellPrefab;

    HexCell[,]		cells;
    HexMesh			hexMesh;

	public void BuildHexMap (int width, int height) {
		this.width = width;
		this.height = height;

		cells = new HexCell[width, height];

        hexMesh = GetComponentInChildren<HexMesh>();

        for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z);
			}
		}
		hexMesh.Triangulate(cells);
	}

	void CreateCell (int x, int z) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = z * (HexMetrics.outerRadius * 1.5f);
		position.z = 0f;
		
		cells[x, z] = new HexCell(x, z, position);
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

		HexCell cell = cells[coordinates.X + coordinates.Y / 2, coordinates.Y];
		cell.color = Color.red;
		hexMesh.Triangulate(cells);
	}

	public void SetCellColor(int x, int y, Color c)
	{
		cells[x, y].color = c;
		hexMesh.Triangulate(cells);
	}

	public void	SetCellText(int x, int y, string text)
	{
		cells[x, y].text = text;
	}

	void OnDrawGizmos()
	{
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				if (cells[x, y].text != null)
					Handles.Label(cells[x, y].center, cells[x, y].text);
	}

}
