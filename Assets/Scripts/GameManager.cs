using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System;
using Random = UnityEngine.Random;
using System.Text;

public class GameManager : MonoBehaviour {

	[Header("Players")]
	public Player	firstPlayer;
	public Player	secondPlayer;

	[Space()]
	[Header("Game Config")]
	public int		playerCount = 2;
	public int		shipsPerPlayer = 1;
	public int		mineCount = 0;
	public int		barrelCount = 5;

	[Space()]
	[Header("Settings")]
	public bool		randomSeed = true;
	public int		seed = 42;
	public float	timeBetweenTurns = .5f;

	GameReferee		referee;

	int				round = 0;

	const int		FIRST_PLAYER = 0;
	const int		SECOND_PLAYER = 1;

	Stream			firstPlayerOutputStream = new MemoryStream();
	Stream			secondPlayerOutputStream = new MemoryStream();
	Stream			firstPlayerInputStream = new MemoryStream();
	Stream			secondPlayerInputStream = new MemoryStream();

	// Use this for initialization
	void Start () {
		referee = new GameReferee();
		Properties	props = new Properties();
		
		props.put("seed", Random.Range(-200000, 20000));
		props.put("shipsPerPlayer", shipsPerPlayer);
//		props.put("mineCount", mineCount);
//		props.put("barrelCount", barrelCount);

		firstPlayerOutputStream.ReadTimeout = 50;
		secondPlayerOutputStream.ReadTimeout = 50;

		RunUserThread(firstPlayer, firstPlayerInputStream, firstPlayerOutputStream);
		RunUserThread(secondPlayer, secondPlayerInputStream, secondPlayerInputStream);

		referee.initReferee(2, props);

		UpdateVisualizator(referee.getInitDataForView());

		StartCoroutine(ExecuteRound());
	}

	IEnumerator ExecuteRound()
	{
		yield return new WaitForSeconds(timeBetweenTurns);
		string[] firstPlayerInput = referee.getInputForPlayer(round, FIRST_PLAYER);
		string[] secondPlayerInput = referee.getInputForPlayer(round, SECOND_PLAYER);

		//TODO: call player scripts to get actions
		var firstPlayerOutput = ExecutePlayerActions(firstPlayerOutputStream, firstPlayerInputStream, firstPlayerInput);
		var secondPlayerOutput = ExecutePlayerActions(secondPlayerOutputStream, secondPlayerInputStream, secondPlayerInput);

		referee.handlePlayerOutput(0, round, FIRST_PLAYER, firstPlayerOutput);
		referee.handlePlayerOutput(0, round, SECOND_PLAYER, secondPlayerOutput);
		referee.updateGame(round);

		round++;
	}

	void		RunUserThread(Player p, Stream input, Stream output)
	{
		new Thread(new ThreadStart(() => {
			StreamWriter sw = new StreamWriter(output);
			StreamReader sr = new StreamReader(input);
			Console.SetOut(sw);
			Console.SetIn(sr);
			p.PlayerMain();
		}));
	}

	string[]	ExecutePlayerActions(Stream playerOutput, Stream playerInput, string[] inputs)
	{
		List< string >	playerRet = new List< string >();
		byte[]		data = new byte[1024];

		//send input to user:
		foreach (var input in inputs)
			playerInput.Write(Encoding.ASCII.GetBytes(input), 0, 1);

		//user compute

		//get output from user
		while (true)
		{
			try {
				playerOutput.Read(data, 0, 1);
			} catch (TimeoutException) {
				Debug.Log("stopping game, user take too long to response !");
				break ;
			}
		}
		return Encoding.UTF8.GetString(data).Split('\n');
	}

	void UpdateVisualizator(string[] infos)
	{
		foreach (var info in infos)
		{
			Debug.Log("info: " + info);
		}
	}
}
