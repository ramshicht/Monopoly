using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isBot;
    [SerializeField]
    private string playerName;
    int id;
    [SerializeField]
    private int money;
    [SerializeField]
    public List<PropertySquare> props;
    [SerializeField]
    public int jailCount;
    [SerializeField]
    private int pos;
    [SerializeField]
    public int freeParkingCount;
    [SerializeField]
    public int doublesCount;
    public bool active;
    [SerializeField]
    public bool isFirstRound;
    public Dictionary<string, Street> streetsOwned;
    public Dictionary<string, List<PropertySquare>> propertysInStreet;
    public Queue<CardRoot> GetOutOfJailCards;
    public Player(string playerName)
    {
        this.playerName = playerName;
        money = 1500;
        props = new List<PropertySquare>();
        pos = 0;
        jailCount = 0;
        
    }
    public void Start()
    {
        //this.playerName = playerName;
        money = 1500;
        props = new List<PropertySquare>();
        pos = 0;
        active = true;
        isFirstRound = true;
        GetOutOfJailCards = new Queue<CardRoot>();
        streetsOwned =new Dictionary<string, Street>();
        propertysInStreet = new Dictionary<string, List<PropertySquare>>();
        string[] s = new string[] { "Utility", "Brown", "Green", "Cyan", "Blue", "Trains", "Red", "Pink", "Yellow", "Orange" };
        for (int i = 0; i < s.Length; i++)
        {
            propertysInStreet.Add(s[i], new List<PropertySquare>());
        }
    }
    public void Move(int steps)
    {
        pos += steps;
        if (pos > 39)
        {
            GameHandler.GetInstance().audioSource.PlayOneShot(GameAssets.GetInstance().kidsCheering);
            AddMoney(200);
            isFirstRound = false;
            GameHandler.GetInstance().PushMessage(name + " passed throw GO and recieved $200");
        }
        pos %= 40;
        Allocate(pos);
    }
    public void SetMoney(int money)
    {
        this.money = money;
        ChangeMoney();
    }
    public void MoveTo(int pos)
    {
        if (pos < this.pos)
        {
            isFirstRound=false;
            AddMoney(200);
            GameHandler.GetInstance().PushMessage(name + " passed throw GO and recieved $200");
            GameHandler.GetInstance().audioSource.PlayOneShot(GameAssets.GetInstance().kidsCheering);
        }
        this.pos = pos;
        Allocate(pos);
        
    }
    public void SendThreeStepsBack()
    {
        pos -= 3;
        Allocate(pos);
    }
    public int GetMoney()
    {
        return money;
    }
    public void RemoveProperty(PropertySquare property)
    {
        props.Remove(property);
        propertysInStreet[property.GetStreet().GetColor()].Remove(property);
    }
    public void AddProperty(PropertySquare property)
    {
        props.Add(property);
        if (property.GetStreet().IsOwnedByOnePlayer(this))
            streetsOwned.Add(property.GetStreet().GetColor(), property.GetStreet());
        propertysInStreet[property.GetStreet().GetColor()].Add(property);
    }
    public void AddMoney(int money)
    {
        this.money += money;
        ChangeMoney();
    }
    public bool CanPay(int debt)
    {
        if (money - debt >= 0)
            return true;
        return false;
    }
    public int Pay(int money)// player pays 
    {
        GameHandler.GetInstance().audioSource.PlayOneShot(GameAssets.GetInstance().cashSound);
        this.money -= money;
        ChangeMoney();
        return money;
    }
    public int GetPos()
    {
        return pos;
    }
    public void Bankrupt()
    {
        foreach (PropertySquare square in this.props)
        {
            square.Mortgage();
        }
        SetMoney(-1);
        ChangeMoney();
        active = false;
        int pCount = 0;
        Player p = null;
        foreach (GameObject go in GameHandler.GetInstance().players)
        {
            if (go.GetComponent<Player>().active)
            {
                p = go.GetComponent<Player>();
                pCount++;
            }
        }
        if (pCount==1)
            GameHandler.GetInstance().ShowCard(p.name + "\r\nis The Winner!");
        GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
        GameAssets.GetInstance().bankruptButton.GetComponent<Button>().onClick.RemoveAllListeners();


    }
    private void ChangeMoney()
    {
        List<Player> list = new List<Player>();
        int index=1;
        foreach (GameObject go in GameHandler.GetInstance().players)
        {
            if (go.GetComponent<Player>() == this)
                break;
            index++;
        }
        Text money = GameObject.Find("money" + index.ToString()).GetComponent<Text>();
        money.text = this.money.ToString();
        if (this.money <= 0)
            money.color = Color.red;
        else
            money.color = Color.black;
    }
    private void Allocate(int i)
    {
        
        float y = 1.5F;
        Vector3 vector = new Vector3(4.2F, y, -4.2F);
        if (i == 0)
        {
            transform.position = vector;
        }
        else
        {
            if (i < 10)
            {
                transform.position = new Vector3(1.6F - (1.9F * (i - 1)),y,-4.2f);
            }
            else
            {
                if (i == 10)
                {
                    transform.position = new Vector3(-17f, y, -5.3f);
                }
                else
                {
                    if (i < 20)
                    {
                        transform.position = new Vector3(-16.5f, y, (float)(-1.7f + (float)((i%10 - 1) * 1.9)));
                    }
                    else
                    {
                        if (i == 20)
                        {
                            transform.position = new Vector3(-16f, y, 16f);
                        }
                        else
                        {
                            if (i < 30)
                            {
                                transform.position = new Vector3(-13.7f + (float)((i%10-1)*2f), y, 16f);
                            }
                            else
                            {
                                if (i == 30)
                                {
                                    transform.position = new Vector3(5f, y, 15f);
                                   
                                }
                                else
                                {
                                    transform.position = new Vector3(4.5f, y, (float)(14f - (float)((i % 10 - 1) * 1.9)));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void Jail()
    {
        GoToJail();
    }
    public void GoToJail()
    {
        GameHandler.GetInstance().audioSource.PlayOneShot(GameAssets.GetInstance().policeSound);
        transform.position = new Vector3(-15f,1.5f, -4f);
        jailCount = 3;
        pos = 10;
        Debug.Log(playerName + " sent to jail!");//update;
    }
    public Vector2Int CountHousesAndHotels()
    {
        Vector2Int count = new Vector2Int();
        foreach (PropertySquare ps in props)
        {
            if (ps.GetNumOfBuildings() == 5)
                count.y++;
            else
                count.x += ps.GetNumOfBuildings();
        }
        return count;
    }
    public void MoveToWithOutCollect(int pos)
    {
        this.pos = pos;
        Allocate(pos);
    }
    private IEnumerator Wait2(string action, int pos, float time)
    {
        GameObject gameObject = GameObject.Find("DiceButton");
        gameObject.GetComponent<Button>().enabled = false;
        GameHandler gameHandler = GameHandler.GetInstance();
        Square[] squareInfo = gameHandler.squareInfo;
        yield return new WaitForSeconds(time);
        switch (action)
        {
            case "Send3":
                SendThreeStepsBack();
                squareInfo[this.pos].SteppedOn(this);
                break;
            case "Jail":
                GoToJail();
                break;
            case "MoveTo":
                MoveTo(pos);
                squareInfo[pos].SteppedOn(this);
                MoveTo(pos);
                break;
            case "MoveToRail":
                MoveToWithOutCollect(pos);
                squareInfo[pos].SteppedOn(this);
                squareInfo[pos].SteppedOn(this);
                break;
            default:
                break;
        }
        gameObject.GetComponent<Button>().enabled = true;
    }
    public void Wait(string type, int pos, float time)
    {
        
        StartCoroutine(Wait2( type, pos, time));
    }
    public void BailFromJailUsingMoney()
    {
        if (CanPay(50))
        {
            Pay(50);
            GameHandler.GetInstance().PushMessage(name + " bailed out of jail for $50");
            jailCount = 0;
            GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.RemoveAllListeners();
            GameAssets.GetInstance().diceButton.GetComponentInChildren<Text>().text = "Toss";
            GameAssets.GetInstance().diceButton.GetComponent<Button>().onClick.AddListener(() => { GameHandler.GetInstance().Toss(); });
        }
    }//pays 50 to bail out of jail
    public void UseGetOutOfJailCard()//uses "Get Out Of Jail Card"
    {
        GameHandler.GetInstance().PushMessage(name + " \"use Get Out Of Jail Card\"");
        jailCount = 0;
        CardRoot card = GetOutOfJailCards.Dequeue();
        if (card.message[1] == 'h')
            GameHandler.GetInstance().chanceCards.Enqueue(card);
        else
            GameHandler.GetInstance().communityChestCards.Enqueue(card);
    }
    
   
     

}

