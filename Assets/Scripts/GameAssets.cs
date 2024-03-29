﻿using System.Collections;
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
    public GameObject[] houses;
    public AudioClip cashSound;
    public AudioClip diceSound;
    public AudioClip policeSound;
    public AudioClip kidsCheering;
    public GameObject getOutOfJailButton;


    public static GameAssets GetInstance()
    {
        return instance;
    }
   
    public void Awake()
    {
        instance = this;
    }

}
