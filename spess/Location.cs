using System;
using Microsoft.Xna.Framework;

namespace spess
{
    class Location
    {
        Sector sector;
        Vector3 coordinates;

        public Sector Sector { get { return sector; } set { sector = value; } }
        public Vector3 Coordinates { get { return coordinates; } set { coordinates = value; } }

        public Location(Sector sector, Vector3 coordinates)
        {
            this.sector = sector; this.coordinates = coordinates;
        }
    }
}
