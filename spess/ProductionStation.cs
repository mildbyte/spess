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
        bool isProducing;

        public ProductionRule Production { get { return production; } }
        public int StorageSpace { get { return storageSpace; } }
        public Inventory Inventory { get { return inventory; } }
        public bool IsProducing { get { return isProducing; } }

        public ProductionStation(string name, Location location, ProductionRule production, int storageSpace, Universe universe) : base(name, location, universe)
        {
            this.production = production; this.storageSpace = storageSpace;
            this.inventory = new Inventory();
            this.iconTexture = TextureProvider.stationTex;
            isProducing = false;
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
            if (isProducing)
            {
                productionProgress += timePassed;

                while (productionProgress > production.RequiredTime)
                {
                    productionProgress -= production.RequiredTime;

                    // End this production cycle and start the next one
                    AddProductionResult();

                    if (!CanProduce())
                    {
                        isProducing = false;
                        productionProgress = 0;
                        break;
                    }

                    ConsumeProductionResources();
                }
            }
            else
            {
                if (CanProduce()) {
                    productionProgress = timePassed;
                    ConsumeProductionResources();
                    isProducing = true;
                }
            }
        }

        public override string ToString()
        {
            string result = Name + "\nProgress: " + productionProgress.ToString("F2");

            result += "\n" + inventory.ToString();

            return result;
        }

        public override bool PermittedToDock(Ship s)
        {
            return s.Owner == Owner;
        }

        public override int AvailableGoodsFor(Ship s, Good g)
        {
            if (!PermittedToDock(s)) return 0;

            return inventory.GetItemCount(g);
        }

        public override void DepositGoods(Ship s, Good g, int amount)
        {
            Inventory.AddItem(g, amount);
            s.Cargo.RemoveItem(g, amount);
        }

        public override void WithdrawGoods(Ship s, Good g, int amount)
        {
            Inventory.RemoveItem(g, amount);
            s.Cargo.AddItem(g, amount);
        }
    }
}
