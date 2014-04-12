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

        public Building(string name, Location location, Texture2D iconTexture, float iconSize) : base (name, location, iconTexture, iconSize)
        {
            dockedShips = new List<Ship>();
        }
    }
}
