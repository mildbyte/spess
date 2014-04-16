using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess.ExchangeData
{
    public class Exchange : Building
    {
        Dictionary<Owner, Account> users;
        Dictionary<Good, OrderBook> orderBooks;
        Dictionary<Good, int> lastTradedPrices;

        public float MatchingInterval { get; set; }

        float timeSinceLastMatch;

        public Exchange(string name, Location location, Universe universe) : base(name, location, universe)
        {
            users = new Dictionary<Owner, Account>();
            orderBooks = new Dictionary<Good, OrderBook>();
            lastTradedPrices = new Dictionary<Good, int>();
            IconTexture = TextureProvider.exchangeTex;
            MatchingInterval = 1000.0f;
            timeSinceLastMatch = 0.0f;
        }

        public Dictionary<Good, int> LastTradedPrices { get { return lastTradedPrices; } }

        public void AddUser(Owner o)
        {
            if (!users.ContainsKey(o)) users[o] = new Account(o, this);
        }

        public bool HasUser(Owner o) { return users.ContainsKey(o); }

        public void DepositGoods(Ship s, Good good, int amount)
        {
            if (s.DockedStation != this) return;
            if (s.Cargo.GetItemCount(good) < amount) return;
            if (!HasUser(s.Owner)) return;

            s.Cargo.RemoveItem(good, amount);
            users[s.Owner].StoredGoods.AddItem(good, amount);
        }

        public void WithdrawGoods(Ship s, Good good, int amount)
        {
            if (s.DockedStation != this) return;
            if (!HasUser(s.Owner)) return;
            if (users[s.Owner].StoredGoods.GetItemCount(good) < amount) return;

            //TODO: what if the ship doesn't have any space?
            users[s.Owner].StoredGoods.RemoveItem(good, amount);
            s.Cargo.AddItem(good, amount);
        }

        public BuyOrder PlaceBuyOrder(Owner owner, Good good, int volume, int price, float timestamp)
        {
            if (owner.Balance < volume * price) return null;
            if (!HasUser(owner)) return null;

            BuyOrder buyOrder = new BuyOrder(owner, good, volume, price, timestamp);

            owner.Balance -= volume * price;
            users[owner].EscrowMoney += volume * price;

            if (!orderBooks.ContainsKey(good)) orderBooks[good] = new OrderBook(good);
            orderBooks[good].AddBuyOrder(buyOrder);

            return buyOrder;
        }

        public SellOrder PlaceSellOrder(Owner owner, Good good, int volume, int price, float timestamp)
        {
            if (!HasUser(owner)) return null;
            if (users[owner].StoredGoods.GetItemCount(good) < volume) return null;

            SellOrder sellOrder = new SellOrder(owner, good, volume, price, timestamp);

            users[owner].StoredGoods.RemoveItem(good, volume);
            users[owner].EscrowGoods.AddItem(good, volume);

            if (!orderBooks.ContainsKey(good)) orderBooks[good] = new OrderBook(good);
            orderBooks[good].AddSellOrder(sellOrder);

            return sellOrder;
        }

        public void PerformMatching()
        {
            foreach (OrderBook o in orderBooks.Values)
            {
                foreach (Match m in o.MatchOrders())
                {
                    users[m.BuyOrder.Owner].EscrowMoney -= m.FillVolume * m.BuyOrder.Price;
                    m.SellOrder.Owner.Balance += m.FillVolume * m.SellOrder.Price;

                    users[m.BuyOrder.Owner].StoredGoods.AddItem(m.BuyOrder.Good, m.FillVolume);
                    users[m.SellOrder.Owner].EscrowGoods.RemoveItem(m.SellOrder.Good, m.FillVolume);

                    lastTradedPrices[m.BuyOrder.Good] = m.BuyOrder.Price;
                }
            }
        }

        public override void Update(float timePassed)
        {
            timeSinceLastMatch += timeSinceLastMatch;
            if (timeSinceLastMatch > MatchingInterval)
            {
                timeSinceLastMatch = 0.0f;
                PerformMatching();
            }
        }
    }
}
