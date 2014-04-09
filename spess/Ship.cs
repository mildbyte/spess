using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Ship : SpaceBody
    {
        Inventory cargo;
        Owner owner;
        Building dockedStation;
        Vector velocity;
        double maxSpeed;

        public Inventory Cargo { get { return cargo; } }
        public Owner Owner { get { return owner; } }
        public double MaxSpeed { get { return maxSpeed; } }
        public Building DockedStation { get { return dockedStation; } }

        public Vector Velocity
        {
            get { return velocity; }
            set
            {
                if (value.Magnitude() > maxSpeed) return;
                velocity = value;
            }
        }

        public Ship(string name, Location location, Owner owner, double maxSpeed) : base(name, location)
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

        bool CanDock(ProductionStation station)
        {
            return (station.Location.Sector == location.Sector && location.Coordinates.Distance(station.Location.Coordinates) < 100.0);
        }

        void Dock(ProductionStation station)
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
