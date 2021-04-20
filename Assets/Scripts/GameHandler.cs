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
public class CardRoot
{
    public string message { get; set; }
    public int kind { get; set; }
    public int? money { get; set; }
    public int? pos { get; set; }
}



public class GameHandler : MonoBehaviour {
    public static GameHandler instance;
    private static GameObject[] squares=new GameObject[40];
    public static Square[] squareInfo=new Square[40];
    private static Dictionary<string, Street> streetInfo=new Dictionary<string, Street>();
    public List<GameObject> players = new List<GameObject>();
    private static int TurnIndex;
    private int[] buttonsToSquares = new int[28];
    public static Queue<CardRoot> chanceCards = new Queue<CardRoot>();
    public static Queue<CardRoot> communityChestCards = new Queue<CardRoot>();
    public static GameHandler GetInstance()
    {
        return instance;
    }
    private void Start()
    {
        instance = this;
        TurnIndex = 0;
        //CreateSquares();
        UseSquares();
        SetCards();
        players.Add(Instantiate(GameAssets.GetInstance().dogPrefab));
        players.Add(Instantiate(GameAssets.GetInstance().carPrefab));
        players[0].GetComponent<Player>().name="Dog";
        players[1].GetComponent<Player>().name = "Car";
        ChangeButtons();
        SetButtons();
        
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener((Toss));
        GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
        
        
    }
    private void SetCards()
    {
        string json;
        System.Random random = new System.Random();
        using (StreamReader r = new StreamReader("Assets/Data/Chance.json"))
        {
            json = r.ReadToEnd();
        }
        List<CardRoot> chanceJson = JsonConvert.DeserializeObject<List<CardRoot>>(json);
        CardRoot[] chanceArr = chanceJson.ToArray();
        Shuffle<CardRoot>(random, chanceArr);
        chanceCards = new Queue<CardRoot>(chanceArr);
        
        using (StreamReader r = new StreamReader("Assets/Data/CommunityChest.json"))
        {
            json = r.ReadToEnd();
        }
        List<CardRoot> communityChestJson = JsonConvert.DeserializeObject<List<CardRoot>>(json);
        CardRoot[] communityChestArr = communityChestJson.ToArray();
        Shuffle<CardRoot>(random, communityChestArr);
        communityChestCards = new Queue<CardRoot>(communityChestArr);
    }
    public void DrawCard(bool Cardtype, Player p)
    {
        
        int money;
        Vector2Int count;
        bool load = true;
        string type;
        Queue<CardRoot> queue;
        if (Cardtype)
        {
            queue = chanceCards;
            type = "Chance";
        }
        else
        {
            queue = communityChestCards;
            type = "Community Chest";
        }
        CardRoot card = queue.Dequeue();
        ShowCard(card);
        switch (card.kind)
        {
            case 0:
                if (p.GetPos() == 0)
                    p.SetMoney(p.GetMoney() - 200);
                p.Wait("MoveTo", (int)card.pos);
                squareInfo[p.GetPos()].SteppedOn(p);
                Debug.Log(p.name + " was sent to " + squareInfo[p.GetPos()].GetSquareName() + " via " + type + " card");//update
                break;
            case 1:
                int minRail=5;
                int minDis = 40;
                int rail = 5;
                for (int i = 0; i < 4; i++)
                {
                    if (Math.Abs(p.GetPos()-rail)<minDis)
                    {
                        minDis = Math.Abs(p.GetPos() - rail);
                        minRail = rail;
                    }
                    rail += 10;
                }
                p.Wait("MoveTo", minRail);
                squareInfo[minRail].SteppedOn(p);
                squareInfo[minRail].SteppedOn(p);
                Debug.Log(p.name + " was sent to " + squareInfo[minRail].GetSquareName() + " via " + type + " card");//update
                break;
            case 2:
                p.AddMoney((int)card.money);
                Debug.Log(p.name + " was given " + card.money + " due " + type + " card");//update
                break;
            case 3:
                p.getOutOfJailCount++;
                load = false;
                Debug.Log(p.name + " given a \" free out of jail card\"");//update
                break;
            case 4:
                p.Wait("", 0);
                squareInfo[p.GetPos()].SteppedOn(p);
                Debug.Log(p.name + " sent back 3 steps");//update
                break;
            case 5:
                p.Wait("Jail", 0);
                break;
            case 6:
                money = 0;
                count = p.CountHousesAndHotels();
                money += count.x * 25 + count.y * 100;
                p.Pay(money);
                Debug.Log(p.name + " payed " + money + " due " + type + " card");//update
                break;
            case 7:
                foreach (GameObject go in players)
                {
                    Player player = go.GetComponent<Player>();
                    if (player.active)
                    {
                        player.AddMoney(p.Pay((int)card.money));
                        Debug.Log(p.name + " payed " + card.money + " to " + player.name);//update
                    }
                }
                break;
            case 8:
                money = 0;
                count = p.CountHousesAndHotels();
                money += count.x * 40 + count.y * 115;
                p.Pay(money);
                Debug.Log(p.name + " payed " + money + " via " + type + " card");//update
                break;
            case 9:
                p.Pay((int)card.money);
                Debug.Log(p.name + " payed " + card.money + " via " + type + " card");//update
                break;
            case 10:
                foreach (GameObject go in players)
                {
                    Player player = go.GetComponent<Player>();
                    if (player.active)
                    {
                        p.AddMoney(player.Pay((int)card.money));
                        Debug.Log(player.name + " payed " + card.money + " to " + p.name);//update
                    }
                }
                break;
            default:
                break;
        }
        if (load)
            queue.Enqueue(card);
        

    }
    private void ShowCard(CardRoot cardRoot)
    {
        GameObject card = GameObject.Find("Square_image");
        GameObject colorImage = GameObject.Find("Color_image");
        Text text = card.GetComponentInChildren<Text>();
        text.text = cardRoot.message;
        Image image = colorImage.GetComponent<Image>();
        Color color = image.color;
        Color color1;
        ColorUtility.TryParseHtmlString("#AEAEAEFF", out color1);
        image.color = color1;
    }
    public static void Shuffle<T>(System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
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
        Player cPlayer = players[(TurnIndex%players.Count)].GetComponent<Player>();
        System.Random random = new System.Random();
        int d1 = random.Next(1, 7);
        int d2 = random.Next(1, 7);
        Vector2Int dices = new Vector2Int(d1, d2);
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
            Debug.Log(cPlayer.name + " landed on " + squareInfo[cPlayer.GetPos()].GetSquareName());
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
        bool changed = false;
        if (TurnIndex==-1)
        {
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
            if (cPlayer.freeParkingCount > 0)
            {
                cPlayer.freeParkingCount--;
                PassTurn();
            }
            else
            {
                SetButtons();
                TurnDot();
                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
                GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener((Toss));

                GameAssets.GetInstance().buySquareButton.GetComponent<Button>().enabled = false;
                GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().enabled = false;
                GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.RemoveAllListeners();
                GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.AddListener(() =>
                {
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
                    TurnIndex++;
                });
            }
        }
    }
    private void TurnDot()
    {
        List<GameObject> dots = new List<GameObject>();
        dots.Add(GameObject.Find("dot1"));
        dots.Add(GameObject.Find("dot2"));
        int i = TurnIndex % players.Count;
        dots[i].GetComponent<Image>().enabled=true;
        dots.RemoveAt(TurnIndex%players.Count);
        dots[0].GetComponent<Image>().enabled = false;
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
