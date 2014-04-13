using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Sector
    {
        List<ProductionStation> stations;
        List<Ship> ships;
        List<Gate> gates;

        // Ships that left/entered the sector (can't remove/insert them on the fly: 
        // a ship can call Remove/AddShip during a foreach loop over ships in the sector)

        List<Ship> removeList;
        List<Ship> addList;

        public List<ProductionStation> Stations { get { return stations; } }
        public List<Ship> Ships { get { return ships; } }
        public List<Gate> Gates { get { return gates; } }

        public Sector()
        {
            stations = new List<ProductionStation>();
            ships = new List<Ship>();
            gates = new List<Gate>();
            removeList = new List<Ship>();
            addList = new List<Ship>();
        }

        public void RemoveShip(Ship ship) {
            removeList.Add(ship);
        }

        public void AddShip(Ship ship)
        {
            addList.Add(ship);
        }

        public void Update(float timePassed)
        {
            foreach (Ship s in ships)
            {
                s.Update(timePassed);
            }

            ships.RemoveAll(s => removeList.Contains(s));
            removeList.Clear();

            ships.AddRange(addList);
            addList.Clear();
        }
    }
}
