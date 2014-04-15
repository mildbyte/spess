using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace spess.UI
{

    class FloatingLabel : Component
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 MouseDistance { get; set; }
        public int Padding { get; set; }
        public SpriteFont Font { get; set; }

        public FloatingLabel(string text, Vector2 position, SpriteFont spriteFont)
        {
            Text = text; Position = position; Font = spriteFont;
            MouseDistance = new Vector2(20, 20);
            Padding = 5;
        }

        public override void Render(SpriteBatch spriteBatch, Texture2D bgTex) {
            Vector2 size = Font.MeasureString(Text);

            Vector2 labelStart = Position + MouseDistance;
            Vector2 textStart = Position + MouseDistance + new Vector2(Padding, Padding);
            
            int labelSizeX = (int)size.X + Padding * 2;
            int labelSizeY = (int)size.Y + Padding * 2;

            spriteBatch.Begin();
            spriteBatch.Draw(bgTex, new Rectangle((int)labelStart.X, (int)labelStart.Y, labelSizeX, labelSizeY), Color.White);
            spriteBatch.DrawString(Font, Text, textStart, Color.Black);
            spriteBatch.End();
        }
        
    }
}
