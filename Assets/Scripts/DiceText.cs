using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceText : MonoBehaviour {
    public Vector2Int dices;

    public void ChangeText(Vector2Int dices)
    {
        int d1 = dices.x;
        int d2 = dices.y;
        if (d1 == d2)
        {
            if (GameObject.Find("DoubleMessage")==null)
            {
                Debug.Log("nuullllll");
            }
            GameObject.Find("DoubleMessage").GetComponent<Text>().text = "DOUBLE!";
        }
        else
            GameObject.Find("DoubleMessage").GetComponent<Text>().text = "";
        this.dices = dices;
        Image dice1 = GameObject.Find("Dice1").GetComponent<Image>();
        Image dice2 = GameObject.Find("Dice2").GetComponent<Image>();
        dice1.sprite =  GameAssets.GetInstance().Dices[d1 - 1];
        dice2.sprite = GameAssets.GetInstance().Dices[d2 - 1];
    }
}
