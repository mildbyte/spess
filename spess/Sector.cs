using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using spess.ExchangeData;

namespace spess
{
    class Sector
    {
        List<SpaceBody> bodiesList;

        // Things that left/entered the sector (can't remove/insert them on the fly: 
        // during a foreach loop over all items an item can remove itself

        List<SpaceBody> removeList;
        List<SpaceBody> addList;

        public List<SpaceBody> Contents { get { return bodiesList; } }

        public Sector()
        {
            bodiesList = new List<SpaceBody>();
            removeList = new List<SpaceBody>();
            addList = new List<SpaceBody>();
        }

        public void RemoveItem(SpaceBody body) {
            removeList.Add(body);
        }

        public void AddItem(SpaceBody body)
        {
            addList.Add(body);
        }

        public void ForcePropagateChanges()
        {
            bodiesList.RemoveAll(b => removeList.Contains(b));
            removeList.Clear();

            bodiesList.AddRange(addList);
            addList.Clear();
        }

        public void Update(float timePassed)
        {
            foreach (SpaceBody item in bodiesList)
            {
                item.Update(timePassed);
            }

            ForcePropagateChanges();
        }
    }
}
