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
    public class Ship : SpaceBody
    {
        Inventory cargo;
        Building dockedStation;
        Vector3 velocity;
        float maxSpeed;
        GoalQueue goalQueue;
        int cargoSpace;

        public Inventory Cargo { get { return cargo; } }
        public float MaxSpeed { get { return maxSpeed; } }
        public Building DockedStation { get { return dockedStation; } }
        public GoalQueue GoalQueue { get { return goalQueue; } }
        public int CargoSpace { get { return cargoSpace; } set { cargoSpace = value; } }

        public Vector3 Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
                float len = velocity.Length();
                if (len > maxSpeed) velocity *= (maxSpeed / len);
            }
        }

        public Ship(string name, Location location, Owner owner, float maxSpeed, Universe universe) : base(name, location, universe)
        {
            cargo = new Inventory();
            this.owner = owner;
            dockedStation = null;
            this.maxSpeed = maxSpeed;
            this.goalQueue = new GoalQueue();
            this.iconTexture = TextureProvider.shipTex;
            cargoSpace = 100;
        }

        public bool CanUseGate(Gate gate)
        {
            return (gate.Location.Sector == location.Sector && (location.Coordinates - gate.Location.Coordinates).Length() < 100.0);
        }

        public void UseGate(Gate gate)
        {
            if (!CanUseGate(gate)) return;

            location.Sector.RemoveItem(this);

            location.Sector = gate.Destination.Sector;
            location.Coordinates = gate.Destination.Coordinates;

            location.Sector.AddItem(this);
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

        public override void Update(float timePassed)
        {
            //TODO: if timePassed is large, can overshoot the goal point
            location.Coordinates += velocity * timePassed;
            goalQueue.Update();
        }

        public override string ToString()
        {
            return Name + "\nCargo Hold:\n" + Cargo.ToString();
        }
    }
}
