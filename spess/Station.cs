using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Station
    {
        string name;
        Location location;
        ProductionRule production;
        int storageSpace;
        Dictionary<Good, int> buysStock;
        Dictionary<Good, int> sellsStock;
        List<Ship> dockedShips;

        public string Name { get { return name; } set { name = value; } }
        public Location Location { get { return location; } set { location = value; } }
        public ProductionRule Production { get { return production; } }
        public int StorageSpace { get { return storageSpace; } }
        public Dictionary<Good, int> BuysStock { get { return buysStock; } }
        public Dictionary<Good, int> SellsStock { get { return sellsStock; } }
        public List<Ship> DockedShips { get { return dockedShips; } }

        public Station(string name, Location location, ProductionRule production, int storageSpace,
            Dictionary<Good, int> buysStock, Dictionary<Good, int> sellsStock)
        {
            this.name = name; this.location = location;
            this.production = production; this.storageSpace = storageSpace;
            this.buysStock = buysStock; this.sellsStock = sellsStock;
            dockedShips = new List<Ship>();
        }
    }
}
