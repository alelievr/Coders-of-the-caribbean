using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : PlayerAI {

	//the *Data storage classes are stored in Core/EntityDatas.cs file if you want them for CG.

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
			if (rumBarrels.Count != 0)
			{
				var rumBarrel = rumBarrels.OrderBy(r => (r.x - ship.x) + (r.y - ship.y)).First();
				if (rumBarrel != null)
					ret += "MOVE " + rumBarrel.x + " " + rumBarrel.y;
			}
			else
				ret += "MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20) + ship;
			if (ship != myShips.Last())
				ret += "\n";
		}

		return ret;
	}
}
