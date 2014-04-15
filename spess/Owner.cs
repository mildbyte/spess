using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    public class Owner
    {
        string name;
        List<ProductionStation> ownedStations;
        List<Ship> ownedShips;
        int accountBalance;

        public string Name { get { return name; } set { name = value; } }
        public int Balance { get { return accountBalance; } set { accountBalance = value; }}
        public List<Ship> Ships { get { return ownedShips; } }
        public List<ProductionStation> Stations { get { return ownedStations; } }

        public Owner()
        {
            ownedStations = new List<ProductionStation>();
            ownedShips = new List<Ship>();
        }
    }
}
