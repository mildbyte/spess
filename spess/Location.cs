using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Location
    {
        Sector sector;
        Vector coordinates;

        public Sector Sector { get { return sector; } set { sector = value; } }
        public Vector Coordinates { get { return coordinates; } set { coordinates = value; } }

        public Location(Sector sector, Vector coordinates)
        {
            this.sector = sector; this.coordinates = coordinates;
        }
    }
}
