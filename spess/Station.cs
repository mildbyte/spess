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
        Inventory buysStock;
        Inventory sellsStock;
        List<Ship> dockedShips;
        int productionProgress;

        public string Name { get { return name; } set { name = value; } }
        public Location Location { get { return location; } set { location = value; } }
        public ProductionRule Production { get { return production; } }
        public int StorageSpace { get { return storageSpace; } }
        public Inventory BuysStock { get { return buysStock; } }
        public Inventory SellsStock { get { return sellsStock; } }
        public List<Ship> DockedShips { get { return dockedShips; } }

        public Station(string name, Location location, ProductionRule production, int storageSpace,
            Dictionary<Good, int> buysStock, Dictionary<Good, int> sellsStock)
        {
            this.name = name; this.location = location;
            this.production = production; this.storageSpace = storageSpace;
            this.buysStock = new Inventory(); this.sellsStock = new Inventory();
            dockedShips = new List<Ship>();
        }

        public int OccupiedSpace()
        {
            int sum = 0;

            foreach (KeyValuePair<Good, int> stock in buysStock) sum += stock.Value * stock.Key.Size;
            foreach (KeyValuePair<Good, int> stock in sellsStock) sum += stock.Value * stock.Key.Size;

            return sum;
        }

        public bool CanProduce()
        {
            foreach (KeyValuePair<Good, int> required in production.Input)
                if (buysStock.GetItemCount(required.Key) < required.Value) return false;

            return true;
        }

        void ConsumeProductionResources()
        {
            foreach (KeyValuePair<Good, int> required in production.Input)
                buysStock.RemoveItem(required.Key, required.Value);
        }

        void AddProductionResult()
        {
            foreach (KeyValuePair<Good, int> result in production.Output)
                sellsStock.AddItem(result.Key, result.Value);
        }

        public void Tick(int ticks) {
            productionProgress += ticks;

            while (productionProgress > production.RequiredTime)
            {
                productionProgress -= production.RequiredTime;

                AddProductionResult();

                if (!CanProduce())
                {
                    productionProgress = 0;
                    break;
                }
            }
        }
    }
}
