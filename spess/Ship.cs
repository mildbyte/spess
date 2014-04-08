using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Ship
    {
        Inventory cargo;
        Location location;
        Owner owner;
        Station dockedStation;
        Vector velocity;
        double maxSpeed;

        public Location Location { get { return location; } set { location = value; } }
        public Inventory Cargo { get { return cargo; } }
        public Owner Owner { get { return owner; } }
        public double MaxSpeed { get { return maxSpeed; } }
        public Vector Velocity
        {
            get { return velocity; }
            set
            {
                if (value.Magnitude() > maxSpeed) return;
                velocity = value;
            }
        }

        public Ship(Owner owner, double maxSpeed)
        {
            cargo = new Inventory();
            this.owner = owner;
            dockedStation = null;
            this.maxSpeed = maxSpeed;
        }

        bool CanUseGate(Gate gate)
        {
            return (gate.Location.Sector == location.Sector && location.Coordinates.Distance(gate.Location.Coordinates) < 100.0);
        }

        void UseGate(Gate gate)
        {
            if (!CanUseGate(gate)) return;

            location.Sector.RemoveShip(this);

            location.Sector = gate.Destination.Sector;
            location.Coordinates = gate.Destination.Coordinates;

            location.Sector.AddShip(this);
        }

        bool CanDock(Station station)
        {
            return (station.Location.Sector == location.Sector && location.Coordinates.Distance(station.Location.Coordinates) < 100.0);
        }

        void Dock(Station station)
        {
            if (CanDock(station)) {
                station.DockShip(this);
                dockedStation = station;
            }
        }

        void Undock()
        {
            if (dockedStation != null)
            {
                dockedStation.UndockShip(this);
                dockedStation = null;
            }
        }
    }
}
