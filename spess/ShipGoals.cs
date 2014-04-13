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

        public ShipGoal (string name, Ship ship) : base(name)
        {
            this.ship = ship;
        }

    }

    class MoveToSector : ShipGoal, IBaseGoal
    {
        Sector sector;
        public Sector Sector { get { return sector; } }

        public MoveToSector(Ship ship, Sector sector)
            : base("Move to sector...", ship) 
        {
            this.sector = sector;
        }

        public bool IsComplete() { return Ship.Location.Sector == sector; }

        public void Execute()
        {
            Ship.Location.Sector = sector;
        }
    }

    class MoveInSector : ShipGoal, IBaseGoal
    {
        Vector3 position;
        public Vector3 Position { get { return position; } }

        public MoveInSector(Ship ship, Vector3 position)
            : base("Move to position in sector...", ship)
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

        public MoveTo(Ship ship, Location location) : base ("Move to...", ship)
        {
            this.location = location;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new Undock(Ship);
            yield return new MoveToSector(Ship, location.Sector);
            yield return new MoveInSector(Ship, location.Coordinates);
        }
    }

    class Undock : ShipGoal, IBaseGoal
    {
        public Undock(Ship ship) : base ("Undock...", ship) {}

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

        public DockAt(Ship ship, Building building)
            : base("Dock...", ship)
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

        public MoveAndDockAt(Ship ship, Building building)
            : base("Move and dock at...", ship)
        {
            this.building = building;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveTo(Ship, building.Location);
            yield return new DockAt(Ship, building);
        }
    }

    class UseGate : ShipGoal, IBaseGoal
    {
        Gate gate;
        public Gate Gate { get { return gate; } }

        public UseGate(Ship ship, Gate gate)
            : base("Use gate...", ship)
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

        public MoveAndUseGate(Ship ship, Gate gate)
            : base("Use gate...", ship)
        {
            this.gate = gate;
        }

        public IEnumerable<Goal> GetSubgoals()
        {
            yield return new MoveTo(Ship, Gate.Location);
            yield return new UseGate(Ship, Gate);
        }
    }
}
