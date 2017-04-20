using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameSnapshot {

	public GameReferee					referee;
	public List< GameReferee.Player >	oldPlayers;
	public List< GameReferee.Cannonball > cannonBalls;
	
	MemoryStream stream = new MemoryStream(1024);
	BinaryFormatter	bf = new BinaryFormatter();

	T Clone< T >(T obj)
	{
		stream.Seek(0, SeekOrigin.Begin);
		bf.Serialize(stream, obj);
		stream.Seek(0, SeekOrigin.Begin);
		T ret = (T)bf.Deserialize(stream);
		return ret;
	}

	public GameSnapshot(GameReferee gr, List< GameReferee.Player > ops, List< GameReferee.Cannonball > cbs)
	{
		referee = Clone< GameReferee >(gr);
		oldPlayers = Clone< List< GameReferee.Player > >(ops);
		cannonBalls = Clone< List< GameReferee.Cannonball > >(cbs);
	}

	public void Restore(out GameReferee gr, out List< GameReferee.Player > ops, out List< GameReferee.Cannonball > cbs)
	{
		gr = Clone< GameReferee >(referee);
		ops = Clone< List< GameReferee.Player > >(oldPlayers);
		cbs = Clone< List< GameReferee.Cannonball > >(cannonBalls);
	}

	public void FastCheck(out GameReferee gr, out List< GameReferee.Player > op, out List< GameReferee.Cannonball > cbs)
	{
		gr = referee;
		op = oldPlayers;
		cbs = cannonBalls;
	}

	public static T CloneObject< T >(T obj)
	{
		MemoryStream stream = new MemoryStream(1024);
		BinaryFormatter	bf = new BinaryFormatter();

		bf.Serialize(stream, obj);
		stream.Seek(0, SeekOrigin.Begin);
		return (T)bf.Deserialize(stream);
	}
}
