using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace spess.UI
{
    public delegate void MenuItemClicked();

    class ContextMenuItem
    {
        public string Text { get; set; }
        public MenuItemClicked Action { get; set; }

        public ContextMenuItem(string text, MenuItemClicked action)
        {
            Text = text;
            Action = action;
        }
    }

    class ContextMenu : Component
    {
        List<ContextMenuItem> items;
        bool isOpen;
        bool clickRegistered;
        Rectangle menuLocation;
        int highlightedIndex;

        public bool IsOpen { get { return isOpen; } }
        public List<ContextMenuItem> Items { get { return items; } }
        public SpriteFont Font { get; set; }
        public int Padding { get; set; }

        public ContextMenu(SpriteFont font)
        {
            Font = font;
            items = new List<ContextMenuItem>();
            highlightedIndex = -1;
            isOpen = false;
            clickRegistered = false;
            Padding = 5;
        }
        
        public void NotifyMouseStateChange(MouseState ms) {
            if (!isOpen) return;

            if (menuLocation.Contains(ms.X, ms.Y))
            {
                highlightedIndex = (int)((ms.Y - menuLocation.Y - Padding) / Font.LineSpacing);
                if (highlightedIndex >= items.Count) highlightedIndex = -1;
            }
            else
            {
                Close();
                return;
            }

            // A flag so that holding a mouse on the object doesn't trigger several events.
            if (!clickRegistered && ms.LeftButton == ButtonState.Pressed && highlightedIndex != -1)
            {
                clickRegistered = true;
                items[highlightedIndex].Action();
            }
            else if (clickRegistered && ms.LeftButton == ButtonState.Released)
            {
                clickRegistered = false;
            }
        }

        public void Open(int x, int y)
        {
            string text = String.Join("\n", items.Select(cmi => cmi.Text));
            Vector2 size = Font.MeasureString(text);
            menuLocation = new Rectangle(x, y, (int)size.X + Padding * 2, (int)size.Y + Padding * 2);
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
            highlightedIndex = -1;
        }

        public override void Render(SpriteBatch spriteBatch, Texture2D bgTex)
        {
            if (!isOpen) return;

            string text = String.Join("\n", items.Select(cmi => cmi.Text));
            Vector2 size = Font.MeasureString(text);

            spriteBatch.Begin();

            // Draw the menu rectangle and the highlighted object strip
            spriteBatch.Draw(bgTex, new Rectangle(menuLocation.X, menuLocation.Y, menuLocation.Width, menuLocation.Height), Color.White);
            if (highlightedIndex != -1) 
                spriteBatch.Draw(bgTex, new Rectangle(menuLocation.X, highlightedIndex * Font.LineSpacing + menuLocation.Y + Padding, 
                    menuLocation.Width, Font.LineSpacing), Color.Gray);

            // Draw the actual menu text
            spriteBatch.DrawString(Font, text, new Vector2(menuLocation.X + Padding, menuLocation.Y + Padding), Color.Black);
            spriteBatch.End();
        }
    }
}
