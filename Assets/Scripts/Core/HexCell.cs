using UnityEngine;

public class HexCell {
    
    public Color            color;
    public Color            colorFilter;
    public HexCoordinates   coordinates;
    public Vector3          center;
    public string           text = null;

    public HexCell(int x, int y, Vector3 center)
    {
        coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
        color = new Color32(0x0A, 0x16, 0x5F, 0xFF);
        colorFilter = new Color(0, 0, 0, 0);
        this.center = center;
    }

}
