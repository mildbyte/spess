using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace spess.AI
{
    abstract class ShipGoal : Goal
    {
        Ship ship;
        public Ship Ship { get { return ship; } }

        public ShipGoal (string name, Ship ship, ICompositeGoal creator) : base(name, creator)
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
        public Undock(Ship ship, ICompositeGoal creator) : base ("Undock...", ship, creator) {}

        public bool IsComplete() { return Ship.DockedStation == null; }

        public void Execute()
        {
            Ship.Undock();
        }
    }

    class DockAt : ShipGoal, IBaseGoal
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
    }

    class MoveAndDockAt : ShipGoal, ICompositeGoal
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
}
