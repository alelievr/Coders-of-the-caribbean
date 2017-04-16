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
		string	ret = "WAIT";

		foreach (var ship in ships.Where(s => s.owner == 0))
			ret += ";MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20);

		return ret;
	}

}
