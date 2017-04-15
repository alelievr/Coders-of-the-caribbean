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
	public int		timeoutMillisecs = 50;
	public int		firstTurnTimeoutMillisecs = 1000;

	[Space()]
	[Header("View settings")]
	public GameObject	shipPrefab;
	public GameObject	rumBarrelPrefab;
	public GameObject	minePrefab;

	GameReferee		referee;

	int				round = 0;

	const int		FIRST_PLAYER = 0;
	const int		SECOND_PLAYER = 1;

	EchoStream			firstPlayerInputStream = new EchoStream();
	EchoStream			secondPlayerInputStream = new EchoStream();
	EchoStream			firstPlayerOutputStream = new EchoStream();
	EchoStream			secondPlayerOutputStream = new EchoStream();

	Thread				firstPlayerThead;
	Thread				secondPlayerThread;
	
	int					playerShipCount;
	int					mineVisibilityRange;

	GameObject[]		rumBarrelPool;
	GameObject[]		playerShipPool;
	GameObject[]		minePool;
	GameObject[]		cannonBallPool;
	
	List< GameReferee.Player > players = new List< GameReferee.Player >();
	List< GameReferee.Cannonball > cannonBalls = new List< GameReferee.Cannonball >();
	List< GameReferee.Mine > mines = new List< GameReferee.Mine >();
	List< GameReferee.RumBarrel > rumBarrels = new List< GameReferee.RumBarrel >();
	List< GameReferee.Damage > damages = new List< GameReferee.Damage >();

	// Use this for initialization
	void Start () {
		
		rumBarrelPool = new GameObject[100];
		minePool = new GameObject[100];
		cannonBallPool = new GameObject[100];

		referee = new GameReferee();
		Properties	props = new Properties();
		
		props.put("seed", Random.Range(-200000, 20000));
		props.put("shipsPerPlayer", shipsPerPlayer);
//		props.put("mineCount", mineCount);
//		props.put("barrelCount", barrelCount);

		firstPlayerThead = RunUserThread(firstPlayer, firstPlayerInputStream, firstPlayerOutputStream);
		secondPlayerThread = RunUserThread(secondPlayer, secondPlayerInputStream, secondPlayerOutputStream);

		referee.initReferee(2, props);

		InitVisualizator(referee.getInitDataForView());

		StartCoroutine(ExecuteRound());
	}

	void OnDestroy()
	{
		firstPlayerThead.Abort();
		secondPlayerThread.Abort();
	}

	IEnumerator ExecuteRound()
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenTurns);
			string[] firstPlayerInput = referee.getInputForPlayer(round, FIRST_PLAYER);
			string[] secondPlayerInput = referee.getInputForPlayer(round, SECOND_PLAYER);
	
			var firstPlayerOutput = ExecutePlayerActions(firstPlayerInputStream, firstPlayerOutputStream, firstPlayerInput);
			var secondPlayerOutput = ExecutePlayerActions(secondPlayerInputStream, secondPlayerOutputStream, secondPlayerInput);
	
			referee.handlePlayerOutput(0, round, FIRST_PLAYER, firstPlayerOutput);
			referee.handlePlayerOutput(0, round, SECOND_PLAYER, secondPlayerOutput);
			referee.updateGame(round);
	
			players.Clear();
			cannonBalls.Clear();
			mines.Clear();
			rumBarrels.Clear();
			damages.Clear();
			referee.getFrameDataForView(players, cannonBalls, mines, rumBarrels, damages);

			UpdateVisualizator();
	
			round++;
		}
	}

	Thread		RunUserThread(Player p, EchoStream input, EchoStream output)
	{
		Thread t = new Thread(new ThreadStart(() => {
			p.PlayerMain(input, output);
		}));
		t.Start();
		return t;
	}

	string[]	ExecutePlayerActions(EchoStream input, EchoStream output, string[] playerInputs)
	{
		List< string >	playerOutput = new List< string >();

		//send input to user:
		foreach (var playerInput in playerInputs)
			input.WriteLine(playerInput);

		//user compute

		//get output from user with timeout
		Thread t = new Thread(new ThreadStart(() => {
			playerOutput.Add(output.ReadLine());
		}));
		t.Start();
		bool finished = t.Join((round == 0) ? firstTurnTimeoutMillisecs : timeoutMillisecs);
		if (!finished)
			Debug.Log("stopping game, user take too long to response !");

		return playerOutput.ToArray();
	}

	void InitVisualizator(string[] infos)
	{
		int		mapWidth;
		int		mapHeight;

		var datas = infos[1].Split(' ');
		mapWidth = int.Parse(datas[0]);
		mapHeight = int.Parse(datas[1]);
		playerShipCount = int.Parse(datas[2]);
		mineVisibilityRange = int.Parse(datas[3]);
		
		playerShipPool = new GameObject[playerShipCount * 2];
	}

	void UpdateVisualizator()
	{
		int		i = 0;
		foreach (var player in players)
		{
			foreach (var ship in player.ships)
			{
				if (playerShipPool[i] == null)
					playerShipPool[i] = Instantiate(shipPrefab);
				playerShipPool[i].transform.position = new Vector2(ship.position.x, ship.position.y);
				playerShipPool[i++].transform.rotation = Quaternion.Euler(0, 0, ship.orientation * 60 + 90);
			}
		}
		i = 0;
		foreach (var rumBarrel in rumBarrels)
		{
			if (rumBarrelPool[i] == null)
				rumBarrelPool[i] = Instantiate(rumBarrelPrefab);
			rumBarrelPool[i++].transform.position = new Vector2(rumBarrel.position.x, rumBarrel.position.y);
		}
		foreach (var mine in mines)
		{
			if (minePool[i] == null)
				minePool[i] = Instantiate(minePrefab);
			minePool[i].transform.position = new Vector2(mine.position.x, mine.position.y);
		}
	}
}

public class EchoStream {

    private ManualResetEvent m_dataReady = new ManualResetEvent(false);
	string	txt = null;
	
	public string ReadLine()
	{
		string	ret;

		while (txt == null)
			m_dataReady.WaitOne();

		ret = txt;
		txt = null;
		return ret;
	}

	public void WriteLine(string s)
	{
		while (txt != null)
			m_dataReady.WaitOne();

		m_dataReady.Set();
		txt = s;
	}

	/*public void Write(string s)
	{
		lock(m_dataReady)
			while (txt != null)
				m_dataReady.WaitOne();
			
		if (s.Contains("\n"))
			m_dataReady.Set();
		txt += s;
	}*/
}