using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour {

	public Image[]	orangeShipHealthImages;
	public Text[]	orangeShipHealthTexts;

	[SpaceAttribute]
	public Image[]	redShipHealthImages;
	public Text[]	redShipHealthTexts;

	[SpaceAttribute]
	public Text		orangePlayerCalculusTime;
	public Text		redPlayerCalculusTime;

	[SpaceAttribute]
	public GameObject	winPanel;
	public GameObject	loosePanel;

	[SpaceAttribute]
	public Text		roundText;
	public Slider	roundSlider;
	public Image	pauseImageButton;
	public Sprite	pauseSprite;
	public Sprite	playSprite;

	[SpaceAttribute]
	public Text		enemyNameText;
	public Text		playerNameText;
	
	[SpaceAttribute]
	public Text[]	playerShipDebugTexts;

	[SpaceAttribute]
	public Image	enemyAvatar;
	public Text		enemyLeagueLevel;
	public Sprite[]	leagueLevels;

	public void UpdatePlayerShipHealth(int player, int shipPerPlayer, int shipId, int health)
	{
		shipId /= 2;
		if (player == GameManager.FIRST_PLAYER)
		{
			orangeShipHealthImages[shipId].fillAmount = (health / 100f);
			orangeShipHealthTexts[shipId].text = health.ToString();
		}
		else
		{
			redShipHealthImages[shipId].fillAmount = (health / 100f);
			redShipHealthTexts[shipId].text = health.ToString();
		}
	}

	public void GameOver(bool win, bool display = true)
	{
		if (win)
			winPanel.SetActive(display);
		else
			loosePanel.SetActive(display);
	}

	public void UpdateCalculTime(int player, int ms)
	{
		if (player == 0)
			orangePlayerCalculusTime.text = ms + "ms";
		else
			redPlayerCalculusTime.text = ms + "ms";
	}

	public void UpdateRoundNumber(int r, int total)
	{
		roundText.text = r + " / " + total;
		roundSlider.maxValue = total;
		roundSlider.value = r;
	}

	public void SetPauseButtonImage(bool paused)
	{
		if (paused)
			pauseImageButton.sprite = pauseSprite;
		else
			pauseImageButton.sprite = playSprite;
	}

	public void UpdateEnemyName(string name)
	{
		enemyNameText.text = name;
	}

	public void UpdatePlayerName(string name)
	{
		playerNameText.text = name;
	}

	public void UpdateShipDebugText(int shipId, string debug)
	{
		playerShipDebugTexts[shipId].text = debug;
	}

	public void UpdateEnemyLeagueLevel(LeagueLevel ll)
	{
		enemyLeagueLevel.text = ll.ToString();
		enemyAvatar.sprite = leagueLevels[(int)ll];
	}
}
