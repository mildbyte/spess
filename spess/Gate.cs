using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    class Gate : SpaceBody
    {
        Location destination;

        public Location Destination { get { return destination; } }

        public Gate(string name, Location location, Location destination, Universe universe) : base(name, location, universe)
        {
            this.destination = destination;
            this.iconTexture = TextureProvider.gateTex;
        }
    }
}
