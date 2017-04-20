using System.Collections.Generic;
using UnityEngine;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class PlayerAI : MonoBehaviour
{
    [HideInInspector]
    public bool playTurnInputOverride = false;

	public virtual string PlayTurn(
        int myShipCount, int entityCount,
        List< ShipData > ships,
        List< RumBarrelData > rumBarrels,
        List< MineData > mines,
        List< CannonBallData > cannonBalls)
    {
        string  ret = "";

        for (int i = 0; i < myShipCount; i++)
            ret += "MOVE 11 10";

        return ret;
    }

    public virtual string PlayTurn(int myShipCount, int entityCount, string[] inputs)
    {
        string  ret = "";

        for (int i = 0; i < myShipCount; i++)
            ret += "MOVE 11 10";
        
        playTurnInputOverride = true;

        return ret;
    }

}