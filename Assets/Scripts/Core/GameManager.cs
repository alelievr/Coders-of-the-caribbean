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
	[Range(0, 3f)]
	public float	timeBetweenTurns = .5f;

	[Space()]
	[Header("View settings")]
	public GameObject	rumBarrelPrefab;
	public GameObject	minePrefab;
	public GameObject	explosionPrefab;
	public GameObject	cannonBallPrefab;
	public GameObject	cannonShootPrefab;

	public static Transform	debugGameObjectRoot;

	GameReferee				referee;
	static HexGrid			hexGrid;
	static PlayerGUI		playerGUI;

	int				round = 0;
	bool			gameOver = false;

	[HideInInspector]
	public Vector2	decal = new Vector2(-4.96f, 1.29f);
	[HideInInspector]
	public float	scaleX = 1.73f;
	[HideInInspector]
	public float	scaleY = 1.51f;

	const int		FIRST_PLAYER = 0;
	const int		SECOND_PLAYER = 1;

	static int			mapWidth;
	static int			mapHeight;
	int					playerShipCount;
	int					mineVisibilityRange;

	//TODO: remove these pools
	GameObjectPool		rumBarrelPool;
	GameObjectPool		minePool;
	GameObjectPool		cannonBallPool;
	GameObject[]		playerShips;

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

		rumBarrelPool = new GameObjectPool(GameReferee.MAX_RUM_BARRELS * 2);
		minePool = new GameObjectPool(GameReferee.MAX_MINES * 2);
		cannonBallPool = new GameObjectPool(GameReferee.MAX_SHIPS * 2);
		playerShips = new GameObject[shipsPerPlayer * 2];

		hexGrid = FindObjectOfType< HexGrid >();
		playerGUI = FindObjectOfType< PlayerGUI >();
		
		debugGameObjectRoot = new GameObject("debugObjectRoot").transform;
		debugGameObjectRoot.parent = hexGrid.transform;
		debugGameObjectRoot.localPosition = Vector3.zero;

		LoadResources();

		StartGame();
	}

	void StartGame()
	{
		Properties	props = new Properties();
		
		props.put("seed", (randomSeed) ? Random.Range(-200000, 20000) : seed);
		props.put("shipsPerPlayer", shipsPerPlayer);
//		props.put("mineCount", mineCount);
//		props.put("barrelCount", barrelCount);

		referee.initReferee(2, props);

		InitVisualizator(referee.getInitDataForView());
		UpdateView();

		StartCoroutine(ExecuteRound());
	}

	void		LoadResources()
	{
		orangeShips = new GameObject[3];
		redShips = new GameObject[4];
		orangeShips[0] = Resources.Load< GameObject >("boat_orange_cg");
		orangeShips[1] = Resources.Load< GameObject >("boat_orange_sabers");
		orangeShips[2] = Resources.Load< GameObject >("boat_orange_skull");
		redShips[0] = Resources.Load< GameObject >("boat_red_cg");
		redShips[1] = Resources.Load< GameObject >("boat_red_sabers");
		redShips[2] = Resources.Load< GameObject >("boat_red_skull");
		redShips[3] = Resources.Load< GameObject >("boat_red__");
	}

	IEnumerator ExecuteRound()
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenTurns);
	
			referee.prepare(round);

			//execute players AI
			var firstPlayerOutput = ExecutePlayerActions(firstPlayer, FIRST_PLAYER);
			var secondPlayerOutput = ExecutePlayerActions(secondPlayer, SECOND_PLAYER);
	
			//send result to the referee
			referee.handlePlayerOutput(0, round, FIRST_PLAYER, firstPlayerOutput);
			referee.handlePlayerOutput(0, round, SECOND_PLAYER, secondPlayerOutput);

			//update game status
			referee.updateGame(round);

			//update view
			UpdateView();
	
			if (referee.isPlayerDead(0))
			{
				GameOver(false);
				break ;
			}
			else if (referee.isPlayerDead(1))
			{
				GameOver(true);
				break ;
			}
	
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
		playerGUI.UpdateCalculTime(playerIndex, (int)st.ElapsedMilliseconds);

		return playerOutput.Split('\n');
	}

	void InitVisualizator(string[] infos)
	{
		var datas = infos[1].Split(' ');
		mapWidth = int.Parse(datas[0]);
		mapHeight = int.Parse(datas[1]);
		playerShipCount = int.Parse(datas[2]);
		mineVisibilityRange = int.Parse(datas[3]);
		
		hexGrid.BuildHexMap(mapWidth, mapHeight);
	}

	GameObject InstanciateShip(int owner, int id)
	{
		if (owner == 0)
			return Instantiate(orangeShips[id]);
		else
			return Instantiate(redShips[id - shipsPerPlayer]);
	}

	Vector2	CoordToPosition(GameReferee.Coord position)
	{
		int y = mapHeight - position.y - 1;
		Vector2 pos = new Vector2(position.x, y) * HexMetrics.outerRadius;

		pos.x += ((y % 2) * HexMetrics.outerRadius) / 2f;
		pos.x *= scaleX;
		pos.y *= scaleY;

		return (pos) + decal;
	}
	
	void UpdateView()
	{
		players.Clear();
		cannonBalls.Clear();
		mines.Clear();
		rumBarrels.Clear();
		damages.Clear();
		referee.getFrameDataForView(players, cannonBalls, mines, rumBarrels, damages);

		UpdateVisualizator();

		//TODO: save old player ships
	}

	IEnumerator fadeAndDestroy(GameObject g, int shipId)
	{
		SpriteRenderer sp = g.GetComponent< SpriteRenderer >();
		SpriteRenderer sailSp = g.GetComponentInChildren< SpriteRenderer >();
		playerShips[shipId] = null;

		for (int i = 0; i < 10; i++)
		{
			sp.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(1f, 0f, (float)i / 10f));
			sailSp.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(1f, 0f, (float)i / 10f));
			yield return new WaitForSeconds(0.16f);
		}
		Destroy(g);
	}

	void UpdateVisualizator()
	{
		int		i = 0;

		minePool.SetUpdated(false);
		rumBarrelPool.SetUpdated(false);

		foreach (var player in players)
		{
			foreach (var ship in player.ships)
			{
				GameObject g = playerShips[ship.id];
				if (ship.health <= 0 && g != null)
					StartCoroutine(fadeAndDestroy(g, ship.id));
				if (ship.health <= 0)
					continue ;
				if (g == null)
					g = playerShips[ship.id] = InstanciateShip(ship.owner, ship.id);
				g.transform.position = CoordToPosition(ship.position);
				g.transform.rotation = Quaternion.Euler(0, 0, ship.orientation * 60 - 90);
				i++;
				playerGUI.UpdatePlayerShipHealth(ship.owner, shipsPerPlayer, ship.id, ship.health);
			}
		}
		i = 0;
		foreach (var rumBarrel in rumBarrels)
		{
			GameObject g;
			if ((g = rumBarrelPool.Get(i)) == null)
				g = rumBarrelPool.Set(Instantiate(rumBarrelPrefab), i);
			g.transform.position = CoordToPosition(rumBarrel.position);
			rumBarrelPool.Update(i);
			i++;
		}
		i = 0;
		foreach (var mine in mines)
		{
			GameObject g;
			if ((g = minePool.Get(i)) == null)
				g = minePool.Set(Instantiate(minePrefab), i);
			g.transform.position = CoordToPosition(mine.position);
			minePool.Update(i);
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

		rumBarrelPool.RemoveUnused(() => {});
		minePool.RemoveUnused(() => {});
	}

	public void InstanciateAnimation(int x, int y, GameManager anim)
	{
		GameObject.Instantiate(anim, new Vector2(x, y), Quaternion.identity);
	}

	public void GameOver(bool win)
	{
		playerGUI.GameOver(win, true);
		gameOver = true;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && gameOver)
		{
			gameOver = false;
			playerGUI.GameOver(true, false);
			playerGUI.GameOver(false, false);
			StartGame();
		}
	}

	//Player access:

	public static void SetCellText(int x, int y, string text)
	{
		if (hexGrid != null)
			hexGrid.SetCellText(x, y, text, Color.white);
	}
	
	public static void SetCellText(int x, int y, string text, Color c)
	{
		if (hexGrid != null)
			hexGrid.SetCellText(x, y, text, c);
	}

	public static void SetCellColor(int x, int y, Color c)
	{
		if (hexGrid != null)
			hexGrid.SetCellColor(x, y, c);
	}

	public static Vector3 GridToWorldPosition(int x, int y)
	{
 		return new Vector3(
			(x * HexMetrics.innerRadius + ((y % 2) * HexMetrics.innerRadius) / 2f) * 2f,
			(y * HexMetrics.innerRadius) * 1.73f,
			-7f);
	}

	public static GameObject AddObjectAt(int x, int y, GameObject g)
	{
		y = mapHeight - y - 1;
		g.transform.parent = debugGameObjectRoot;
		g.transform.localPosition = GridToWorldPosition(x, y);
		return g;
	}
}