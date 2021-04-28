using Assets.Script;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;
using Assets.Scripts;

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
    public bool haveBot;
    private Computer computer;
    public static GameHandler instance;
    public Square[] squareInfo=new Square[40];
    public Dictionary<string, Street> streetInfo=new Dictionary<string, Street>();
    public List<GameObject> players = new List<GameObject>();
    private static int TurnIndex;
    private int[] buttonsToSquares = new int[28];
    public Queue<CardRoot> chanceCards = new Queue<CardRoot>();
    public Queue<CardRoot> communityChestCards = new Queue<CardRoot>();
    public AudioSource audioSource;
    private static System.Random random = new System.Random(Environment.TickCount);

    public static GameHandler GetInstance()
    {
        return instance;
    }//returns this game handler object
    private void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        TurnIndex = 0;
        UseSquares();
        SetCards();
        players.Add(Instantiate(GameAssets.GetInstance().dogPrefab));
        players.Add(Instantiate(GameAssets.GetInstance().carPrefab));
        players[0].GetComponent<Player>().name="Dog";
        if (StartMenuHandler.GetInstance().haveBot)
        {
            
            players[1].GetComponent<Player>().name = "Bot Nick";
            players[1].GetComponent<Player>().isBot = true;
            Text text = GameObject.Find("name2").GetComponent<Text>();
            text.text = "Bot Nick";
            computer = new Computer(players[1].GetComponent<Player>());
        }
        else
        {
            players[1].GetComponent<Player>().name = "Car";
        }
        
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
    }//sets the Chance and Community ChestCards
    public void PushMessage(string message)
    {
        string tmp1, contentName= "TextListContent ";
        for (int i = 1; i <= 7; i++)
        {
            GameObject content = GameObject.Find(contentName + "(" + i + ")");
            Text text = content.GetComponentInChildren<Text>();
            content.GetComponent<Image>().enabled = true ;
            if (text.text == "")
            {
                text.text = message;
                break;
            }
            else
            {
                tmp1 = text.text;
                text.text = message;
                message = tmp1;
            }
        }
    }//pushes message
    public void DrawCard(bool Cardtype, Player p)//draws a Chance or Community Chest
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
        ShowCard(card.message);
        switch (card.kind)
        {
            case 0:
                if ((int)card.pos == 0)
                    p.SetMoney(p.GetMoney() - 200);
                p.Wait("MoveTo", (int)card.pos,1.5f);
                PushMessage(p.name + " was sent to " + squareInfo[(int)card.pos].GetSquareName() + " via " + type + " card");
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
                p.Wait("MoveToRail", minRail,1.5f);
                PushMessage(name + " was sent to nears railroad, " + squareInfo[minRail].GetSquareName() + " via " + type + " card");
                break;
            case 2:
                p.AddMoney((int)card.money);
                PushMessage(p.name + " collected " + card.money + " due " + type + " card");
                break;
            case 3:
                p.GetOutOfJailCards.Enqueue(card);
                load = false;
                PushMessage(p.name + " collected a \" free out of jail card\"");
                break;
            case 4:
                p.Wait("Send3", 0, 1.5f);
                PushMessage(p.name + " sent 3 steps back");
                break;
            case 5:
                p.Wait("Jail", 0,1.5f);
                PushMessage(p.name + " sent to jail via "+type + " card");
                break;
            case 6:
                money = 0;
                count = p.CountHousesAndHotels();
                money += count.x * 25 + count.y * 100;
                p.Pay(money);
                Debug.Log(p.name + " payed " + money + " due " + type + " card");//update
                PushMessage(p.name + " payed " + money + " due " + type + " card");
                break;
            case 7:
                foreach (GameObject go in players)
                {
                    Player player = go.GetComponent<Player>();
                    if (player.active&&player!=p)
                    {
                        player.AddMoney(p.Pay((int)card.money));
                        PushMessage(p.name + " payed " + card.money + " to " + player.name);
                    }
                }
                break;
            case 8:
                money = 0;
                count = p.CountHousesAndHotels();
                money += count.x * 40 + count.y * 115;
                p.Pay(money);
                PushMessage(p.name + " payed " + money + " via " + type + " card");
                break;
            case 9:
                p.Pay((int)card.money);
                PushMessage(p.name + " payed " + card.money + " via " + type + " card");
                break;
            case 10:
                foreach (GameObject go in players)
                {
                    Player player = go.GetComponent<Player>();
                    if (player.active && player != p)
                    {
                        p.AddMoney(player.Pay((int)card.money));
                        PushMessage(player.name + " payed " + card.money + " to " + p.name);
                    }
                }
                break;
            default:
                break;
        }
        if (load)
            queue.Enqueue(card);
    }
    public void ShowCard(string s)//printing a message on the card block
    {
        GameObject card = GameObject.Find("Square_image");
        GameObject colorImage = GameObject.Find("Color_image");
        Text text = card.GetComponentInChildren<Text>();
        text.text = s;
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
    }//shuffles array (used for shuffling cards)
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
    }//sets the squares button to be clickable
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
    }//helps "SetButtons"
    public void Toss()//action of tossing dice inculding what happens after
    {
        audioSource.PlayOneShot(GameAssets.GetInstance().diceSound);
        Player cPlayer = players[(TurnIndex%players.Count)].GetComponent<Player>();
        int d1 = random.Next(1, 7);
        int d2 = random.Next(1, 7);
        Vector2Int dices = new Vector2Int(d1, d2);
        GameObject.Find("DiceText").GetComponent<DiceText>().ChangeText(dices);
        ShowGetOutOfJailButton(false);
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
                        PushMessage(cPlayer.name + " doubled 3 times in a row and sent to jail!");
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
    private Player GetCurrentPlayer()//returns the current player
    {
        return players[(TurnIndex % players.Count)].GetComponent<Player>();
    }
    public void PassTurn()//passing turn
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
                TurnDot();
                if (cPlayer.isBot)
                {
                    computer.Play();
                }
                else
                {
                    SetButtons();
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
                        int playerCount = 0;
                        Player p = new Player("winner");
                        foreach (GameObject go in players)
                        {
                            if (go.GetComponent<Player>().active)
                            {
                                playerCount++;
                                p = go.GetComponent<Player>();
                            }
                        }
                        if (playerCount == 1)
                        {
                            EndMatch();
                        }
                        TurnIndex++;
                    });
                    if (cPlayer.jailCount!=0)
                    {
                        
                        ShowGetOutOfJailButton(true);
                       
                        Button button = GameAssets.GetInstance().getOutOfJailButton.GetComponent<Button>();
                        button.onClick.RemoveAllListeners();
                        string message;
                        if (cPlayer.GetOutOfJailCards.Count==0)
                        {
                            message = "You can bail out of jail for\r\n$50";
                            button.onClick.AddListener(() =>
                            {
                                if (cPlayer.CanPay(50))
                                {
                                    cPlayer.BailFromJailUsingMoney();
                                    ShowGetOutOfJailButton(false);
                                    ShowCard("");
                                }
                            });
                        }
                        else
                        {
                            message = "You have \"Get Out Jail Card\"\r\nyou can use it if u like to";
                            button.onClick.AddListener(() =>
                            {
                                cPlayer.UseGetOutOfJailCard();
                                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
                                GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
                                GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener((Toss));
                                ShowGetOutOfJailButton(false);
                                ShowCard("");
                            });
                        }
                        ShowCard(message);
                        
                    }
                }
            }
        }
    }
    private void ShowGetOutOfJailButton(bool active)//disable or enabled the Get Out Of Jail dialog
    {
        GameAssets.GetInstance().getOutOfJailButton.SetActive(active);
        if (!active)
            ShowCard("");
        
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
    }//moving the dot that show who's tuen right now
    private void EndMatch()
    {
        foreach (GameObject go in players)
        {
            if (go.GetComponent<Player>().active)
            {
                PushMessage(go.GetComponent<Player>().name + " is The Winner!");
                ShowCard(go.GetComponent<Player>().name + "\r\nis The Winner!");
            }
        }
        
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
        GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.RemoveAllListeners();
    }//ending match
    private bool IsDoubleDice(Vector2Int dices)
    {
        return dices.y == dices.x;
    }// return if the dices are equal
    private void Move(Player player, Vector2Int dices)
    {
        player.Move(dices.x + dices.y);
    }// moves the player
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
    }//set the squares and streets info
}
