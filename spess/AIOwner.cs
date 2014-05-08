using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using spess.ExchangeData;

namespace spess.AI
{
    public class AIOwner : Owner
    {
        delegate void OnMatch(Match m);

        // List of orders that we placed to cover our requirements and all items
        // that these orders imply (can't use the order volume since it can change)
        private Dictionary<ProductionStation, Tuple<List<BuyOrder>, Inventory>> outstandingBuyOrders;

        private OnMatch onMatch;

        public AIOwner(Universe universe)
            : base(universe)
        {
            outstandingBuyOrders = new Dictionary<ProductionStation, Tuple<List<BuyOrder>, Inventory>>();
        }

        public override void NotifyMatch(Match match)
        {
            // Call the relevant delegates (for the exchange arbitrageur ships)
            if (onMatch != null) onMatch(match);

            if (match.SellOrder.Owner == this) return; // Don't care about our goods being sold on the market

            // Find the station that placed the order
            BuyOrder bo = match.BuyOrder;
            ProductionStation client = null;
            List<BuyOrder> clientOrders = null;
            Inventory clientOrdered = null;

            foreach (KeyValuePair<ProductionStation, Tuple<List<BuyOrder>, Inventory>> orders in outstandingBuyOrders)
            {
                if (orders.Value.Item1.Contains(bo))
                {
                    client = orders.Key;
                    clientOrders = orders.Value.Item1;
                    clientOrdered = orders.Value.Item2;
                    break;
                }
            }
            if (client == null) return;

            // Find the closest ship to the exchange to take the goods away
            AIShip closestShip = Universe.GetClosestBodyBy(b => b.Owner == this && b is AIShip && (b as AIShip).Role == AIShipRole.Supplier,
                match.Exchange.Location, this) as AIShip;
            if (closestShip == null) return;

            // Order the ship to deposit the goods from the exchange to the station
            closestShip.GoalQueue.AddGoal(
                new AI.MoveAndWithdrawGoods(closestShip, match.Exchange, match.BuyOrder.Good, match.FillVolume, null));

            Goal depositGoal = new AI.MoveAndDepositGoods(closestShip, client, match.BuyOrder.Good, match.FillVolume, null);
            closestShip.GoalQueue.AddGoal(depositGoal);

            // Add a one-time action to the ship when the contents have been moved to the station:
            // remove the order from the list of outstanding orders that the station is expecting
            // and remove the goods that have arrived from the list of expected goods
            OrderCompleted depositCompleted = null;

            depositCompleted = delegate(IGoal g) {
                if (g != depositGoal) return;
                if (bo.Volume == 0) clientOrders.Remove(bo);
                clientOrdered.RemoveItem(bo.Good, match.FillVolume);
                closestShip.GoalQueue.OnOrderCompleted -= depositCompleted;
            };

            closestShip.GoalQueue.OnOrderCompleted += depositCompleted;
        }

        private void PlaceStationBuyOrders()
        {
            Dictionary<ProductionStation, Inventory> requiredInventory = new Dictionary<ProductionStation, Inventory>();

            // Get a list of goods that our stations require
            foreach (Sector s in Universe.Sectors)
            {
                foreach (SpaceBody b in s.Contents.OfType<ProductionStation>().Where(b => b.Owner == this))
                {
                    ProductionStation ps = b as ProductionStation;
                    if (!ps.CanProduce())
                    {
                        requiredInventory[ps] = new Inventory();

                        // Only order enough goods for one production cycle for now
                        foreach (KeyValuePair<Good, int> required in ps.Production.Input)
                        {
                            requiredInventory[ps].AddItem(required.Key, required.Value);
                        }

                        // Remove those items that we already have
                        requiredInventory[ps].SubtractInventory(ps.Inventory);
                    }
                }
            }

            // Subtract the list of goods that we already placed the orders for
            foreach (KeyValuePair<ProductionStation, Tuple<List<BuyOrder>, Inventory>> orders in outstandingBuyOrders)
            {
                if (requiredInventory.ContainsKey(orders.Key))
                {
                    requiredInventory[orders.Key].SubtractInventory(orders.Value.Item2);
                }
            }

            // We don't need a ship to place a buy order, just to collect it.
            foreach (KeyValuePair<ProductionStation, Inventory> required in requiredInventory)
            {
                // Use the closest exchange to buy goods from (alternative: cheapest/most liquid)
                Exchange closestExchange = Universe.GetClosestBodyBy(
                    b => b is Exchange, required.Key.Location, this) as Exchange;

                foreach (KeyValuePair<Good, int> good in required.Value)
                {
                    // TODO: pricing algorithms. Does the station aim to make a profit from its inputs?
                    BuyOrder bo = closestExchange.PlaceBuyOrder(this, good.Key, good.Value, 10);
                    if (bo == null) continue;

                    if (!outstandingBuyOrders.ContainsKey(required.Key))
                        outstandingBuyOrders[required.Key] = new Tuple<List<BuyOrder>, Inventory>(new List<BuyOrder>(), new Inventory());
                    outstandingBuyOrders[required.Key].Item1.Add(bo);
                    outstandingBuyOrders[required.Key].Item2.AddItem(bo.Good, bo.Volume);
                }
            }
        }

