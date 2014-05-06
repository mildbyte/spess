using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.ExchangeData
{
    public class OrderBook
    {
        Good good;
        List<BuyOrder> bids;
        List<SellOrder> asks;
        Exchange exchange;

        public OrderBook(Good good, Exchange exchange) 
        { 
            this.good = good;
            this.exchange = exchange;
            bids = new List<BuyOrder>();
            asks = new List<SellOrder>();
        }

        public List<BuyOrder> Bids { get { return bids; } }
        public List<SellOrder> Offers { get { return asks; } }

        public BuyOrder GetBestBid()
        {
            return bids.Min();
        }

        public SellOrder GetBestOffer()
        {
            return asks.Min();
        }

        public void AddBuyOrder(BuyOrder o) { bids.Add(o); }
        public void AddSellOrder(SellOrder o) { asks.Add(o); }

        public IEnumerable<Match> MatchOrders()
        {
            bids.Sort();
            asks.Sort();

            while (bids.Any() && asks.Any() && bids[0].Price >= asks[0].Price)
            {
                BuyOrder currBid = bids[0];
                SellOrder currAsk = asks[0];

                int fillVolume = Math.Min(currAsk.Volume, currBid.Volume);

                currAsk.Volume -= fillVolume;
                currBid.Volume -= fillVolume;

                if (currBid.Volume == 0) bids.RemoveAt(0);
                if (currAsk.Volume == 0) asks.RemoveAt(0);

                yield return new Match(currBid, currAsk, fillVolume, exchange);
            }
        }

        public IEnumerable<BuyOrder> GetUserBuyOrders(Owner o)
        {
            return bids.Where(b => b.Owner == o);
        }

        public IEnumerable<SellOrder> GetUserSellOrders(Owner o)
        {
            return asks.Where(b => b.Owner == o);
        }
    }
}
