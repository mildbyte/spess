using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    public enum AIShipRole
    {
        Supplier,    // Goes from the station to the market, buying/selling goods
        Arbitrageur  // Goes between markets trying to make a profit
    }

    public class AIShip : Ship
    {
        public AIShipRole Role { get; set; }

        public AIShip(string name, Location location, Owner owner, float maxSpeed, Universe universe)
            : base(name, location, owner, maxSpeed, universe)
        {

        }
    }
}
