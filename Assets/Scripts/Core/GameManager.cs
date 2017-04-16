using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {

	[Header("Players")]
	public PlayerAI	firstPlayer;
	public PlayerAI	secondPlayer;

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
	public GameObject	rumBarrelPrefab;
	public GameObject	minePrefab;
	public GameObject	explosionPrefab;
	public GameObject	cannonBallPrefab;
	public GameObject	cannonShootPrefab;

	GameReferee		referee;
	HexGrid			hexGrid;

	int				round = 0;

	public Vector2	decal;
	public float	scaleX;
	public float	scaleY;

	const int		FIRST_PLAYER = 0;
	const int		SECOND_PLAYER = 1;

	int					mapWidth;
	int					mapHeight;
	int					playerShipCount;
	int					mineVisibilityRange;

	//TODO: remove these pools
	GameObject[]		rumBarrelPool;
	GameObject[]		playerShipPool;
	GameObject[]		minePool;
	GameObject[]		cannonBallPool;

	GameObject[]		orangeShips;
	GameObject[]		redShips;
	
	List< GameReferee.Player > players = new List< GameReferee.Player >();
	List< GameReferee.Cannonball > cannonBalls = new List< GameReferee.Cannonball >();
	List< GameReferee.Mine > mines = new List< GameReferee.Mine >();
	List< GameReferee.RumBarrel > rumBarrels = new List< GameReferee.RumBarrel >();
	List< GameReferee.Damage > damages = new List< GameReferee.Damage >();

	// Use this for initialization
	void Start () {
		referee = new GameReferee();
		
		rumBarrelPool = new GameObject[GameReferee.MAX_RUM_BARRELS * 20];
		minePool = new GameObject[GameReferee.MAX_MINES * 2];
		cannonBallPool = new GameObject[GameReferee.MAX_SHIPS * 2];

		hexGrid = FindObjectOfType< HexGrid >();

		LoadResources();

		Properties	props = new Properties();
		
		props.put("seed", Random.Range(-200000, 20000));
		props.put("shipsPerPlayer", shipsPerPlayer);
//		props.put("mineCount", mineCount);
//		props.put("barrelCount", barrelCount);

		referee.initReferee(2, props);

		InitVisualizator(referee.getInitDataForView());

		StartCoroutine(ExecuteRound());
	}

	void		LoadResources()
	{
		orangeShips = new GameObject[3];
		redShips = new GameObject[4];
		orangeShips[0] = Resources.Load< GameObject >("boat_orange_cg");
		orangeShips[1] = Resources.Load< GameObject >("boat_orange_sabers");
		orangeShips[2] = Resources.Load< GameObject >("boat_orange_skull");
		redShips[0] = Resources.Load< GameObject >("boat_red__");
		redShips[1] = Resources.Load< GameObject >("boat_red_cg");
		redShips[2] = Resources.Load< GameObject >("boat_red_skull");
		redShips[3] = Resources.Load< GameObject >("boat_red_sabers");

	}

	IEnumerator ExecuteRound()
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenTurns);
	
			var firstPlayerOutput = ExecutePlayerActions(firstPlayer, FIRST_PLAYER);
			var secondPlayerOutput = ExecutePlayerActions(secondPlayer, SECOND_PLAYER);
	
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

	string[]	ExecutePlayerActions(PlayerAI ai, int playerIndex)
	{
		string		playerOutput = "";
		string[]	playerInput = referee.getInputForPlayer(round, playerIndex);
		//user compute

		int						playerShipCount;
		int						entityCount;
		List< ShipData >		ships = new List< ShipData >();
		List< MineData >		mines = new List< MineData >();
		List< CannonBallData > cannonBalls = new List< CannonBallData >();
		List< RumBarrelData >	rumBarrels = new List< RumBarrelData >();
		
        playerShipCount = int.Parse(playerInput[0]);
		entityCount = int.Parse(playerInput[1]);
        for (int i = 0; i < entityCount; i++)
        {
            string[] inputs = playerInput[i + 2].Split(' ');
            int entityId = int.Parse(inputs[0]);

            string entityType = inputs[1];
            int x = int.Parse(inputs[2]);
            int y = int.Parse(inputs[3]);

            int arg1 = int.Parse(inputs[4]);
            int arg2 = int.Parse(inputs[5]);
            int arg3 = int.Parse(inputs[6]);
            int arg4 = int.Parse(inputs[7]);

			switch (entityType)
			{
				case "SHIP":
					ships.Add(new ShipData(x, y, entityId, arg1, arg2, arg3, arg4));
					break;
				case "MINE":
					mines.Add(new MineData(x, y, entityId));
					break ;
				case "CANNONBALL":
					cannonBalls.Add(new CannonBallData(x, y, entityId, arg1, arg2));
					break ;
				case "BARREL":
					rumBarrels.Add(new RumBarrelData(x, y, entityId, arg1));
					break ;
			}
        }
		
		Stopwatch	st = new Stopwatch();
		st.Start();
		playerOutput = ai.PlayTurn(playerShipCount, entityCount, ships, rumBarrels, mines, cannonBalls);
		st.Stop();
		Debug.Log("player " + playerIndex + " AI took " + st.ElapsedMilliseconds + "ms");

		return playerOutput.Split(';');
	}

	void InitVisualizator(string[] infos)
	{
		var datas = infos[1].Split(' ');
		mapWidth = int.Parse(datas[0]);
		mapHeight = int.Parse(datas[1]);
		playerShipCount = int.Parse(datas[2]);
		mineVisibilityRange = int.Parse(datas[3]);
		
		hexGrid.BuildHexMap(mapWidth - 1, mapHeight - 1);

		playerShipPool = new GameObject[playerShipCount * 2];
	}

	GameObject InstanciateShip(int owner)
	{
		if (owner == 0)
			return Instantiate(orangeShips[Random.Range(0, orangeShips.Length)]);
		else
			return Instantiate(redShips[Random.Range(0, redShips.Length)]);
	}

	Vector2	CoordToPosition(GameReferee.Coord position)
	{
		Vector2 pos = new Vector2(position.x, position.y) * HexMetrics.outerRadius;

		pos.x += ((position.y % 2) * HexMetrics.outerRadius) / 2f;
		pos.x *= scaleX;
		pos.y *= scaleY;

		return (pos) + decal;
	}

	void UpdateVisualizator()
	{
		int		i = 0;
		foreach (var g in playerShipPool)
			if (g != null)
				g.SetActive(false);
		foreach (var g in minePool)
			if (g != null)
				g.SetActive(false);
		foreach (var g in rumBarrelPool)
			if (g != null)
				g.SetActive(false);
		foreach (var player in players)
		{
			foreach (var ship in player.ships)
			{
				if (playerShipPool[i] == null)
					playerShipPool[i] = InstanciateShip(ship.owner);
				playerShipPool[i].transform.position = CoordToPosition(ship.position);
				playerShipPool[i].transform.rotation = Quaternion.Euler(0, 0, ship.orientation * 60 + 90);
				playerShipPool[i].SetActive(true);
				i++;
			}
		}
		i = 0;
		foreach (var rumBarrel in rumBarrels)
		{
			if (rumBarrelPool[i] == null)
				rumBarrelPool[i] = Instantiate(rumBarrelPrefab);
			rumBarrelPool[i].transform.position = CoordToPosition(rumBarrel.position);
			rumBarrelPool[i].SetActive(true);
			i++;
		}
		i = 0;
		foreach (var mine in mines)
		{
			if (minePool[i] == null)
				minePool[i] = Instantiate(minePrefab);
			minePool[i].transform.position = CoordToPosition(mine.position);
			minePool[i].SetActive(true);
			i++;
		}
		i = 0;
		foreach (var cannonBall in cannonBalls)
		{

		}
		foreach (var damage in damages)
		{
			GameObject g = Instantiate(explosionPrefab, CoordToPosition(damage.position), Quaternion.identity);
			Destroy(g, 1);
		}
	}

	public void InstanciateAnimation(int x, int y, GameManager anim)
	{
		GameObject.Instantiate(anim, new Vector2(x, y), Quaternion.identity);
	}
}