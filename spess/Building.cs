using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    class Building : SpaceBody
    {
        List<Ship> dockedShips;

        public List<Ship> DockedShips { get { return dockedShips; } }

        public void DockShip(Ship ship)
        {
            dockedShips.Add(ship);
        }

        public void UndockShip(Ship ship)
        {
            dockedShips.Remove(ship);
        }

        public Building(string name, Location location, Universe universe) : base (name, location, universe)
        {
            dockedShips = new List<Ship>();
        }
    }
}
