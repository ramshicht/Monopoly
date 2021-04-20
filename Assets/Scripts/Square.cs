using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    //public class CardHandler
    //{
    //    public void DrawCard(Queue<CardRoot> queue, Player p)
    //    {
    //        CardRoot card = queue.Dequeue();
    //        Square[] squares = GameHandler.GetInstance().squareInfo;
    //        switch (card.kind)
    //        {
    //            case 0:
    //                p.Move((p.GetPos()+(int)card.pos)%40);
    //                squares[p.GetPos()].SteppedOn(p);
    //                if (p.GetPos() == 0)
    //                    p.SetMoney(p.GetMoney() - 200);
    //                break;
    //            case 1:
    //                break;
    //            case 2:
    //                break;
    //            case 3:
    //                break;
    //            case 4:
    //                break;
    //            case 5:
    //                break;
    //            case 6:
    //                break;
    //            case 7:
    //                break;
    //            case 8:
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //}
   
    public enum Action
    {
        START_SQUARE,//0
        CHANCE_SQUARE, //1
        COMMUNITY_BOX_SQUARE,//2
        JAIL_SQUARE,//3
        FREE_PARKING_SQUARE,//4
        GOTO_JAIL_SQUARE,//5
        INCOME_TAX_SQUARE,//6
        LUXURY_TAX_SQUARE//7
    }
    public abstract class Square
    {
       
        protected int id;
        protected string squareName;
        public Square(int id, string squareName)
        {
            this.id = id;
            this.squareName = squareName;
        }
        public abstract void SteppedOn(Player player);
        public string GetSquareName()
        {
            return squareName;
        }
        public int GetID()
        {
            return id;
        }
        public override string ToString()
        {
            return  squareName;
        }
        public abstract void ShowCard();

    }
    public class ActionSquare : Square
    {
        private Action action;
        public ActionSquare(int id, string SquareName, Action action) : base(id, SquareName)
        {
            this.action = action;
        }
        public override void SteppedOn(Player player)
        {
            ShowCard();
            switch (action)
            {
                case Action.START_SQUARE:
                    player.AddMoney(200);
                    
                    break;
                case Action.CHANCE_SQUARE:
                    GameHandler.GetInstance().DrawCard(true,player);
                    break;
                case Action.COMMUNITY_BOX_SQUARE:
                    //player.Wait(3);
                    GameHandler.GetInstance().DrawCard(false, player);
                    break;
                case Action.JAIL_SQUARE:
                    //Blank
                    break;
                case Action.FREE_PARKING_SQUARE:
                    player.freeParkingCount = 3;
                    break;
                case Action.GOTO_JAIL_SQUARE:
                    player.Wait("Jail",0);
                    break;
                case Action.INCOME_TAX_SQUARE:
                    player.Pay(200);
                    break;
                case Action.LUXURY_TAX_SQUARE:
                    player.Pay(100);
                    break;
                default:
                    break;
            }
           
        }
        public override string ToString()
        {
            return this.squareName;
        }
        public override void ShowCard()
        {
            GameObject card = GameObject.Find("Square_image");
            GameObject colorImage = GameObject.Find("Color_image");
            Text text = card.GetComponentInChildren<Text>();
            text.text = this.ToString();
            Image image = colorImage.GetComponent<Image>();
            Color color = image.color;
            Color color1;
            ColorUtility.TryParseHtmlString("#AEAEAEFF", out color1);
            image.color = color1;
        }


    }
    
    public class PropertySquare : Square
    {
        protected Player owner;
        protected int cost;
        private int numOfBuildings;
        protected int mortgageCost;
        protected Street street;
        private int[] rentPerBuildingsNum;
        GameObject house;
        public override string ToString()
        {
            string str = base.ToString() + "\r\n" + "Base rent: " + rentByBuildingNum(0).ToString() + "\r\n";
            for (int i = 1; i <= 4; i++)
            {
                str += i + " house rent: " + rentByBuildingNum(i) + "\r\n";
            }
            str += "Hotel (5 houses) rent: " + rentByBuildingNum(5) + "\r\n";
            str += "House Cost: " + GetBuildingCost()+ "\r\n";
            str += "Square cost: " + this.cost.ToString();
            str += "\r\n" + "Mortage: " + mortgageCost;
            if (owner != null)
            {
                str += "\r\n\r\n" + "Owner: ";
                str += owner.name;
            }
            
            return str;
        }
        public PropertySquare(int id, string squareName, int cost, int mortgageCost, Street s, List<int> rentPerBuildingsNum) : base(id,squareName)
        {
            owner = null;
            this.cost = cost;
            this.numOfBuildings = 0;
            this.mortgageCost = mortgageCost;
            this.street = s;
            this.rentPerBuildingsNum = new int[6];
            for (int i = 0; i < 6; i++)
            {
                this.rentPerBuildingsNum[i] = rentPerBuildingsNum[i];
            }
        }
        public PropertySquare( int id,string squareName, Street s) : base(id, squareName)
        {
            owner = null;
            this.street = s;
        }
        public override void ShowCard()
        {
            GameObject card = GameObject.Find("Square_image");
            GameObject colorImage = GameObject.Find("Color_image");
            Text text = card.GetComponentInChildren<Text>();
            text.text = this.ToString();
            Image image = colorImage.GetComponent<Image>();
            Color color = image.color;
            Color color1=Color.black;
            switch (street.GetColor())
            {
               case "Pink":
                        ColorUtility.TryParseHtmlString("#FF00FFFF", out color1);
                        break;
                case "Brown":
                        ColorUtility.TryParseHtmlString("#654321", out color1);
                        break;
                case "Utility":
                    ColorUtility.TryParseHtmlString("#AEAEAEFF", out color1);
                    break;
                case "Trains":
                    ColorUtility.TryParseHtmlString("#AEAEAEFF", out color1);
                    break;
                default:
                        ColorUtility.TryParseHtmlString(street.GetColor(), out color1);
                        break;
            }
            image.color = color1 ;
           
        }
        public void SetCost(int cost)
        {
            this.cost = cost;
        }
        protected void SetMortgage(int mortgage)
        {
            this.mortgageCost = mortgage;
        }
        public bool IsOwned()
        {
            if (owner == null)
                return false;
            return true;
        }
        public Player GetOwner()
        {
            return owner;
        }
        public int GetCost()
        {
            return cost;
        }
        public Street GetStreet()
        {
            return street;
        }
        private void ChangeOwner(Player player)
        {
            this.owner = player;
            player.AddProperty(this);
        }
        public int GetNumOfBuildings()
        {
            return this.numOfBuildings;
        }
        public void Mortgage()
        {
            owner.RemoveProperty(this);
            owner.AddMoney(mortgageCost+numOfBuildings/2);
            SetNumOfBuildings(0);
            this.owner = null;
            if (house != null)
                GameObject.Destroy(house);
            ShowCard();
        }
        public void SetNumOfBuildings(int num)
        {
            numOfBuildings = num;
        }
        public bool Buy(Player player)
        {
            if (player.CanPay(cost))
            {
                ChangeOwner(player);
                player.Pay(cost);
                return true;
            }
            return false;
        }
        public int RentCost()
        {
            if (street.IsOwnedByOnePlayer(owner))
            {
                if (street.HaveHouse())
                    return rentPerBuildingsNum[numOfBuildings];
                else
                    return rentPerBuildingsNum[0] * 2;
            }
            return rentPerBuildingsNum[0];
        }
        public int rentByBuildingNum(int i)
        {
            return rentPerBuildingsNum[i];
        }
       
        public override void SteppedOn(Player player)
        {   
            ChangeCurrentSquare(player);
            if ( owner != null && this.owner!=player)
            {
                owner.AddMoney(player.Pay(this.RentCost()));
                Debug.Log(player.name + "payed " + this.RentCost() + " to " + this.owner.name);//update
            }
        }
        public void ChangeCurrentSquare(Player player)
        {
            ShowCard();
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().enabled = true;
            GameAssets.GetInstance().buySquareButton.GetComponent<Button>().enabled = true;
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
            GameAssets.GetInstance().buySquareButton.GetComponent<Button>().onClick.RemoveAllListeners();
            GameAssets.GetInstance().mortageButton.GetComponent<Button>().onClick.RemoveAllListeners();
            if (player.GetPos()==this.id)
            {
                if (owner == null)
                {
                    if (player.isFirstRound)
                    {
                        GameAssets.GetInstance().buySquareButton.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            Debug.Log("You cant buy at your first round");//PopUp
                        });
                    }
                    else
                    {
                        GameAssets.GetInstance().buySquareButton.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            if (player.CanPay(this.cost))
                            {
                                this.owner = player;
                                player.AddProperty(this);
                                player.Pay(this.cost);
                                //ShowCard();
                                Debug.Log(player.name + " bought " + this.squareName);//message
                                ChangeCurrentSquare(player);
                            }   
                            else
                            {
                                Debug.Log("Not Have enough money"); //popupmessage
                            }
                        });
                    }
                }
                else
                {
                    GameAssets.GetInstance().buySquareButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log("sqaure already owned");//popup
                    });
                }
            }
            else
            {
                GameAssets.GetInstance().buySquareButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("not your current postion");//popup
                });
            }
            if (this.street.IsOwnedByOnePlayer(player))
            {
                GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (this.numOfBuildings!=5)
                    {
                        if (player.CanPay(this.GetBuildingCost()))
                        {
                            BuildNewHouse();
                            if (numOfBuildings!=5)
                                Debug.Log(player.name + " built house in " + this.squareName);//message
                            else
                                Debug.Log(player.name + " built hotel in " + this.squareName);//message
                        }
                    }
                    else
                    {
                        Debug.Log("Already have Hotel in this sqaure");//popup
                    }
                    
                });
            }
            else
            {
                GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("not whole street is owned by you");//popup
                });
            }
            if (this.owner==player)
            {
                GameAssets.GetInstance().mortageButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Mortgage();
                    Debug.Log(player.name + " mortaged " + this.squareName);//message
                    ChangeCurrentSquare(player);
                });
            }
            else
            {
                GameAssets.GetInstance().mortageButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("You cant moratge a square you dont own");//popup
                });
            }
        }
        private int GetBuildingCost()
        {
            return street.GetBuildingCost();
        }
        private void BuildNewHouse()
        {
            this.numOfBuildings++;
            owner.Pay(this.GetBuildingCost());
            BuildHouse(numOfBuildings-1);
        }
        private void BuildHouse(int houseIndex)
        {
            if (house != null)
                GameObject.Destroy(house);
            house =  GameObject.Instantiate(GameAssets.GetInstance().houses[houseIndex]);
            Transform transform = house.transform;
            float y = 1.5F;
            int i = this.id;
            if (i == 0)
            { }
            else
            {
                if (i < 10)
                {
                    //transform.position = vector + new Vector3( -1.9F * i,0);
                    transform.position = new Vector3(1.62F - (1.9F * (i - 1)), y, -2.9f);
                }
                else
                {
                    if (i == 10)
                    {
                       
                    }
                    else
                    {
                        if (i < 20)
                        {
                            transform.position = new Vector3(-15f, y, (float)(-1.7f + (float)((i % 10 - 1) * 1.9)));
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
                                    transform.position = new Vector3(-13.7f + (float)((i % 10 - 1) * 2f), y, 15.2f);
                                }
                                else
                                {
                                    if (i == 30)
                                    {
                                        transform.position = new Vector3(5f, y, 15f);

                                    }
                                    else
                                    {
                                        transform.position = new Vector3(3.25f, y, (float)(14f - (float)((i % 10 - 1) * 1.9)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }

    public class TrainSquare : PropertySquare
    {
        public TrainSquare(int id, string squareName, Street s) : base(id, squareName, s)
        {
            base.SetCost(200);
            base.SetMortgage(100);
        }
        public new int RentCost()
        {
            
            int times = base.GetStreet().NumberTimesOwnedByOnePlayer(base.GetOwner());
            switch (times)
            {
                case 1:
                    return 25;
                case 2:
                    return 50;
                case 3:
                    return 100;
                default:
                    return 200;
            }
        }
        public override string ToString()
        {
            string str = squareName;
            str += " \r\n Rent: $25 \r\n" +
                "if 2 R.R's are owned $50" +"\r\n"
                +"if 3  ''        ''         ''      $100 "+"\r\n" +
                 "if 4   ''        ''        ''      $200"+"\r\n";
            str += "Square cost: " + this.cost.ToString();
            str += "\r\n" + "Mortage: " + mortgageCost;

            if (owner != null)
            {
                str += "\r\n\r\n" + "Owner: ";
                str += owner.name;
            }
            return str;
        }
        public override void SteppedOn(Player player)
        {

            base.ChangeCurrentSquare(player);
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().enabled = false;
            if (this.owner != null && this.owner.name != player.name)
            {
                player.Pay(this.RentCost());
                Debug.Log(player.name + " payed " + this.RentCost() + " to " + this.owner.name);//message
            }
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("You cant buy house in Utility Squares");
            });

        }


    }

    public class UtilitySquare: PropertySquare
    {
        public UtilitySquare(int id, string squareName, Street s) : base(id, squareName, s)
        {
            base.SetCost(150);
            base.SetMortgage(75);
        }
        public int RentCost(int dice)
        {
            if (GetStreet().IsOwnedByOnePlayer(GetOwner()))
                return 10 * dice;
            return 4 * dice;
        }
        public override void SteppedOn(Player player)
        {
            base.ChangeCurrentSquare(player);
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
            GameAssets.GetInstance().buildHouseButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("You cant buy house in RailRoads");
            });
            if (this.owner!=null&&this.owner.name!=player.name)
            {
                Vector2Int dices= GameAssets.GetInstance().diceText.GetComponent<DiceText>().dices;
                player.Pay(this.RentCost(dices.x+dices.y));
                Debug.Log(player.name + " payed " + RentCost(dices.x+dices.y).ToString() + " to " + this.owner.name);//message
            }
        }
        public override string ToString()
        {
            string str = squareName;
            str += "\r\n if one \"Utility\" is owned \r\n" +
                "rent is 4 times amount shown on dice \r\n" +
                "if both \"Utility\" are owned"+"\r\n" +
                "rent is 10 times amount shown on dice";
            str +="\r\n" + "Square cost: " + this.cost.ToString();
            str += "\r\n" + "Mortage: " + mortgageCost;
            if (owner != null)
            {
                str += "\r\n\r\n" + "Owner: ";
                str += owner.name;
            }
            return str;
        }
    }

    public class Street
    {
        List<PropertySquare> props;
        string streetColor;
        int buildingCost;
        public Street(string streetColor, int buildingCost)
        {
            props = new List<PropertySquare>();
            this.streetColor = streetColor;
            this.buildingCost = buildingCost;
        }
        public Street(string streetColor)
        {
            props = new List<PropertySquare>();
            this.streetColor = streetColor;
            this.buildingCost = 0;
        }
        public List<PropertySquare> GetProps()
        {
            return props;
        }
        public int NumberTimesOwnedByOnePlayer(Player player)
        {
            int count = 0;
            foreach (PropertySquare ps in props)
            {
                if (ps.GetOwner() == player)
                    count++;
            }
            return count;
        }
        public bool IsOwnedByOnePlayer(Player player)
        {
            return NumberTimesOwnedByOnePlayer(player) == props.Count;
        }
        public bool HaveHouse()
        {
            foreach (PropertySquare square in this.props)
            {
                if (square.GetNumOfBuildings() != 0)
                    return true;
            }
            return false;
        }
        public void AddPropertySquare(PropertySquare ps)
        {
            props.Add(ps);
        }
        public int GetBuildingCost()
        {
            return buildingCost;
        }
        public string GetColor()
        {
            return this.streetColor;
        }
    }
}



   

