using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    class FloatingLabel
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }

        public FloatingLabel(string text, Vector2 position)
        {
            Text = text; Position = position;
        }

        public void Render(SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D bgTex) {
            string[] lines = Text.Split('\n');
            Vector2 size = spriteFont.MeasureString(Text);

            spriteBatch.Begin();
            spriteBatch.Draw(bgTex, new Rectangle((int)Position.X + 20, (int)Position.Y + 20, (int)size.X + 10, (int)size.Y + 10), Color.White);
            spriteBatch.DrawString(spriteFont, Text, Position + new Vector2(25, 25), Color.Black);
            spriteBatch.End();
        }
        
    }
}
