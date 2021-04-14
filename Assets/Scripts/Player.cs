using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;

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
    }
    public void Move(int steps)
    {
        Debug.Log("hello");
        pos += steps;
        if (pos > 39)
            AddMoney(200);
        pos %= 40;
        Allocate(pos);
        Debug.Log(transform.position);
    }
    public void MoveTo(int pos)
    {
        this.pos = pos;
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
        return money;
    }
    public void BuySomething(PropertySquare s, string caption)
    {
        switch (caption)
        {
            case "Property":
                if (CanPay(s.GetCost()))
                {
                    Pay(s.GetCost());
                    // s.ChangeOwner(this);
                }
                break;
            case "Building":
                if (CanPay(s.GetStreet().GetBuildingCost()))
                    Pay(s.GetCost());
                break;

            default:
                break;
        }
    }
    public int GetPos()
    {
        return pos;
    }
    private Pose GetLocation(int pos)
    {
        int line = pos / 10;
        int i = pos - line * 10;
        Pose pose = new Pose();
        Debug.Log(i);
        switch (line)
        {
            case 0:
                pose.position = new Vector3((float)(i * -1.5), (float)0.5, 0);
               // pose.rotation = new Quaternion((float)-90, 0, (float)-90, 0);
                break;
            default:
                pose.position = new Vector3((float)(0), (float)0.5, 0);
              //  pose.rotation = new Quaternion((float)-90, 0, (float)-90, 0);
                break;
        }
        return pose;
    }
    public void Bankrupt()
    {
        foreach (PropertySquare square in this.props)
        {
            square.Mortgage();
        }
        this.money = -1;
        active = false;

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
                //transform.position = vector + new Vector3( -1.9F * i,0);
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
    private void GoToJail()
    {
        //Wait(5);
        transform.position = new Vector3(-15f,1.5f, -4f);
        jailCount = 3;
        pos = 10;
    }
    private IEnumerable Wait(int secs)
    { 
        yield return new WaitForSeconds(secs);
    }

}

