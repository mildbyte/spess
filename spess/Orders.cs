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

    class Order
    {
        Good good;
        int volume;
        int price;
        int timestamp;
        Owner owner;

        public Good Good { get { return good; } }
        public int Volume { get { return volume; } set { volume = value; } }
        public int Price { get { return price; } set { price = value; } }
        public int Timestamp { get { return timestamp; } }
        public Owner Owner { get { return owner; } }

        public Order(Owner owner, Good good, int volume, int price, int timestamp)
        {
            this.good = good; this.volume = volume;
            this.price = price; this.timestamp = timestamp;
            this.owner = owner;
        }
    }

    class BuyOrder : Order
    {
        public BuyOrder(Owner owner, Good good, int volume, int price, int timestamp) : base(owner, good, volume, price, timestamp) { }
        public static bool operator <(BuyOrder o1, BuyOrder o2)
        {
            if (o1.Price > o2.Price) return true;
            return (o1.Timestamp < o2.Timestamp);
        }

        public static bool operator >(BuyOrder o1, BuyOrder o2)
        {
            if (o1.Price < o2.Price) return true;
            return (o1.Timestamp > o2.Timestamp);
        }
    }

    class SellOrder : Order
    {
        public SellOrder(Owner owner, Good good, int volume, int price, int timestamp) : base(owner, good, volume, price, timestamp) { }
        public static bool operator <(SellOrder o1, SellOrder o2) {
            if (o1.Price < o2.Price) return true;
            return (o1.Timestamp < o2.Timestamp);
        }
        public static bool operator >(SellOrder o1, SellOrder o2)
        {
            if (o1.Price > o2.Price) return true;
            return (o1.Timestamp > o2.Timestamp);
        }
    }
}
