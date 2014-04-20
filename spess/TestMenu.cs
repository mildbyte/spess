using NuclearWinter.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuclearWinter.UI;

namespace spess
{
    public class GameStateMainMenu : NuclearWinter.GameFlow.GameStateFadeTransition<SpessGame>
    {
        //----------------------------------------------------------------------
        Screen mScreen;

        //----------------------------------------------------------------------
        public GameStateMainMenu(SpessGame _game)
            : base(_game)
        {
        }

        //----------------------------------------------------------------------
        public override void Start()
        {
            Game.IsMouseVisible = true;

            mScreen = new Screen(Game, new Style(), Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);

            //------------------------------------------------------------------
            Label titleLabel = new Label(mScreen, "Hello world!");
            titleLabel.Font = mScreen.Style.LargeFont;

            mScreen.Root.AddChild(titleLabel);

            //------------------------------------------------------------------
            base.Start();
        }

        //----------------------------------------------------------------------
        public override void Update(float _fElapsedTime)
        {
            mScreen.IsActive = Game.IsActive;
            mScreen.HandleInput();
            mScreen.Update(_fElapsedTime);
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            mScreen.Draw();
        }
    }
}
