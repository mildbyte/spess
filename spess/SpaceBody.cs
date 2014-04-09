using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class SpaceBody
    {
        protected string name;
        protected Location location;
        
        public string Name { get { return name; } set { name = value; } }
        public Location Location { get { return location; } set { location = value; } }

        public SpaceBody(string name, Location location)
        {
            this.name = name; this.location = location;
        }
    }
}
