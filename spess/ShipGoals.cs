using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using spess.ExchangeData;

namespace spess.AI
{
    abstract class ShipGoal : Goal
    {
        Ship ship;
        public Ship Ship { get { return ship; } }

        public ShipGoal(string name, Ship ship, ICompositeGoal creator)
            : base(name, creator)
        {
            this.ship = ship;
        }

    }

    class MoveToSector : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Sector sector;
        public Sector Sector { get { return sector; } }

        bool calculated = false;
        IEnumerable<Goal> calculatedSubgoals = null;

        public MoveToSector(Ship ship, Sector sector, ICompositeGoal creator)
            : base("Move to sector...", ship, creator)
        {
            this.sector = sector;
        }

        public bool Failed()
        {
            if (calculatedSubgoals == null && calculated) return true;
            else
            {
                GetSubgoals();
                return calculatedSubgoals == null;
            }
        }


        public IEnumerable<Goal> GetSubgoals()
        {
            if (calculated) return calculatedSubgoals;

            List<Gate> route = Ship.Universe.GetGateAwareRoute(Ship.Owner, Ship.Location.Sector, sector);
            if (route == null) calculatedSubgoals = null;
            else calculatedSubgoals = route.Select(g => new MoveAndUseGate(Ship, g, this));

            calculated = true;
            return calculatedSubgoals;
        }
    }

    class MoveInSector : ShipGoal, IBaseGoal
    {
        Vector3 position;
        public Vector3 Position { get { return position; } }

        public MoveInSector(Ship ship, Vector3 position, ICompositeGoal creator)
            : base("Move to position in sector...", ship, creator)
        {
            this.position = position;
        }

        public bool IsComplete() { return (Ship.Location.Coordinates - position).Length() < 1.0; }

        public void Execute()
        {
            // Scaled to the maximum speed by the Velocity setter
            Ship.Velocity = position - Ship.Location.Coordinates;
        }
    }

    class MoveTo : ShipGoal, ICompositeGoal
    {
        Location location;
        public Location Location { get { return location; } }

        public MoveTo(Ship ship, Location location, ICompositeGoal creator)
            : base("Move to...", ship, creator)
        {
            this.location = location;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new Undock(Ship, this);
            yield return new MoveToSector(Ship, location.Sector, this);
            yield return new MoveInSector(Ship, location.Coordinates, this);
        }
    }

    class Undock : ShipGoal, IBaseGoal
    {
        public Undock(Ship ship, ICompositeGoal creator) : base("Undock...", ship, creator) { }

        public bool IsComplete() { return Ship.DockedStation == null; }

        public void Execute()
        {
            Ship.Undock();
        }
    }

    class DockAt : ShipGoal, IBaseGoal, IFailableGoal
    {
        Building building;
        public Building Building { get { return building; } }

        public DockAt(Ship ship, Building building, ICompositeGoal creator)
            : base("Dock...", ship, creator)
        {
            this.building = building;
        }

        public bool IsComplete() { return Building == Ship.DockedStation; }

        public void Execute()
        {
            Ship.Velocity = Vector3.Zero;
            Ship.Dock(building);
        }

        public bool Failed()
        {
            return (!building.PermittedToDock(Ship));
        }
    }

    class MoveAndDockAt : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Building building;
        public Building Building { get { return building; } }

        public MoveAndDockAt(Ship ship, Building building, ICompositeGoal creator)
            : base("Move and dock at...", ship, creator)
        {
            this.building = building;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveTo(Ship, building.Location, this);
            yield return new DockAt(Ship, building, this);
        }

        public bool Failed()
        {
            return (!building.PermittedToDock(Ship));
        }
    }

    class UseGate : ShipGoal, IBaseGoal
    {
        Gate gate;
        public Gate Gate { get { return gate; } }

        public UseGate(Ship ship, Gate gate, ICompositeGoal creator)
            : base("Use gate...", ship, creator)
        {
            this.gate = gate;
        }

        public bool IsComplete() { return Ship.Location.Sector == Gate.Destination.Sector; }

        public void Execute()
        {
            Ship.UseGate(gate);
        }
    }

    class MoveAndUseGate : ShipGoal, ICompositeGoal
    {
        Gate gate;
        public Gate Gate { get { return gate; } }

        public MoveAndUseGate(Ship ship, Gate gate, ICompositeGoal creator)
            : base("Use gate...", ship, creator)
        {
            this.gate = gate;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveTo(Ship, Gate.Location, this);
            yield return new UseGate(Ship, Gate, this);
        }
    }

    class PlaceBuyOrder : ShipGoal, IBaseGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;
        int price;

        BuyOrder resultOrder = null;

        public PlaceBuyOrder(Ship ship, Exchange exchange, Good good, int volume, int price, ICompositeGoal creator)
            : base("Place buy order...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
            this.price = price;
        }

        public void Execute()
        {
            if (Ship.DockedStation != exchange) return;
            if (!exchange.HasUser(Ship.Owner)) exchange.AddUser(Ship.Owner);
            resultOrder = exchange.PlaceBuyOrder(Ship.Owner, good, volume, price, Ship.Universe.GameTime);
        }

        public bool IsComplete()
        {
            return resultOrder != null;
        }

        public bool Failed()
        {
            return Ship.Owner.Balance < price * volume;
        }
    }

    class MoveAndPlaceBuyOrder : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;
        int price;

        public MoveAndPlaceBuyOrder(Ship ship, Exchange exchange, Good good, int volume, int price, ICompositeGoal creator)
            : base("Move and place buy order...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
            this.price = price;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveAndDockAt(Ship, exchange, this);
            yield return new PlaceBuyOrder(Ship, exchange, good, volume, price, this);
        }

        public bool Failed()
        {
            return Ship.Owner.Balance < price * volume;
        }
    }

    class PlaceSellOrder : ShipGoal, IBaseGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;
        int price;

        SellOrder resultOrder = null;

        public PlaceSellOrder(Ship ship, Exchange exchange, Good good, int volume, int price, ICompositeGoal creator)
            : base("Place sell order...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
            this.price = price;
        }

        public void Execute()
        {
            if (Ship.DockedStation != exchange) return;
            if (!exchange.HasUser(Ship.Owner)) exchange.AddUser(Ship.Owner);
            resultOrder = exchange.PlaceSellOrder(Ship.Owner, good, volume, price, Ship.Universe.GameTime);
        }

        public bool IsComplete()
        {
            return resultOrder != null;
        }

        public bool Failed()
        {
            return exchange.GetUserAccount(Ship.Owner).StoredGoods.GetItemCount(good) < volume;
        }
    }

    class MoveAndPlaceSellOrder : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;
        int price;

        public MoveAndPlaceSellOrder(Ship ship, Exchange exchange, Good good, int volume, int price, ICompositeGoal creator)
            : base("Move and place sell order...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
            this.price = price;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveAndDockAt(Ship, exchange, this);
            yield return new PlaceSellOrder(Ship, exchange, good, volume, price, this);
        }

        public bool Failed()
        {
            return exchange.GetUserAccount(Ship.Owner).StoredGoods.GetItemCount(good) < volume;
        }
    }

    class MoveAndDepositGoods : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;

        public MoveAndDepositGoods(Ship ship, Exchange exchange, Good good, int volume, ICompositeGoal creator)
            : base("Move and deposit goods...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveAndDockAt(Ship, exchange, this);
            yield return new DepositGoods(Ship, exchange, good, volume, this);
        }

        public bool Failed()
        {
            return Ship.Cargo.GetItemCount(good) < volume;
        }
    }

    class DepositGoods : ShipGoal, IBaseGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;

        public DepositGoods(Ship ship, Exchange exchange, Good good, int volume, ICompositeGoal creator)
            : base("Deposit goods...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
        }

        public void Execute()
        {
            if (Ship.DockedStation != exchange) return;
            if (!exchange.HasUser(Ship.Owner)) exchange.AddUser(Ship.Owner);
            exchange.DepositGoods(Ship, good, volume);
        }

        public bool IsComplete()
        {
            return true;
        }

        public bool Failed()
        {
            return Ship.Cargo.GetItemCount(good) < volume;
        }
    }

    class MoveAndWithdrawGoods : ShipGoal, ICompositeGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;

        public MoveAndWithdrawGoods(Ship ship, Exchange exchange, Good good, int volume, ICompositeGoal creator)
            : base("Move and withdraw goods...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveAndDockAt(Ship, exchange, this);
            yield return new WithdrawGoods(Ship, exchange, good, volume, this);
        }

        public bool Failed()
        {
            return exchange.GetUserAccount(Ship.Owner).StoredGoods.GetItemCount(good) < volume
                || Ship.CargoSpace - Ship.Cargo.TotalSize < good.Size * volume;
        }
    }

    class WithdrawGoods : ShipGoal, IBaseGoal, IFailableGoal
    {
        Exchange exchange;
        Good good;
        int volume;

        public WithdrawGoods(Ship ship, Exchange exchange, Good good, int volume, ICompositeGoal creator)
            : base("Withdraw goods...", ship, creator)
        {
            this.exchange = exchange;
            this.good = good;
            this.volume = volume;
        }

        public void Execute()
        {
            if (Ship.DockedStation != exchange) return;
            if (!exchange.HasUser(Ship.Owner)) exchange.AddUser(Ship.Owner);
            exchange.WithdrawGoods(Ship, good, volume);
        }

        public bool IsComplete()
        {
            return true;
        }

        public bool Failed()
        {
            return exchange.GetUserAccount(Ship.Owner).StoredGoods.GetItemCount(good) < volume
                || Ship.CargoSpace - Ship.Cargo.TotalSize < good.Size * volume;
        }
    }
}
