using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class ProductionStation : Building
    {
        ProductionRule production;
        int storageSpace;
        Inventory inventory;
        int productionProgress;

        public ProductionRule Production { get { return production; } }
        public int StorageSpace { get { return storageSpace; } }
        public Inventory Inventory { get { return inventory; } }

        public ProductionStation(string name, Location location, ProductionRule production, int storageSpace) : base(name, location)
        {
            this.production = production; this.storageSpace = storageSpace;
            this.inventory = new Inventory();
        }

        public int OccupiedSpace()
        {
            int sum = 0;

            foreach (KeyValuePair<Good, int> stock in inventory) sum += stock.Value * stock.Key.Size;

            return sum;
        }

        public bool CanProduce()
        {
            foreach (KeyValuePair<Good, int> required in production.Input)
                if (inventory.GetItemCount(required.Key) < required.Value) return false;

            return true;
        }

        void ConsumeProductionResources()
        {
            foreach (KeyValuePair<Good, int> required in production.Input)
                inventory.RemoveItem(required.Key, required.Value);
        }

        void AddProductionResult()
        {
            foreach (KeyValuePair<Good, int> result in production.Output)
                inventory.AddItem(result.Key, result.Value);
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
