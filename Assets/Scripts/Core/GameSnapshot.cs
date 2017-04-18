using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameSnapshot {

	public GameReferee					referee;
	public List< GameReferee.Player >	oldPlayers;
	
	MemoryStream stream = new MemoryStream(1024);

	public GameSnapshot(GameReferee gr, List< GameReferee.Player > op)
	{
		BinaryFormatter	bf = new BinaryFormatter();
		
		stream.Seek(0, SeekOrigin.Begin);
		bf.Serialize(stream, gr);
		stream.Seek(0, SeekOrigin.Begin);
		referee = bf.Deserialize(stream) as GameReferee;
		stream.Seek(0, SeekOrigin.Begin);
		bf.Serialize(stream, op);
		stream.Seek(0, SeekOrigin.Begin);
		oldPlayers = bf.Deserialize(stream) as List< GameReferee.Player >;
	}

	public void Restore(out GameReferee gr, out List< GameReferee.Player > op)
	{
		BinaryFormatter	bf = new BinaryFormatter();
		
		stream.Seek(0, SeekOrigin.Begin);
		bf.Serialize(stream, referee);
		stream.Seek(0, SeekOrigin.Begin);
		gr = bf.Deserialize(stream) as GameReferee;
		stream.Seek(0, SeekOrigin.Begin);
		bf.Serialize(stream, oldPlayers);
		stream.Seek(0, SeekOrigin.Begin);
		op = bf.Deserialize(stream) as List< GameReferee.Player >;
	}

	public void FastCheck(out GameReferee gr, out List< GameReferee.Player > op)
	{
		gr = referee;
		op = oldPlayers;
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
