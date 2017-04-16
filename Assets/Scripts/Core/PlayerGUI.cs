using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour {

	public Image[]	orangeShipHealthImages;
	public Text[]	orangeShipHealthTexts;

	[SpaceAttribute]
	public Image[]	redShipHealthImages;
	public Text[]	redShipHealthTexts;

	public void UpdatePlayerShipHealth(int player, int shipPerPlayer, int shipId, int health)
	{
		if (player == 0)
		{
			orangeShipHealthImages[shipId].fillAmount = (health / 100f);
			orangeShipHealthTexts[shipId].text = health.ToString();
		}
		else
		{
			redShipHealthImages[shipId - player * shipPerPlayer].fillAmount = (health / 100f);
			redShipHealthTexts[shipId - player * shipPerPlayer].text = health.ToString();
		}
	}
}
