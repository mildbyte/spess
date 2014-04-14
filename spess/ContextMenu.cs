using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace spess
{
    class ContextMenuItem
    {
        public string Text { get; set; }
        public Delegate Action { get; set; }

        public ContextMenuItem(string text, Delegate action)
        {
            Text = text;
            Action = action;
        }
    }

    class ContextMenu
    {
        List<ContextMenuItem> items;
        bool isOpen;
        Rectangle menuLocation;
        int highlightedIndex;

        public bool IsOpen { get { return isOpen; } }
        public List<ContextMenuItem> Items { get { return items; } }
        public SpriteFont Font { get; set; }

        public ContextMenu(SpriteFont font)
        {
            Font = font;
            items = new List<ContextMenuItem>();
            highlightedIndex = -1;
        }
        
        public void NotifyMousePosition(int x, int y) {
            if (!isOpen) return;

            if (menuLocation.Contains(x, y))
            {
                highlightedIndex = (int)((y - menuLocation.Y) / Font.LineSpacing);
                if (highlightedIndex >= items.Count) highlightedIndex = -1;
            }
            else
            {
                Close();
            }
        }

        public void Open(int x, int y)
        {
            string text = String.Join("\n", items.Select(cmi => cmi.Text));
            Vector2 size = Font.MeasureString(text);
            menuLocation = new Rectangle(x, y, (int)size.X, (int)size.Y);
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
            highlightedIndex = -1;
        }

        public void Render(SpriteBatch spriteBatch, Texture2D bgTex)
        {
            if (!isOpen) return;

            string text = String.Join("\n", items.Select(cmi => cmi.Text));
            Vector2 size = Font.MeasureString(text);

            spriteBatch.Begin();

            // Draw the menu rectangle and the highlighted object strip
            spriteBatch.Draw(bgTex, new Rectangle(menuLocation.X, menuLocation.Y, (int)size.X, (int)size.Y), Color.White);
            if (highlightedIndex != -1) 
                spriteBatch.Draw(bgTex, new Rectangle(menuLocation.X, highlightedIndex * Font.LineSpacing + menuLocation.Y, 
                    (int)size.X, Font.LineSpacing), Color.Gray);

            // Draw the actual menu text
            spriteBatch.DrawString(Font, text, new Vector2(menuLocation.X, menuLocation.Y), Color.White);
            spriteBatch.End();
        }
    }
}
