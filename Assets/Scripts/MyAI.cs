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
		int		shipId = 0;

        var myShips = ships.Where(s => s.health > 0 && s.owner == 1);
        var enemyShips = ships.Where(s => s.health > 0 && s.owner == 0);

        foreach (var ship in myShips)
        {
            var firstEnemy = enemyShips.OrderBy(e => GameManager.HexDistance(ship.x, ship.y, e.x, e.y)).First();

            bool stopped = false;

            foreach (var e in enemyShips)
                if (GameManager.HexDistance(ship.x, ship.y, e.x, e.y) <= 2)
                    stopped = true;

            if (ship.speed == 0 && stopped)
            {
                if (firstEnemy.speed == 0)
                    ret += ("FIRE " + firstEnemy.x + " " + firstEnemy.y) + "\n";
                else
                    ret += ("FIRE " + ShipBow(firstEnemy).x + " " + ShipBow(firstEnemy).y) + "\n";
            }
            else
                if (rumBarrels.Count != 0)
            {
                var barrel = rumBarrels.OrderBy(b =>
                {
                    return GameManager.HexDistance(ship.x, ship.y, b.x, b.y);
                }).First();

                ret += ("MOVE " + barrel.x + " " + barrel.y) + "\n";
            }
            else
                ret += ("FIRE " + firstEnemy.x + " " + firstEnemy.y) + "\n";
        }


        // GameManager.SetCellColor(Random.Range(0, 20), 0, Color.red);
        // GameManager.SetCellText(2, 2, "olol");

        // var g = GameManager.AddObjectAt(0, 0, GameObject.CreatePrimitive(PrimitiveType.Cube));
        // g.transform.localScale = Vector3.one * .7f;

        return ret;
	}

    public static GameReferee.Coord ShipStern(ShipData ship)
    {
        return new GameReferee.Coord(ship.x, ship.y).neighbor((ship.orientation + 3) % 6);
    }

    public static GameReferee.Coord ShipBow(ShipData ship)
    {
        return new GameReferee.Coord(ship.x, ship.y).neighbor(ship.orientation);
    }

}
