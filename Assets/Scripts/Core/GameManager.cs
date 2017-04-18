using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour {

	[Header("Players")]
	public PlayerAI		playerAI;
	public PlayerAI[]	enemyAIs;

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
	[Range(0, 6)]
	public int		previousFrameVisibility = 2;
	[Range(0, 6)]
	public int		folowingFrameVisibility = 2;

	[Space()]
	[Header("View settings")]
	public GameObject	rumBarrelPrefab;
	public GameObject	minePrefab;
	public GameObject	explosionPrefab;
	public GameObject	cannonBallPrefab;
	public GameObject	cannonShootPrefab;
	
#region Internal manager variables
	const int		FIRST_PLAYER = 0;
	const int		SECOND_PLAYER = 1;

	int				enemyAIId = 0;

	GameReferee				referee;
	static HexGrid			hexGrid;
	static PlayerGUI		playerGUI;

	[HideInInspector]
	public Vector2	decal = new Vector2(-4.96f, 1.29f);
	[HideInInspector]
	public float	scaleX = 1.73f;
	[HideInInspector]
	public float	scaleY = 1.51f;

	public static Transform	debugGameObjectRoot;

	static int			mapWidth;
	static int			mapHeight;
	int					round = 0;
	bool				paused = false;
	int					totalRounds;
	bool				sliderUpdateDisabled = false;
	bool				gameOver = false;
	int					mineVisibilityRange;

	GameObjectPool					rumBarrelPool;
	GameObjectPool					minePool;
	GameObjectPool					cannonBallPool;
	GameObject[]					playerShips;

	GameObject[]					orangeShips;
	GameObject[]					redShips;
	
	List< GameReferee.Player >		players = new List< GameReferee.Player >();
	List< GameReferee.Cannonball >	cannonBalls = new List< GameReferee.Cannonball >();
	List< GameReferee.Mine >		mines = new List< GameReferee.Mine >();
	List< GameReferee.RumBarrel >	rumBarrels = new List< GameReferee.RumBarrel >();
	List< GameReferee.Damage >		damages = new List< GameReferee.Damage >();

	List< GameReferee.Player >		oldPlayers;

	Dictionary< int, GameSnapshot >			snapshots = new Dictionary< int, GameSnapshot >();
#endregion

#region Start and Initializaion

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
		
		snapshots.Clear();
		
		props.put("seed", (randomSeed) ? Random.Range(-200000, 20000) : seed);
		props.put("shipsPerPlayer", shipsPerPlayer);
		//props.put("mineCount", mineCount);
		//props.put("barrelCount", barrelCount);

		referee.initReferee(2, props);

		InitVisualizator(referee.getInitDataForView());
		UpdateView();

		oldPlayers = players;

		StartCoroutine("ExecuteRound");
	}

	void		ReStartGame()
	{
		gameOver = false;
		playerGUI.GameOver(true, false);
		playerGUI.GameOver(false, false);
		StartGame();
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
#endregion

#region Rounds execution

	IEnumerator ExecuteRound()
	{
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenTurns);
	
			referee.prepare(round);

			//execute players AI
			var firstPlayerOutput = ExecutePlayerActions(playerAI, FIRST_PLAYER);
			var secondPlayerOutput = ExecutePlayerActions(enemyAIs[enemyAIId], SECOND_PLAYER);
	
			//send result to the referee
			referee.handlePlayerOutput(0, round, FIRST_PLAYER, firstPlayerOutput);
			referee.handlePlayerOutput(0, round, SECOND_PLAYER, secondPlayerOutput);

			//update game status
			referee.updateGame(round);

			//update view
			UpdateView();

			//take a snapshots of the round
			snapshots[round] = new GameSnapshot(referee, oldPlayers);

			oldPlayers = players;

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
			totalRounds = Mathf.Max(totalRounds, round);
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
#endregion

#region Visualization

	void InitVisualizator(string[] infos)
	{
		var datas = infos[1].Split(' ');
		mapWidth = int.Parse(datas[0]);
		mapHeight = int.Parse(datas[1]);
		mineVisibilityRange = int.Parse(datas[3]);
		
		hexGrid.BuildHexMap(mapWidth, mapHeight);
	}

	Queue< IEnumerator > animationCoroutines = new Queue< IEnumerator >();

	IEnumerator ShipAnimation(GameReferee.Ship ship, GameReferee.Ship oldShip)
	{
		GameObject shipGO = playerShips[ship.id];
		if (shipGO == null)
			yield return null;

		int		nIter = Mathf.RoundToInt(timeBetweenTurns / Time.deltaTime);
		int		i = 0;

		while (i != nIter)
		{
			float t = ((float)i / nIter);
			shipGO.transform.position = Vector3.Lerp(shipGO.transform.position, CoordToPosition(ship.position), t);
			shipGO.transform.rotation = Quaternion.Lerp(shipGO.transform.rotation, Quaternion.Euler(0, 0, ship.orientation * 60 - 90), t);
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

	void StartAnimations()
	{
		//TODO: start animations of ghosts

		//TODO: animate cannon balls

		if (oldPlayers == null)
			return ;
		for (int i = 0; i < players.Count; i++)
		{
			var player = players[i];
			var oldPlayer = oldPlayers[i];

			for (int j = 0; j < player.ships.Count; j++)
			{
				var ship = player.ships[j];
				var oldShip = oldPlayer.ships[j];

				var anim = ShipAnimation(ship, oldShip);
				animationCoroutines.Enqueue(anim);
				StartCoroutine(anim);
			}
		}
	}

	void StopAnimations()
	{
		foreach (var anim in animationCoroutines.ToList())
			StopCoroutine(animationCoroutines.Dequeue());
		
		//TODO: end animations of ghosts
	}

	void ShowPreviousBoatGhost()
	{
		for (int i = 0; i < previousFrameVisibility; i++)
		{
			GameReferee 				gr;
			List< GameReferee.Player >	oldPlayers;

			if (round - i > 0)
			{
				snapshots[round - i].FastCheck(out gr, out oldPlayers);
				//instanciate ghosts
			}
		}
	}

	void ShowFolowingAnimations()
	{
		for (int i = 0; i < folowingFrameVisibility; i++)
		{
			GameReferee 				gr;
			List< GameReferee.Player >	oldPlayers;

			if (round + i < snapshots.Count)
			{
				snapshots[round + i].FastCheck(out gr, out oldPlayers);
				//instanciate ghosts
			}
		}
	}

	void UpdateView()
	{
		players.Clear();
		cannonBalls.Clear();
		mines.Clear();
		rumBarrels.Clear();
		damages.Clear();
		referee.getFrameDataForView(players, cannonBalls, mines, rumBarrels, damages);

		sliderUpdateDisabled = true;
		playerGUI.UpdateRoundNumber(round, totalRounds);
		sliderUpdateDisabled = false;

		UpdateVisualizator();

		StartAnimations();
	}

	void ApplyShipVisibilityRange(int playerIndex)
	{
		var ships = players.Where(p => p.id == playerIndex).Select(p => p.ships);

		for (int x = 0; x < mapWidth; x++)
			for (int y = 0; y < mapHeight; y++)
				hexGrid.SetCellColorFilter(x, y, Color.black);

		foreach (var ship in ships.First())
		{
			int bounds = mineVisibilityRange;
			for (int x = -bounds; x <= bounds; x++)
				for (int y = -bounds; y <= bounds; y++)
				{
					int	lx = x + ship.position.x;
					int	ly = y + ship.position.y;
					var point = new GameReferee.Coord(lx, ly);
					if (lx < mapWidth && lx >= 0 && ly < mapHeight && ly >= 0)
						if (ship.position.distanceTo(point) < mineVisibilityRange + 1)
							hexGrid.SetCellColorFilter(lx, ly, new Color(.15f, .15f, .15f));
				}
		}
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

		ApplyShipVisibilityRange(FIRST_PLAYER);

		hexGrid.UpdateMap();

		rumBarrelPool.RemoveUnused(() => {});
		minePool.RemoveUnused(() => {});
	}

	public void GameOver(bool win)
	{
		playerGUI.GameOver(win, true);
		gameOver = true;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && gameOver)
			ReStartGame();
	}
#endregion 

#region Player API

	//Player access functions:

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

	public static void SetShipDebugText(int shipId, string debug)
	{
		if (playerGUI != null)
			playerGUI.UpdateShipDebugText(shipId, debug);
	}
#endregion

#region GUI Callbacks

	//GUI callbacks:
	public void OnFirstClicked()
	{
		round = 0;
		snapshots[0].Restore(out referee, out oldPlayers);
		UpdateView();
		Pause();
		round++;
		UnPause();
	}

	public void OnPrevClicked()
	{
		if (round != 0)
		{
			round--;
			snapshots[round].Restore(out referee, out oldPlayers);
			UpdateView();
			Pause();
		}
	}

	public void OnPlayClicked()
	{
		if (paused)
		{
			round++;
			UnPause();
		}
		else
			Pause();
	}

	public void OnNextClicked()
	{
		if (round < totalRounds - 1)
		{
			round++;
			snapshots[round].Restore(out referee, out oldPlayers);
			UpdateView();
			Pause();
		}
	}

	public void OnLastClicked()
	{
		snapshots.Last().Value.Restore(out referee, out oldPlayers);
		round = snapshots.Count - 1;
		UpdateView();
		Pause();
		round++;
		UnPause();
	}

	public void OnRoundSliderValueChanged(float val)
	{
		if (!sliderUpdateDisabled && val < snapshots.Count)
		{
			round = (int)val;
			snapshots[(int)val].Restore(out referee, out oldPlayers);
			Pause();
			UpdateView();
		}
	}

	public void OnEnemyUpButtonClicked()
	{
		enemyAIId = (enemyAIId == 0) ? enemyAIs.Length - 1 : enemyAIId - 1;
		playerGUI.UpdateEnemyName(enemyAIs[enemyAIId].name);
		ReStartGame();
	}

	public void OnEnemyDownButtonClicked()
	{
		enemyAIId = ++enemyAIId % enemyAIs.Length;
		playerGUI.UpdateEnemyName(enemyAIs[enemyAIId].name);
		ReStartGame();
	}

#endregion

#region Utils
	GameObject InstanciateShip(int owner, int id)
	{
		if (owner == FIRST_PLAYER)
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
	
	void Pause()
	{
		if (paused)
			return ;

		Debug.Log("paused");
		playerGUI.SetPauseButtonImage(paused);

		StopCoroutine("ExecuteRound");
		Time.timeScale = 0;
		paused = true;
	}

	void UnPause()
	{
		if (!paused)
			return ;
		
		Debug.Log("unpause");
		playerGUI.SetPauseButtonImage(paused);

		StartCoroutine("ExecuteRound");
		Time.timeScale = 1;
		paused = false;
	}

#endregion
}