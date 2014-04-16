using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    public class ProductionStation : Building
    {
        ProductionRule production;
        int storageSpace;
        Inventory inventory;
        float productionProgress;

        public ProductionRule Production { get { return production; } }
        public int StorageSpace { get { return storageSpace; } }
        public Inventory Inventory { get { return inventory; } }

        public ProductionStation(string name, Location location, ProductionRule production, int storageSpace, Universe universe) : base(name, location, universe)
        {
            this.production = production; this.storageSpace = storageSpace;
            this.inventory = new Inventory();
            this.iconTexture = TextureProvider.stationTex;
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

        public override void Update(float timePassed) {
            if (!CanProduce()) return;

            productionProgress += timePassed;

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

        public override string ToString()
        {
            string result = Name + "\nProgress: " + productionProgress.ToString("F2");

            foreach (KeyValuePair<Good, int> kv in Inventory)
            {
                result += "\n" + kv.Key.Name + ": " + kv.Value;
            }

            return result;
        }
    }
}
