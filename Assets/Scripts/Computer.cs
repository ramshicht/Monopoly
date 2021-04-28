using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class Computer
    {
        List<Player> otherPlayers;
        Player player;
        GameHandler gameHandler;
        string[] streetsByImportantOrder;
        Dictionary<string, Vector2Int> MinAndMaxHousesToBuy;
        public Computer(Player player)
        {
            otherPlayers = new List<Player>();
            this.player = player;
            this.gameHandler = GameHandler.GetInstance();
            foreach (GameObject go in gameHandler.players)
            {
                Player other = go.GetComponent<Player>();
                if (other != player)
                    otherPlayers.Add(other);
            }
            streetsByImportantOrder = new string[]{"Utility", "Brown", "Green", "Cyan", "Blue", "Trains", "Red", "Pink", "Yellow", "Orange"};
            SetMaxHousesToBuy();
        }
        private void SetMaxHousesToBuy()
        {
            MinAndMaxHousesToBuy = new Dictionary<string, Vector2Int>();
            for (int i = 0; i < streetsByImportantOrder.Length; i++)
            {
                switch (streetsByImportantOrder[i])
                {
                    case "Brown":
                        MinAndMaxHousesToBuy.Add(streetsByImportantOrder[i], new Vector2Int(5, 5));
                        break;
                    case "Cyan": case "Orange":
                        MinAndMaxHousesToBuy.Add(streetsByImportantOrder[i], new Vector2Int(3, 5));
                        break;
                    case "Pink":
                        MinAndMaxHousesToBuy.Add(streetsByImportantOrder[i], new Vector2Int(3, 4));
                        break;
                    case "Utility": case "Railroads":
                        MinAndMaxHousesToBuy.Add(streetsByImportantOrder[i], new Vector2Int());
                        break;
                    default:
                        MinAndMaxHousesToBuy.Add(streetsByImportantOrder[i], new Vector2Int(3, 3));
                        break;
                }
            }
        }
        public void Play()
        {

            if (player.jailCount>0)
            {
                
            }
            if (player.GetMoney() < 0)
                MortageToMoney(0);
            gameHandler.Toss();
            int pos = player.GetPos();
            DiceText diceText = GameObject.Find("DiceText").GetComponent<DiceText>();
            Square square = gameHandler.squareInfo[pos];
            if (square.ToString().Length>20)
            {
                 if (ShouldBuy((PropertySquare)square))
                    ((PropertySquare)square).Buy(player);
            }
            if (player.streetsOwned.Keys.Count!=0&&player.jailCount==0)
            {
                BuyHouses();
            }
            gameHandler.PassTurn();
        }
        public bool ShouldBuy(PropertySquare square)
        {
            if (player.isFirstRound)
                return false;
            if (!square.IsOwned())
            {
                if (player.CanPay(square.GetCost()))
                {
                    Street street = square.GetStreet();
                    if (street.GetColor() == "Trains")
                        return true;
                    if (street.GetColor() == "Utility")
                        return false;
                    if (street.NoOwners())
                        return true;
                    if (street.IsOwnedBy(player))
                        return true;
                    if (street.NumberTimesOwnedByOnePlayer(player) == street.props.Count - 1)
                        return true;
                    if (street.IsOwnedBytwoPlayers())
                        return false;
                    foreach (Player p in otherPlayers)
                    {
                        if (street.IsOwnedBy(p))
                            return true;
                    }
                }
                return false;
            }
            return false;
            
        }
        private void BuyHouses()
        {
            Dictionary<string, Street> streetsOwned = player.streetsOwned;
            List<Street> StreetsToBackTo = new List<Street>();
           
            for (int i = streetsByImportantOrder.Length-1; i >=0 ; i--)
            {
                Debug.Log(player.streetsOwned.ContainsKey(streetsByImportantOrder[i])+" "+ streetsByImportantOrder[i]);
                if (streetsByImportantOrder[i]!="Trains"&& streetsByImportantOrder[i] != "Utility"&& player.streetsOwned.ContainsKey(streetsByImportantOrder[i]))
                {
                    foreach (PropertySquare square in player.streetsOwned[streetsByImportantOrder[i]].props)
                    {
                            if (square.GetNumOfBuildings()> MinAndMaxHousesToBuy[streetsOwned[streetsByImportantOrder[i]].GetColor()].x)
                            {
                                if (player.streetsOwned.Count >1)
                                {
                                    StreetsToBackTo.Add(player.streetsOwned[streetsByImportantOrder[i]]);
                                    break;
                                }
                                else
                                {
                                    for (int j = square.GetNumOfBuildings() ; j < MinAndMaxHousesToBuy[streetsOwned[streetsByImportantOrder[i]].GetColor()].y&&!WillBeDengerous(square.GetBuildingCost()); j++)
                                    {
                                        square.BuildNewHouse();
                                    }
                                }
                                
                            }
                            else
                            {
                                for (int j = square.GetNumOfBuildings(); j < MinAndMaxHousesToBuy[streetsOwned[streetsByImportantOrder[i]].GetColor()].x && !WillBeDengerous(square.GetBuildingCost()); j++)
                                {
                                    square.BuildNewHouse();
                                }
                            }
                    }
                }
            }
            foreach (Street street in StreetsToBackTo)
            {
                foreach (PropertySquare square in street.props)
                {
                    for (int j = square.GetNumOfBuildings(); j < MinAndMaxHousesToBuy[street.GetColor()].y && !WillBeDengerous(square.GetBuildingCost()); j++)
                    {
                        square.BuildNewHouse();
                    }
                }
            }
        }
        private bool ISinDanger()
        {
            bool inDanger = false;
            foreach (Player p in otherPlayers)
            {
                if (p.streetsOwned.Count != 0)
                    inDanger = true;
            }
            return inDanger && player.GetMoney() < 300;
        }
        private bool WillBeDengerous(int money)
        {
            return player.GetMoney() - money < 300;
        }
        private void MortageToMoney(int money)
        {
            int moneyNeeded = money - player.GetMoney();
            Street street;
            int scansCount = 0;
            bool Mortaged;
            while (player.GetMoney()<money&&player.props.Count!=0)
            {
                Mortaged = false;
                for (int i = 0; i < streetsByImportantOrder.Length&&!Mortaged&& player.GetMoney() < money; i++)
                {
                    foreach (PropertySquare square in player.propertysInStreet[streetsByImportantOrder[i]])
                    {
                        Debug.Log(square.GetSquareName() + " run " + scansCount);
                        if (scansCount<10)
                        {
                            if (DontNeed(square))
                            {
                                square.Mortgage();
                                Mortaged = true;
                                break;
                            }
                        }
                        else
                        {
                            if (scansCount<20)
                            {
                                if (!NeedThiSquare(square))
                                {
                                    square.Mortgage();
                                    Mortaged = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (scansCount < 30)
                                {
                                    if (!HaveTokeep(square))
                                    {
                                        square.Mortgage();
                                        Mortaged = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    square.Mortgage();
                                    Mortaged = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                scansCount++;
            }
            if (player.GetMoney()<0)
            {
                player.Bankrupt();
            }
        }
        private bool DontNeed(PropertySquare square)
        {
            Street street = square.GetStreet();
            if (street.IsOwnedBytwoPlayers()&&street.NumberTimesOwnedByOnePlayer(player) != street.props.Count - 1)
                return true;
            return false;
        }
        private bool ShouldGetOutOfJail()
        {
           // Dictionary<Street> streets 
            if (true)
            {

            }
            if (player.streetsOwned.Count>0)
            {
                
            }
            return false;
        }
        private bool NeedThiSquare(PropertySquare square)
        {
            Street street = square.GetStreet();
            if (street.NumberTimesOwnedByOnePlayer(player) == street.props.Count - 1)
                  return true;
            foreach (Player p in otherPlayers)
            {
                if (street.NumberTimesOwnedByOnePlayer(p) == street.props.Count - 1)
                    return true;
            }
            return false;
        }
        private bool HaveTokeep(PropertySquare square)
        {
            Street street = square.GetStreet();
            foreach (Player p in otherPlayers)
            {
                if (street.NumberTimesOwnedByOnePlayer(p) == street.props.Count - 1)
                    return true;
            }
            return false;
        }
        
    }
}
