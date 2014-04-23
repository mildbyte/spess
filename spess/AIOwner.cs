﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using spess.ExchangeData;

namespace spess.AI
{

    public class AIOwner : Owner
    {
        private Dictionary<ProductionStation, Inventory> requiredInventory;
        private Dictionary<ProductionStation, List<BuyOrder>> outstandingBuyOrders;

        public AIOwner(Universe universe)
            : base(universe)
        {
            requiredInventory = new Dictionary<ProductionStation, Inventory>();
            outstandingBuyOrders = new Dictionary<ProductionStation, List<BuyOrder>>();
        }

        public override void NotifyMatch(Match match)
        {
            if (match.SellOrder.Owner == this) return; // Don't care about our goods being sold on the market

            // Find the station that placed the order
            BuyOrder bo = match.BuyOrder;
            ProductionStation client = null;

            foreach (KeyValuePair<ProductionStation, List<BuyOrder>> orders in outstandingBuyOrders)
            {
                if (orders.Value.Contains(bo))
                {
                    // If the matcher updated the order volume to 0, it's been completely matched.
                    if (bo.Volume == 0) orders.Value.Remove(bo);
                    client = orders.Key;
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
            closestShip.GoalQueue.AddGoal(
                new AI.MoveAndDepositGoods(closestShip, client, match.BuyOrder.Good, match.FillVolume, null));
        }

        public override void Update(float timePassed)
        {
            requiredInventory.Clear();

            // Get a list of goods that our stations require
            foreach (Sector s in Universe.Sectors)
            {
                foreach (SpaceBody b in s.Contents.OfType<ProductionStation>())
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
                    }
                }
            }

            // Subtract the list of goods that we already placed the orders for
            foreach (KeyValuePair<ProductionStation, List<BuyOrder>> orders in outstandingBuyOrders)
            {
                if (requiredInventory.ContainsKey(orders.Key))
                {
                    foreach (BuyOrder bo in orders.Value)
                    {
                        requiredInventory[orders.Key].RemoveItem(bo.Good, bo.Volume);
                    }
                }
            }

            // TODO: the ship's place buy order goal needs to return the order handle to us

            // * Pass it to the nearest supplier ship
            // * Batch orders somehow?
            // * How to prioritise exchanges?
            // * Owner has a pool of required items, supplier ships take things from this pool?
            // * Delivery contracts as a way for the player to make money?
        }
    }
}