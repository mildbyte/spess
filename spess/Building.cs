using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    public abstract class Building : SpaceBody
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

        public abstract bool PermittedToDock(Ship s);

        public abstract int AvailableGoodsFor(Ship s, Good g);
        public abstract void WithdrawGoods(Ship s, Good g, int amount);
        public abstract void DepositGoods(Ship s, Good g, int amount);
    }
}