        private void PlaceStationSellOrders()
        {
            foreach (Sector s in Universe.Sectors)
            {
                foreach (SpaceBody b in s.Contents.OfType<ProductionStation>().Where(b => b.Owner == this))
                {
                    ProductionStation ps = b as ProductionStation;

                    // Storage space full, need to sell some of the stock
                    if (ps.OccupiedSpace() >= ps.StorageSpace)
                    {
                        // Get the closest ship to the station and the closest exchange to it
                        AIShip closestShip = Universe.GetClosestBodyBy(sb => sb is AIShip && sb.Owner == this && (sb as AIShip).Role == AIShipRole.Supplier,
                            ps.Location, this) as AIShip;
                        if (closestShip == null) return;
                        Exchange closestExchange = Universe.GetClosestBodyBy(
                            sb => sb is Exchange, ps.Location, this) as Exchange;

                        foreach (KeyValuePair<Good, int> output in ps.Production.Output)
                        {
                            // Withdraw the goods from the station
                            closestShip.GoalQueue.AddGoal(
                                new MoveAndWithdrawGoods(closestShip, ps, output.Key, ps.Inventory.GetItemCount(output.Key), null));
                            // Deposit them at the exchange
                            closestShip.GoalQueue.AddGoal(
                                new MoveAndDepositGoods(closestShip, closestExchange, output.Key, ps.Inventory.GetItemCount(output.Key), null));
                            // Sell the goods
                            // TODO: price?
                            closestShip.GoalQueue.AddGoal(
                                new MoveAndPlaceSellOrder(closestShip, closestExchange, output.Key, ps.Inventory.GetItemCount(output.Key), 10, null));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given 2 exchanges, calculates the largest profit we can have
        /// buying one type of good at the first one and selling it 
        /// at the second one (at most taking maxCargoSpace).
        /// <returns>Tuple of good, volume, buy price, sell price</returns>
        /// </summary>
        private Tuple<Good, int, int, int> PossibleProfit(Exchange e1, Exchange e2, int maxCargoSpace)
        {
            Tuple<Good, int, int, int> best = null;

            foreach (Good g in e1.OrderBooks.Keys)
            {
                if (!e2.OrderBooks.ContainsKey(g)) continue;
                BuyOrder bestBid = e2.OrderBooks[g].GetBestBid();
                SellOrder bestAsk = e1.OrderBooks[g].GetBestOffer();

                if (bestBid.Price < bestAsk.Price) continue; // Can't make a profit

                // The largest amount we can buy
                int volume = Math.Min(Math.Min(bestBid.Volume, bestAsk.Volume), maxCargoSpace / g.Size);

                // Update the best action tuple (buy at the ask, sell at the bid)
                if (best == null || (best.Item4 - best.Item3) * best.Item2 < volume * (bestBid.Price - bestAsk.Price))
                {
                    best = new Tuple<Good, int, int, int>(g, volume, bestAsk.Price, bestBid.Price);
                }
            }

            return best;
        }

        /// <summary>
        /// Assign buy-sell orders to our arbitrageur ships that don't have any orders
        /// </summary>
        private void PlaceArbitrageurOrders()
        {
            IEnumerable<AIShip> availableShips =
                Universe.GetAllSpaceBodies().Where(s => s.Owner == this && s is AIShip
                    && (s as AIShip).Role == AIShipRole.Arbitrageur 
                    && (s as AIShip).GoalQueue.IsEmpty()).Cast<AIShip>();

            IEnumerable<Exchange> exchanges =
                Universe.GetAllSpaceBodies().Where(b => b is Exchange).Cast<Exchange>();

            HashSet<Exchange> assignedExchanges = new HashSet<Exchange>();

            // Assign an arbitrage task to every ship
            foreach (AIShip ship in availableShips)
            {
                // Get the closest exchange to the ship
                Exchange closestExchange =
                    Universe.GetClosestBodyBy(b => b is Exchange && 
                        !assignedExchanges.Contains(b as Exchange), ship.Location, this) as Exchange;

                if (closestExchange == null) continue;

                // Get the most profitable run we can do with this exchange
                Exchange bestExchange = null;
                Tuple<Good, int, int, int> bestRun = null;

                foreach (Exchange e in exchanges.Where(e => e != closestExchange && !assignedExchanges.Contains(e)))
                {
                    Tuple<Good, int, int, int> currRun = PossibleProfit(bestExchange, e, ship.CargoSpace);
                    if (currRun == null) continue;

                    if (bestRun == null || (bestRun.Item4 - bestRun.Item3) * bestRun.Item2 < (currRun.Item4 - currRun.Item3) * currRun.Item2)
                    {
                        bestRun = currRun;
                        bestExchange = e;
                    }
                }

                if (bestExchange == null) continue;

                // Place the buy order and make the ship collect it when it is matched
                BuyOrder buyOrder = bestExchange.PlaceBuyOrder(this, bestRun.Item1, bestRun.Item2, bestRun.Item3);
                if (buyOrder == null) continue;

                OnMatch om = null;
                om = delegate(Match m)
                {
                    if (m.BuyOrder != buyOrder) return;
                    onMatch -= om;

                    ship.GoalQueue.AddGoal(new MoveAndWithdrawGoods(ship, closestExchange, bestRun.Item1, bestRun.Item2, null));
                    ship.GoalQueue.AddGoal(new MoveAndDepositGoods(ship, bestExchange, bestRun.Item1, bestRun.Item2, null));
                    ship.GoalQueue.AddGoal(new MoveAndPlaceSellOrder(ship, bestExchange, bestRun.Item1, bestRun.Item2, bestRun.Item4, null));
                };

                onMatch += om;

                // These exchanges are taken
                // TODO: bestExchange (the one with the sell order) may be used again
                // on the next round
                assignedExchanges.Add(bestExchange);
                assignedExchanges.Add(closestExchange);
            }

        }

        public override void Update(float timePassed)
        {
            PlaceStationBuyOrders();
            PlaceStationSellOrders();

            // * Batch orders somehow?
            // * How to prioritise exchanges?
            // * Owner has a pool of required items, supplier ships take things from this pool?
            // * Delivery contracts as a way for the player to make money?
        }
    }
}
