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

        public Location Location { get { return location; } set { location = value; } }
        public Inventory Cargo { get { return cargo; } }
        public Owner Owner { get { return owner; } }

        public Ship(Owner owner)
        {
            cargo = new Inventory();
            this.owner = owner;
        }
    }
}
