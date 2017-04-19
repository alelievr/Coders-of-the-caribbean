using UnityEngine;
using UnityEditor;

public class HexGrid : MonoBehaviour {

    int width;
    int height;

    public HexCell		cellPrefab;
	public GameObject	textCell;

    HexCell[,]		cells;
	TextMesh[,]		texts;
    HexMesh			hexMesh;

	public void BuildHexMap (int width, int height) {
		this.width = width;
		this.height = height;

		cells = new HexCell[width, height];
		texts = new TextMesh[width, height];

        hexMesh = GetComponentInChildren<HexMesh>();

        for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, height - z - 1);
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

	public void SetCellColor(int x, int y, Color c)
	{
		y = height - y - 1;
		cells[x, y].color = c;
	}

	public void SetCellColorFilter(int x, int y, Color c)
	{
		y = height - y - 1;
		cells[x, y].colorFilter = c;
	}

	public void UpdateMap()
	{
		hexMesh.Triangulate(cells);
	}

	public void	SetCellText(int x, int y, string text, Color c)
	{
		y = height - y - 1;
		if (texts[x, y] == null)
		{
			texts[x, y] = Instantiate(textCell, hexMesh.transform).GetComponent< TextMesh >();
			texts[x, y].transform.localPosition = GameManager.GridToWorldPosition(x, y);
		}
		texts[x, y].text = text;
		texts[x, y].color = c;
	}

	public void ClearTexts()
	{
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				if (texts[x, y] != null)
					texts[x, y].text = "";
	}
}
