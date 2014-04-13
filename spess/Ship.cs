using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using spess.AI;

namespace spess
{
    class Ship : SpaceBody
    {
        Inventory cargo;
        Owner owner;
        Building dockedStation;
        Vector3 velocity;
        double maxSpeed;
        GoalQueue goalQueue;

        public Inventory Cargo { get { return cargo; } }
        public Owner Owner { get { return owner; } }
        public double MaxSpeed { get { return maxSpeed; } }
        public Building DockedStation { get { return dockedStation; } }
        public GoalQueue GoalQueue { get { return goalQueue; } }

        public Vector3 Velocity
        {
            get { return velocity; }
            set
            {
                if (value.Length() > maxSpeed) return;
                velocity = value;
            }
        }

        public Ship(string name, Location location, Owner owner, double maxSpeed, Texture2D texture) : base(name, location, texture, 48.0f)
        {
            cargo = new Inventory();
            this.owner = owner;
            dockedStation = null;
            this.maxSpeed = maxSpeed;
            this.goalQueue = new GoalQueue();
        }

        public bool CanUseGate(Gate gate)
        {
            return (gate.Location.Sector == location.Sector && (location.Coordinates - gate.Location.Coordinates).Length() < 100.0);
        }

        public void UseGate(Gate gate)
        {
            if (!CanUseGate(gate)) return;

            location.Sector.RemoveShip(this);

            location.Sector = gate.Destination.Sector;
            location.Coordinates = gate.Destination.Coordinates;

            location.Sector.AddShip(this);
        }

        public bool CanDock(Building building)
        {
            return (building.Location.Sector == location.Sector && (location.Coordinates - building.Location.Coordinates).Length() < 100.0);
        }

        public void Dock(Building building)
        {
            if (CanDock(building)) {
                building.DockShip(this);
                dockedStation = building;
            }
        }

        public void Undock()
        {
            if (dockedStation != null)
            {
                dockedStation.UndockShip(this);
                dockedStation = null;
            }
        }

        public void Update(float timePassed)
        {
            location.Coordinates += velocity * timePassed;
        }
    }
}
