using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Gate
    {
        Location location;
        Location destination;

        public Location Location { get { return location; } }
        public Location Destination { get { return destination; } }

        public Gate(Location location, Location destination)
        {
            this.location = location;
            this.destination = destination;
        }
    }
}
