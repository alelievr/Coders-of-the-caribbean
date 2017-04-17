using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSnapshot {

	public GameReferee					referee;
	public List< GameReferee.Player >	oldPlayers;

	public GameSnapshot(GameReferee gr, List< GameReferee.Player > op)
	{
		//TODO: clone these classes
		referee = gr;
		oldPlayers = op;
	}

	public void Restore(out GameReferee gr, out List< GameReferee.Player > op)
	{
		gr = referee;
		op = oldPlayers;
	}

}
