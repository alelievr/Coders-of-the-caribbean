## Unity wrapper for the CodinGame challenge `Coders of the Caribbean`

### Where to put my AI ?
To implement your AI you will need to create a new c# class and inherit from PlayerAI class.  
You need to override the function `PlayTurn()` which is called every rounds with game status in parameter.  
For convenience reasons, tou cannot use Console.ReadLine nor Console.WriteLine to manage your AI input/output, the game status is in parameter of the PlayTurn function and you need tu return the output as a string with '\n' between ship actions.  
Once you have you script with your AI,
+ Go to unity create an empty object
+ Add your script to it
+ Inspect the GameManager object in the hierarchy and add your gameObject to the "Player AIs" list (or EnemyAIs if you want to fight against)
+ Play and use the arrows beside the player to swicth the AI until you find yours.

#### Sample codes:

using basic PlayTurn function:
```csharp
using UnityEngine;
using System.Linq;

public class MyAI : PlayerAI {

    public override LeagueLevel GetLeagueLevel()
    {
        return LeagueLevel.BRONZE;
    }
    
    public override string PlayTurn(int myShipCount, int entityCount, string[] inputs)
    {
        string  ret = "";

        for (int i = 0; i < myShipCount; i++)
            ret += "WAIT\n";

        return ret;
    }
}
```

OR PlayTurn with already parsed datas:
```csharp
using UnityEngine;
using System.Linq;

public class MyAI : PlayerAI {

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

        var myShips = ships.Where(s => s.health > 0 && s.owner == 1);

        foreach (var ship in myShips)
            ret += "MOVE " + Random.Range(0, 22) + " " + Random.Range(0, 20) + "\n";

        return ret;
    }
}

```

Code your AI in the file Scripts/MyAI.cs, in this file you will find a sample code to help you.  

### Additional debug and utility functions

name | description
--- | ---
GameManager.SetCellColor | Set the cell background color
GameManager.SetCellText | Set a text over the cell
GameManager.AddObjectAt | Add a GameObject over a cell
GameManager.SetShipDebugText | Set the text beside the health display (in the left bar)
GameManager.HexDistance | calcul the distances between two cells

:warning: both cell text and color are reset every rounds so you need to draw everything you need at each rounds.

### Screens
![screenshot1](https://image.noelshack.com/fichiers/2017/16/1492714606-screen-shot-2017-04-20-at-8-35-21-pm.png)

![screenshot2](https://image.noelshack.com/fichiers/2017/16/1492714572-screen-shot-2017-04-20-at-8-54-52-pm.png)

### TODO
+ on click on entity, get his informations
+ buttons to dynamically change object position / add new objects in the game and remake AI simulation
