using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    public class Inventory : IEnumerable
    {
        Dictionary<Good, int> items;

        public int TotalSize { get { return items.Sum(kv => kv.Key.Size * kv.Value); } }

        public int GetItemCount(Good good)
        {
            if (!items.ContainsKey(good)) return 0;
            return items[good];
        }

        public void SetItemCount(Good good, int amount)
        {
            if (amount == 0) items.Remove(good);
            else items[good] = amount;
        }

        public void AddItem(Good good, int amount)
        {
            SetItemCount(good, GetItemCount(good) + amount);
        }

        public void RemoveItem(Good good, int amount)
        {
            AddItem(good, -amount);
        }

        public Inventory()
        {
            items = new Dictionary<Good, int>();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (KeyValuePair<Good, int> kv in items) {
                result.Append("\n" + kv.Key.Name + ": " + kv.Value);
            }

            return result.ToString();
        }
    }
}
