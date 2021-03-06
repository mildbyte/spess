﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    public abstract class SpaceBody
    {
        protected string name;
        protected Owner owner;
        protected Location location;
        protected Texture2D iconTexture;
        protected float iconSize;

        Universe universe;
        
        public string Name { get { return name; } set { name = value; } }
        public Location Location { get { return location; } set { location = value; } }
        public Texture2D IconTexture { get { return iconTexture; } set { iconTexture = value; } }
        public float IconSize { get { return iconSize; } set { iconSize = value; } }
        public Universe Universe { get { return universe; } }
        public Owner Owner { get { return owner; } set { owner = value; } }

        public SpaceBody(string name, Location location, Universe universe)
        {
            this.name = name; this.location = location; this.universe = universe;
            iconTexture = null;
            IconSize = 48.0f;
        }

        public abstract void Update(float timePassed);

        public override string ToString()
        {
            return name;
        }
    }
}
