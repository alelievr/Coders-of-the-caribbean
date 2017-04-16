using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

	public int X { get; private set; }

	public int Y { get; private set; }

	public HexCoordinates (int x, int y) {
		X = x;
		Y = y;
	}

	public static HexCoordinates FromOffsetCoordinates (int x, int y) {
		return new HexCoordinates(x, y);
	}
	
	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString() + "\n" + Y.ToString();
	}

	public static HexCoordinates FromPosition (Vector3 position) {
		float x = position.x / (HexMetrics.innerRadius * 2f);
		float y = -x;
		float offset = position.y / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x -y);

		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x -y - iZ);

			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			}
			else if (dZ > dY) {
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

}
