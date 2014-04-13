using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    class SpaceBody
    {
        protected string name;
        protected Location location;
        protected Texture2D iconTexture;
        protected float iconSize;
        
        public string Name { get { return name; } set { name = value; } }
        public Location Location { get { return location; } set { location = value; } }
        public Texture2D IconTexture { get { return iconTexture; } set { iconTexture = value; } }
        public float IconSize { get { return iconSize; } set { iconSize = value; } }

        public SpaceBody(string name, Location location)
        {
            this.name = name; this.location = location;
            iconTexture = null;
            IconSize = 48.0f;
        }
    }
}
