using UnityEngine;

public class HexCell {
    
    public Color            color;
    public HexCoordinates   coordinates;
    public Vector3          center;

    public HexCell(int x, int y, Vector3 center)
    {
        coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
        color = Color.white;
        this.center = center;
    }

}
