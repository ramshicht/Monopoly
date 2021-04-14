using Assets.Script;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;

public class SquareRoot
{
    public int id { get; set; }
    public string squareName { get; set; }
    public int? action { get; set; }
    public int? cost { get; set; }
    public string streetColor { get; set; }
    public int? mortgage { get; set; }
    public List<int> rentPerHouse { get; set; }
}
public class StreetRoot
{
    public string color { get; set; }
    public int? housePrice { get; set; }
}



public class GameHandler : MonoBehaviour {
   // private const float SQUARE_GAP = 1f;
    private static GameObject[] squares=new GameObject[40];
    private static Square[] squareInfo=new Square[40];
    private static Dictionary<string, Street> streetInfo=new Dictionary<string, Street>();
    private List<GameObject> players = new List<GameObject>();
    private static int TurnIndex;
    private int[] buttonsToSquares = new int[28];
    private void Start()
    {
        TurnIndex = 0;
        //CreateSquares();
        UseSquares();
        players.Add(Instantiate(GameAssets.GetInstance().dogPrefab));
        players.Add(Instantiate(GameAssets.GetInstance().carPrefab));
        players[0].GetComponent<Player>().name="Dog";
        players[1].GetComponent<Player>().name = "Car";
        ChangeButtons();
        SetButtons();
        
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener((Toss));
        GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
        
        
    }
    private void SetButtons()
    {
        //Button[] buttons = GameAssets.GetInstance().squareButtons;
         PropertySquare propertySquare;
        int j = 0;
        foreach (int i in buttonsToSquares)
        {
                //propertySquare =  (PropertySquare)squareInfo[i];
                GameObject gameObject = GameObject.Find("Button" + j);
                Button button = gameObject.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    propertySquare = (PropertySquare)squareInfo[i];
                    propertySquare.ChangeCurrentSquare(GetCurrentPlayer());
                });
            j++;
        }
    }
    private void ChangeButtons()
    {
        int j = 0;
        for (int i = 0; i < 40; i++)
        {
            if (squareInfo[i].ToString().Length > 20)
            {
                buttonsToSquares[j] = i;
                j++;
            }
        }
    }
    private void Toss()
    {
        Debug.Log("ti " + TurnIndex + " count " + players.Count + ", res " + TurnIndex % players.Count);
        Player cPlayer = players[(TurnIndex%players.Count)].GetComponent<Player>();
        System.Random random = new System.Random();
        int d1 = random.Next(1, 7);
        int d2 = random.Next(1, 7);
        Vector2Int dices = new Vector2Int(d1, d2);
        Debug.Log(dices);
        GameObject.Find("DiceText").GetComponent<DiceText>().ChangeText(dices);
        if (cPlayer.GetPos()+dices.x+dices.y>40)
        {
            cPlayer.isFirstRound = false;
        }
        bool canMove = true;
        if (cPlayer.jailCount != 0)
        {
            if (IsDoubleDice(dices))
                cPlayer.jailCount = 0;
            else
            {
                cPlayer.jailCount--;
                canMove = false;
                cPlayer.doublesCount = 0;
            }
        }
        else
        {
            if (cPlayer.freeParkingCount != 0)
            {
                
                canMove = false;
            }
            else
            {
                if (IsDoubleDice(dices))
                {
                    cPlayer.doublesCount++;
                    if (cPlayer.doublesCount == 3)
                    {
                        cPlayer.doublesCount = 0;
                        cPlayer.Jail();
                        canMove = false;
                    }
                    else
                        TurnIndex--;
                }
                else
                {
                    cPlayer.doublesCount = 0;
                }
            }
        }
        if (canMove)
        {
            Move(cPlayer, dices);
            squareInfo[cPlayer.GetPos()].SteppedOn(cPlayer);
            if (cPlayer.jailCount != 0 && IsDoubleDice(dices)) 
                TurnIndex++;
        }
        GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "End Your Turn";
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener(PassTurn);
    }
    private Player GetCurrentPlayer()
    {
        return players[(TurnIndex % players.Count)].GetComponent<Player>();
    }
    private void PassTurn()
    { 
        
        Debug.Log("ti " + TurnIndex + " count " + players.Count + ", res " + TurnIndex % players.Count);
        bool changed = false;
        if (TurnIndex==-1)
        {
            Debug.Log("why u here?");
            TurnIndex++;
            changed = true;
        }
        if (GetCurrentPlayer().GetComponent<Player>().GetMoney()<0)
        {
            Debug.Log("Mortage some propetys before you can finish your turn, or bankrupt");//popup
        }
        else
        {
            if (!changed)
                TurnIndex++;
            Player cPlayer = players[(TurnIndex % players.Count)].GetComponent<Player>();
            if (!cPlayer.active)
                PassTurn();
            SetButtons();
            if (cPlayer.freeParkingCount > 0)
            {
                cPlayer.freeParkingCount--;
                PassTurn();
            }
            else
            {
                if (TurnIndex == players.Count)
                    TurnIndex = 0;
                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
                GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener((Toss));

                GameAssets.GetInstance().buySquareButton.GetComponent<Button>().enabled = false;
                GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().enabled = false;
                GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.RemoveAllListeners();
                GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("Hello bank");
                    cPlayer.active = false;
                    cPlayer.Bankrupt();
                    int playerCount=0;
                    Player p=new Player("winner");
                    foreach (GameObject go in players)
                    {
                        if (go.GetComponent<Player>().active)
                        {
                            playerCount++;
                            p = go.GetComponent<Player>();
                        }
                    }
                    if (playerCount==1)
                    {
                        Debug.Log(p.name + " is The Winner!");
                        EndMatch();
                    }
                    
                     
                });
            }
        }
    }
    private void EndMatch()
    {
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
        GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.RemoveAllListeners();
    }
    private bool IsDoubleDice(Vector2Int dices)
    {
        return dices.y == dices.x;
    }
    private void Move(Player player, Vector2Int dices)
    {
        player.Move(dices.x + dices.y);
    }
    private void UseSquares()
    {
        string json;
        StreetRoot street;
        SquareRoot square;
        
        using (StreamReader r = new StreamReader("Assets/Data/Squares.json"))
        {
            json = r.ReadToEnd();
        }

        List<SquareRoot> squaresJson =JsonConvert.DeserializeObject<List<SquareRoot>>(json);
        using (StreamReader r = new StreamReader("Assets/Data/Streets.json"))
        {
            json = r.ReadToEnd();
        }
        List<StreetRoot> streets = JsonConvert.DeserializeObject<List<StreetRoot>>(json);
        
        for (int i = 0; i < 10; i++)
        {
            street = streets[i];
            if (street.housePrice==null)
            {
                Street s = new Street(street.color);
                streetInfo.Add(s.GetColor(), s);
            }
            else
            {
                Street s = new Street(street.color,(int)street.housePrice);
                streetInfo.Add(s.GetColor(), s);
            }
        }
        Street CurrentStreet;
        for (int i = 0; i < 40; i++)
        {
            square = squaresJson[i];
            

            
            if (square.action!=null)
            {
                squareInfo[i] = new ActionSquare(i, square.squareName, (Assets.Script.Action)(int)square.action);
            }
            else
            {
                CurrentStreet = streetInfo[square.streetColor];
                switch (square.streetColor)
                {
                    case "Trains":
                        squareInfo[i] = new TrainSquare(i, square.squareName, CurrentStreet);
                        CurrentStreet.AddPropertySquare((TrainSquare)squareInfo[i]);
                        break;
                    case "Utility":
                        squareInfo[i] = new UtilitySquare(i, square.squareName, CurrentStreet);
                        CurrentStreet.AddPropertySquare((UtilitySquare)squareInfo[i]);
                        break;
                    default:
                        squareInfo[i] = new PropertySquare(i,square.squareName,(int)square.cost,(int)square.mortgage, CurrentStreet,square.rentPerHouse);
                        CurrentStreet.AddPropertySquare((PropertySquare)squareInfo[i]);
                        break;
                }
            }
        }
    }
  

}
