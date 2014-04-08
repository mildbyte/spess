using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Ship
    {
        Dictionary<Good, int> cargo;
        Location location;

        public Location Location { get { return location; } set { location = value; } }
        public Dictionary<Good, int> Cargo { get { return cargo; } }

        public Ship()
        {
            cargo = new Dictionary<Good, int>();
        }
    }
}
