using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MyAI : PlayerAI {

	public override string PlayTurn(
		int myShipCount, int enemyShipCount,
		List< ShipData > ships,
        List< RumBarrelData > rumBarrels,
		List< MineData > mines,
		List< CannonBallData > cannonBalls)
	{
		string	ret = "";

		var myShips = ships.Where(s => s.owner == 0 && s.health > 0);

		foreach (var ship in myShips)
		{
			var rumBarrel = rumBarrels.OrderBy(r => (r.x - ship.x) + (r.y - ship.y)).First();
			if (rumBarrel != null)
				ret += "MOVE " + rumBarrel.x + " " + rumBarrel.y;
			else
				ret += "MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20) + ship;
			if (ship != myShips.Last())
				ret += "\n";
		}

		GameManager.SetCellColor(0, 0, Color.red);
		GameManager.SetCellText(2, 2, "olol");

		// var g = GameManager.AddObjectAt(0, 0, GameObject.CreatePrimitive(PrimitiveType.Cube));
		// g.transform.localScale = Vector3.one * .7f;

		return ret;
	}

}
