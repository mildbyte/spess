using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace spess.UI
{
    public abstract class Component
    {
        public abstract void Render(SpriteBatch spriteBatch, Texture2D bgTex);
    }
}
