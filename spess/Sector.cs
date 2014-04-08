using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Sector
    {
        List<Station> stations;
        List<Ship> ships;
        List<Gate> gates;

        public List<Station> Stations { get { return stations; } }
        public List<Ship> Ships { get { return ships; } }
        public List<Gate> Gates { get { return gates; } }

        public Sector()
        {
            stations = new List<Station>();
            ships = new List<Ship>();
            gates = new List<Gate>();
        }

        public void RemoveShip(Ship ship) {
            ships.Remove(ship);
        }

        public void AddShip(Ship ship)
        {
            ships.Add(ship);
        }
    }
}
