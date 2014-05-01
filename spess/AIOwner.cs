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
        // List of orders that we placed to cover our requirements and all items
        // that these orders imply (can't use the order volume since it can change)
        private Dictionary<ProductionStation, Tuple<List<BuyOrder>, Inventory>> outstandingBuyOrders;

        public AIOwner(Universe universe)
            : base(universe)
        {
            outstandingBuyOrders = new Dictionary<ProductionStation, Tuple<List<BuyOrder>, Inventory>>();
        }

        // TODO: station now doesn't keep placing buy orders, but the ship only makes one trip after matching
        public override void NotifyMatch(Match match)
        {
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
            AIShip closestShip = Universe.GetClosestBodyBy(b => b is AIShip && (b as AIShip).Role == AIShipRole.Supplier,
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

            // TODO: delegate triggered twice on the first arrival of the ship, removing both
            // expected items from the list (should be only one)
            depositCompleted = delegate(IGoal g) {
                // Dirty hack: need to notify about the end of the overall goal instead
                // (exposes internals of the goal system and doesn't work if the ship visits several
                // stations before the target one)
                if (!(g is DepositGoods)) return;
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
                        AIShip closestShip = Universe.GetClosestBodyBy(sb => sb is AIShip && (sb as AIShip).Role == AIShipRole.Supplier,
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
