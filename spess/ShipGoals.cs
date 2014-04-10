using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class MoveToSector : ShipGoal
    {
        Sector sector;
        public Sector Sector { get { return sector; } }

        public MoveToSector(Ship ship, Sector sector)
            : base("Move to sector...", ship) 
        {
            this.sector = sector;
        }

        public override bool IsComplete() { return true; }

        public override IEnumerable<Goal> Execute()
        {
            if (Ship.Location.Sector == sector) return Enumerable.Empty<Goal>();

            Ship.Location.Sector = sector;
            return Enumerable.Empty<Goal>();
        }
    }

    class MoveInSector : ShipGoal
    {
        Vector position;
        public Vector Position { get { return position; } }

        public MoveInSector(Ship ship, Vector position)
            : base("Move to position in sector...", ship)
        {
            this.position = position;
        }

        public override bool IsComplete() { return true; }

        public override IEnumerable<Goal> Execute()
        {
            if (Ship.Location.Coordinates.Distance(position) < 1.0) return Enumerable.Empty<Goal>();

            Ship.Location.Coordinates = position;
            return Enumerable.Empty<Goal>();
        }
    }

    class MoveTo : ShipGoal
    {
        Location location;
        public Location Location { get { return location; } }

        public MoveTo(Ship ship, Location location) : base ("Move to...", ship)
        {
            this.location = location;
        }

        public override bool IsComplete() { return true; }

        public override IEnumerable<Goal> Execute()
        {
            yield return new MoveToSector(Ship, location.Sector);
            yield return new MoveInSector(Ship, location.Coordinates);
        }
    }
}
