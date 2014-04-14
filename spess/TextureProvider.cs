using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace spess
{
    class TextureProvider
    {
        public static Texture2D shipTex;
        public static Texture2D gateTex;
        public static Texture2D stationTex;
        public static Texture2D satelliteTex;
        public static Texture2D exchangeTex;

        public static void LoadTextures(ContentManager content) {
            shipTex = content.Load<Texture2D>("ship");
            gateTex = content.Load<Texture2D>("gate");
            stationTex = content.Load<Texture2D>("station");
            satelliteTex = content.Load<Texture2D>("satellite");
            exchangeTex = content.Load<Texture2D>("exchange");
        }
    }
}
