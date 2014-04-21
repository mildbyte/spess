using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.AI
{
    class HivemindAI
    {
        public static Ship GetClosestShipTo(Owner owner, Building building)
        {
            return building.Location.Sector.Contents.OfType<Ship>()
                .Where(s => s.Owner == owner)
                .OrderBy(s => (s.Location.Coordinates - building.Location.Coordinates).Length())
                .First();
        }
    }
}
