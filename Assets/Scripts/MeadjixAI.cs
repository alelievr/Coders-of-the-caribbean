using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeadjixAI : PlayerAI {

    Gradient g; 

    public void Start()
    {
        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        g = new Gradient();
        gck = new GradientColorKey[2];
        gck[0].color = Color.red;
        gck[0].time = 0.0F;
        gck[1].color = Color.blue;
        gck[1].time = 1.0F;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 0.0F;
        gak[1].time = 1.0F;
        g.SetKeys(gck, gak);
    }

    public override LeagueLevel GetLeagueLevel()
    {
        return LeagueLevel.BRONZE;
    }

	public override string PlayTurn(
		int myShipCount, int entityCount,
		List< ShipData > ships,
        List< RumBarrelData > barrels,
		List< MineData > mines,
		List< CannonBallData > cannonBalls)
	{
		string	ret = "";
        int     shipId = 0;

        var myShips = ships.Where(s => s.health > 0 && s.owner == 1);
        var enemyShips = ships.Where(s => s.health > 0 && s.owner == 0);
        
        foreach (var ship in myShips)
        {
            var firstEnemy = enemyShips.OrderBy(e => GameManager.HexDistance(ship.x, ship.y, e.x, e.y)).First();
        
            bool stoppedByEnemy = false;
            bool stoppedByAlly = false;
            bool stoppedByEmenyAtBow = false;
            bool bowIsBlocked = false;
            bool cannonBallIncommingSoon = false;
            ShipData blockingAlly = new ShipData(10, 10, 0, 0, 0, 0, 0);
            ShipData blockingEnemyAtBow = null;
            
            foreach (var e in enemyShips)
                if (GameManager.HexDistance(ship.x, ship.y, e.x, e.y) <= 2)
                    stoppedByEnemy = true;
            foreach (var e in myShips)
            {
                if (e != ship)
                    if (GameManager.HexDistance(ship.x, ship.y, e.x, e.y) <= 2)
                    {
                        stoppedByAlly = true;
                        blockingAlly = e;
                    }
            }
            var shipBow = ShipBow(ship);
            foreach (var e in enemyShips)
            {
                if (GameManager.HexDistance(shipBow.x, shipBow.y, e.x, e.y) <= 1)
                {
                    stoppedByEmenyAtBow = true;
                    blockingEnemyAtBow = e;
                }
            }
            foreach (var c in cannonBalls)
            {
                if (c.remainingTurns <= 2)
                {
                    if ((c.x == shipBow.x && c.y == shipBow.y) || (c.x == ship.x && c.y == ship.y))
                        cannonBallIncommingSoon = true;
                }
            }
            
            if (shipBow.x <= 1 || shipBow.x >= 22 || shipBow.y <= 1 || shipBow.y >= 20)
                bowIsBlocked = true;
            if (ship.speed == 0 && stoppedByEnemy)
            {
                if (firstEnemy.speed == 0)
                    ret += ("FIRE " + firstEnemy.x + " " + firstEnemy.y) + "\n";
                else
                    ret += ("FIRE " + ShipBow(firstEnemy).x + " " + ShipBow(firstEnemy).y) + "\n";
            }
            else if (stoppedByEmenyAtBow)
                ret += ("FIRE " + ShipBow(blockingEnemyAtBow).x + " " + ShipBow(blockingEnemyAtBow).y) + "\n";
            else if (ship.speed == 0 && stoppedByAlly || bowIsBlocked)
                ret += ("MOVE " + (22 - blockingAlly.x) + " " + (20 - blockingAlly.y)) + "\n";
            else
                if (barrels.Count != 0)
                {
                    var barrel = barrels.OrderBy(b => {
                        return GameManager.HexDistance(ship.x, ship.y, b.x, b.y);
                        }).First();
                    
                    ret += ("MOVE " + barrel.x + " " + barrel.y) + "\n";
                }
                else
                {
                    if (cannonBallIncommingSoon)
                        ret += ("MOVE " + shipBow.x + " " + shipBow.y) + "\n";
                    else
                        ret += ("FIRE " + firstEnemy.x + " " + firstEnemy.y) + "\n";
                }

            for (int x = 0; x < 23; x++)
                for (int y = 0; y < 21; y++)
                {
                    int dist = GameManager.HexDistance(x, y, ship.x, ship.y);
                    GameManager.SetCellColor(x, y, g.Evaluate((float)dist / 20f));
                }
            GameManager.SetShipDebugText(shipId++, "pos: " + ship.x + " / " + ship.y);
        }
        
        foreach (var mine in mines)
            GameManager.SetCellText(mine.x, mine.y, Random.Range(0, 10) + "");

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
