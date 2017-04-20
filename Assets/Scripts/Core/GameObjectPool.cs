using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameObjectPool {

	GameObject[]	objects;
	bool[]			updated;

	public void SetUpdated(bool b)
	{
		for (int i = 0; i < updated.Length; i++)
			updated[i] = b;
	}

	public GameObjectPool(int n)
	{
		objects = new GameObject[n];
		updated = new bool[n];
	}

	public void RemoveUnused(Action< GameObject > onDestroy)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			if (updated[i] == false && objects[i] != null)
			{
				onDestroy(objects[i]);
				GameObject.Destroy(objects[i]);
			}
		}
	}

	public void Update(int i)
	{
		updated[i] = true;
	}

	public GameObject Set(GameObject g, int i)
	{
		objects[i] = g;
		return g;
	}

	public GameObject Get(int i)
	{
		return objects[i];
	}
}
