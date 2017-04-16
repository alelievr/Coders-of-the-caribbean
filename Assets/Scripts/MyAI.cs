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
			ret += "MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20) + ship;
			if (ship != myShips.Last())
				ret += ";";
		}

		GameManager.SetCellText(3, 3, "olol");

		return ret;
	}

}
