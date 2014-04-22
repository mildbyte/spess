using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using spess.ExchangeData;

namespace spess
{
    public abstract class Owner
    {
        string name;
        List<ProductionStation> ownedStations;
        List<Ship> ownedShips;
        int accountBalance;
        Universe universe;

        public string Name { get { return name; } set { name = value; } }
        public int Balance { get { return accountBalance; } set { accountBalance = value; }}
        public List<Ship> Ships { get { return ownedShips; } }
        public List<ProductionStation> Stations { get { return ownedStations; } }
        public Universe Universe { get { return universe; } }

        public abstract void NotifyMatch(Match match);

        public Owner(Universe universe)
        {
            ownedStations = new List<ProductionStation>();
            ownedShips = new List<Ship>();
            this.universe = universe;
        }

        public abstract void Update(float timePassed);
    }

    public class PlayerOwner : Owner
    {
        public PlayerOwner(Universe universe) : base(universe) { }
        public override void NotifyMatch(Match match)
        {
            
        }

        public override void Update(float timePassed)
        {
            
        }
    }

    public class AIOwner : Owner
    {


        public AIOwner(Universe universe) : base(universe) { }

        public override void NotifyMatch(Match match)
        {
            // TODO: how would a ship know which station asked it to place the buy order?
            AIShip closestShip = Universe.GetClosestBodyBy(b => b is AIShip && (b as AIShip).Role == AIShipRole.Supplier,
                match.Exchange.Location, this) as AIShip;
            if (closestShip == null) return;
            
            closestShip.GoalQueue.AddGoal(
                new AI.MoveAndWithdrawGoods(closestShip, match.Exchange, match.BuyOrder.Good, match.FillVolume, null));
        }

        public override void Update(float timePassed)
        {
            // TODO:
            // * Get a list of goods that our stations require
            // * Subtract the list of goods that we placed the orders for
            // * Pass it to the nearest supplier ship
            // * A data structure holding which order belonged to which station?
            // * Batch orders somehow?
            // * How to prioritise exchanges?
            // * Owner has a pool of required items, supplier ships take things from this pool?
            // * Delivery contracts as a way for the player to make money?
        }
    }
}
