using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;
using UnityEngine.UI;

public class Player :MonoBehaviour
{
    
    [SerializeField]
    private string playerName;
    int id;
    [SerializeField]
    private int money;
    [SerializeField]
    private List<PropertySquare> props;
    //private Dictionary<int, List<PropertySquare>> props;
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
    public int getOutOfJailCount;
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
        getOutOfJailCount = 0;
    }
    public void Move(int steps)
    {
        pos += steps;
        if (pos > 39)
        {
            AddMoney(200);
            Debug.Log(name + " passed throw GO and recieved $200");//update
            isFirstRound = false;
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
            AddMoney(200);
            Debug.Log(name + " passed throw GO and recieved $200");//update
        }
        this.pos = pos;
        Allocate(pos);
        
    }
    public void SendThreeStepsBack()
    {
        this.pos -= 3;
        Allocate(pos);
    }
    public int GetMoney()
    {
        return money;
    }
    public void RemoveProperty(PropertySquare property)
    {
        props.Remove(property);
    }
    public void AddProperty(PropertySquare property)
    {
        props.Add(property);
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
    public int Pay(int money)
    {
        this.money -= money;
        ChangeMoney();
        Debug.Log(this.money);
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
    private IEnumerator Wait2(string type, int pos)
    {
        GameObject gameObject = GameObject.Find("DiceButton");
        gameObject.GetComponent<Button>().enabled = false;
        yield return new WaitForSeconds(1.5f);
        switch (type)
        {
            case "Jail":
                GoToJail();
                break;
            case "MoveTo":
                MoveTo(pos);
                break;
            default:
                SendThreeStepsBack();
                break;
        }
        gameObject.GetComponent<Button>().enabled = true;

    }
    public void Wait(string type, int pos)
    {
        StartCoroutine(Wait2( type, pos));
    }
    

}

