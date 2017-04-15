﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Player : MonoBehaviour
{
    public void PlayerMain(EchoStream input, EchoStream output)
    {
		try {
        // game loop
        while (true)
        {
            string s = input.ReadLine();
            int myShipCount = int.Parse(s); // the number of remaining ships
            s = input.ReadLine();
            int entityCount = int.Parse(s); // the number of entities (e.g. ships, mines or cannonballs)
            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = input.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int arg1 = int.Parse(inputs[4]);
                int arg2 = int.Parse(inputs[5]);
                int arg3 = int.Parse(inputs[6]);
                int arg4 = int.Parse(inputs[7]);
            }
            for (int i = 0; i < myShipCount; i++)
            {

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                output.WriteLine("MOVE 11 10"); // Any valid action, such as "WAIT" or "MOVE x y"
            }
        }
		} catch (Exception e) {
			Debug.LogError(e);
		}
    }
}