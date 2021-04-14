using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameAssets : MonoBehaviour {

    private static GameAssets instance;
    public GameObject dogPrefab;
    public GameObject carPrefab;
    public GameObject diceText;
    public GameObject diceButton;
    public GameObject buySquareButton;
    public GameObject buildHouseButton;
    public GameObject mortageButton;
    public GameObject bankruptButton;
    public Sprite[] Dices;
    internal object buyHouseButton;
    public Button[] squareButtons = new Button[28];

    public static GameAssets GetInstance()
    {
        return instance;
    }
   
    public void Awake()
    {
        instance = this;
    }

}
