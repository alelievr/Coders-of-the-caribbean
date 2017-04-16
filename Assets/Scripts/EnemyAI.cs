using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : PlayerAI {

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

		return ret;
	}
}
