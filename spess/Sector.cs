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

        // Ships that left the sector (can't remove them on the fly: a ship can call RemoveShip
        // during a foreach loop over ships in the sector)

        List<Ship> removeList;

        public List<ProductionStation> Stations { get { return stations; } }
        public List<Ship> Ships { get { return ships; } }
        public List<Gate> Gates { get { return gates; } }

        public Sector()
        {
            stations = new List<ProductionStation>();
            ships = new List<Ship>();
            gates = new List<Gate>();
            removeList = new List<Ship>();
        }

        public void RemoveShip(Ship ship) {
            removeList.Add(ship);
        }

        public void AddShip(Ship ship)
        {
            ships.Add(ship);
        }

        public void Update()
        {
            ships.RemoveAll(s => removeList.Contains(s));
            removeList.Clear();
        }
    }
}
