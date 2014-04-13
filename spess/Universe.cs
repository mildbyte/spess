using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace spess
{
    class Universe
    {
        List<Sector> sectors;
        List<Owner> owners;

        public List<Sector> Sectors { get { return sectors; } }
        public List<Owner> Owners { get { return owners; } }

        public Owner GetPlayer() { return owners[0]; }

        public Universe()
        {
            sectors = new List<Sector>();
            owners = new List<Owner>();
            owners.Add(new Owner());
        }

        public Sector AddSector()
        {
            Sector s = new Sector();
            sectors.Add(s);
            return s;
        }

        public Ship AddShip(Sector sector, Vector3 position, Owner owner, float maxSpeed)
        {
            Ship ship = new Ship("", new Location(sector, position), owner, maxSpeed);
            sector.AddShip(ship);
            return ship;
        }

        public ProductionStation AddProductionStation(Sector sector, Vector3 position, ProductionRule production, int storageSpace) {
            ProductionStation station = new ProductionStation("", new Location(sector, position), production, storageSpace);
            sector.Stations.Add(station);
            return station;
        }

        public void JoinSectors(Sector sector1, Sector sector2, Vector3 sector1GateLoc, Vector3 sector2GateLoc)
        {
            Location s1Loc = new Location(sector1, sector1GateLoc);
            Location s2Loc = new Location(sector2, sector2GateLoc);

            Gate s1Gate = new Gate("", s1Loc, s2Loc);
            Gate s2Gate = new Gate("", s2Loc, s1Loc);

            sector1.Gates.Add(s1Gate);
            sector2.Gates.Add(s2Gate);
        }
    }
}
