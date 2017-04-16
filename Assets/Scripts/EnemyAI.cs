using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : PlayerAI {

	public override string PlayTurn(
		int myShipCount, int enemyShipCount,
		List< ShipData > ships,
        List< RumBarrelData > rumBarrels,
		List< MineData > mines,
		List< CannonBallData > cannonBalls)
	{
		return "MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20);
	}
}
