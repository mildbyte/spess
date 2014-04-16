using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.ExchangeData
{
    class Match
    {
        BuyOrder buyOrder;
        SellOrder sellOrder;
        int fillVolume;

        public BuyOrder BuyOrder { get { return buyOrder; } }
        public SellOrder SellOrder { get { return sellOrder; } }
        public int FillVolume { get { return fillVolume; } }

        public Match(BuyOrder buyOrder, SellOrder sellOrder, int fillVolume)
        {
            this.buyOrder = buyOrder; this.sellOrder = sellOrder;
            this.fillVolume = fillVolume;
        }
    }

    public class Order
    {
        Good good;
        int volume;
        int price;
        float timestamp;
        Owner owner;

        public Good Good { get { return good; } }
        public int Volume { get { return volume; } set { volume = value; } }
        public int Price { get { return price; } set { price = value; } }
        public float Timestamp { get { return timestamp; } }
        public Owner Owner { get { return owner; } }

        public Order(Owner owner, Good good, int volume, int price, float timestamp)
        {
            this.good = good; this.volume = volume;
            this.price = price; this.timestamp = timestamp;
            this.owner = owner;
        }
    }

    public class BuyOrder : Order, IComparable
    {
        public BuyOrder(Owner owner, Good good, int volume, int price, float timestamp) : base(owner, good, volume, price, timestamp) { }

        public int CompareTo(object obj)
        {
            BuyOrder o2 = obj as BuyOrder;

            int priceC = Price.CompareTo(o2.Price);

            if (priceC == 0) return Timestamp.CompareTo(o2.Timestamp);
            else return -priceC;
        }
    }

    public class SellOrder : Order
    {
        public SellOrder(Owner owner, Good good, int volume, int price, float timestamp) : base(owner, good, volume, price, timestamp) { }

        public int CompareTo(object obj)
        {
            SellOrder o2 = obj as SellOrder;

            int priceC = Price.CompareTo(o2.Price);

            if (priceC == 0) return Timestamp.CompareTo(o2.Timestamp);
            else return priceC;
        }
    }
}
