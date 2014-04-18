#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using spess.UI;
using spess.ExchangeData;
#endregion

namespace spess
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpessGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Universe universe;
        SectorScreen sectorScreen;

        Random rand = new Random();

        Good cabbages = new Good("Space Cabbages", "Cabbages in space!!1", 10);
        Good earth = new Good("Space Cabbage Seeds", "Makes cabbages!", 2);
        Good seeds = new Good("Space Earth", "A box of earth. In space. Taken from Earth.", 8);
        ProductionRule cabbageProd;

        public SpessGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.IsFullScreen = true;
            IsFixedTimeStep = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected void SetupUI()
        {
            sectorScreen = new SectorScreen(GraphicsDevice, this);
            sectorScreen.Font = font;

            FloatingLabel currLabel = null;

            sectorScreen.OnIconMouseEnter += delegate(SpaceBody mouseOverBody, MouseState ms)
            {
                currLabel = new FloatingLabel(mouseOverBody.ToString(), new Vector2(ms.X, ms.Y), font);
                sectorScreen.FloatingLabels.Add(currLabel);
            };

            sectorScreen.OnIconMouseover += delegate(SpaceBody mouseOverBody, MouseState ms)
            {
                currLabel.Position = new Vector2(ms.X, ms.Y);
                currLabel.Text = mouseOverBody.ToString();
            };

            sectorScreen.OnIconMouseLeave += delegate(SpaceBody mouseOverBody, MouseState ms)
            {
                sectorScreen.FloatingLabels.Remove(currLabel);
            };

            sectorScreen.OnIconClicked += delegate(SpaceBody mouseOverBody, MouseState ms)
            {
                if (ms.RightButton == ButtonState.Pressed)
                {
                    ContextMenu currMenu = new ContextMenu(font);
                    currMenu.Items.Add(new ContextMenuItem("Move to origin", delegate() { mouseOverBody.Location.Coordinates = Vector3.Zero; }));
                    currMenu.Items.Add(new ContextMenuItem("Remove", delegate() { mouseOverBody.Location.Sector.RemoveItem(mouseOverBody); }));
                    if (mouseOverBody is Ship)
                    {
                        currMenu.Items.Add(new ContextMenuItem("Dock in the Sector 2 Station", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDockAt((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(), null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Buy cabbages!!", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndPlaceBuyOrder((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    cabbages, 10, 10, null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Sell cabbages!!", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndPlaceSellOrder((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    cabbages, 10, 10, null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Deposit cabbages in exchange!!", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDepositGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    cabbages, 10, null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Withdraw cabbages from the exchange!!", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndWithdrawGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    cabbages, 10, null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Deposit soil and cabbage seeds", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDepositGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(),
                                    seeds, 10, null));
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDepositGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(),
                                    earth, 10, null));
                        }));

                        currMenu.Items.Add(new ContextMenuItem("Withdraw cabbages from the station", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndWithdrawGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(),
                                    cabbages, 10, null));
                        }));

                    }

                    currMenu.Open(ms.X, ms.Y);

                    sectorScreen.ContextMenus.Add(currMenu);
                }
                else if (ms.LeftButton == ButtonState.Pressed)
                {
                    if (mouseOverBody is Gate)
                    {
                        sectorScreen.CurrentSector = ((Gate)mouseOverBody).Destination.Sector;
                    }
                }
            };
        }

        protected void SetupTestSector()
        {

            universe = new Universe();

            Sector testSector1 = universe.AddSector("Sector 1");
            Sector testSector2 = universe.AddSector("Sector 2");

            cabbageProd = new ProductionRule(new Dictionary<Good, int>() { {earth, 1}, {seeds, 1} }, new Dictionary<Good, int>() { {cabbages, 1} }, 1.0f);

            universe.JoinSectors(testSector1, testSector2, new Vector3(30, 0, 0), new Vector3(-30, 0, 0));

            sectorScreen.CurrentSector = testSector2;

            ProductionRule dummy = new ProductionRule(new Dictionary<Good, int>(), new Dictionary<Good, int>(), 9000.0f);

            for (int i = 0; i < 5; i++)
            {
                //TODO: production station has no owner
                //TODO: sector has AddShips for ships and have to use the List object to add gates and stations
                ProductionStation testStation = universe.AddProductionStation("Station " + i, testSector1,
                    RandomVector(10.0f), universe.GetPlayer(), dummy, 100);
            }

            Owner cabbageBuyer = universe.AddOwner();

            ProductionStation destStation = universe.AddProductionStation("Cabbage Farm", testSector2,
                Vector3.Zero, universe.GetPlayer(), cabbageProd, 100);
            Exchange exchange = universe.AddExchange("Space Exchange", testSector2, RandomVector(30.0f));

            for (int i = 0; i < 10; i++)
            {
                Ship testShip = universe.AddShip("Ship #" + i, testSector1, RandomVector(20.0f), universe.GetPlayer(), 1.0f);
                testShip.Velocity = RandomVector(0.5f);
            }

            // Test scenario:
            // * Cabbage farm supplier deposits earth and seeds to the cabbage farm
            // * Cabbage farm turns it into cabbages
            // * Cabbage farm seller takes the cabbages, brings them to the exchange and places a sell order
            // * Cabbage buyer places a buy order in the exchange
            // * The orders are matched
            // * Cabbage buyer withdraws the cabbages from the exchange

            universe.AddShip("Exchange buyer ship", testSector2, exchange.Location.Coordinates + RandomVector(3.0f), cabbageBuyer, 1.0f);
            
            Ship sellerShip = universe.AddShip("Exchange seller ship", testSector2, exchange.Location.Coordinates + RandomVector(3.0f), universe.GetPlayer(), 1.0f);

            Ship supplierShip = universe.AddShip("Cabbage farm supplier ship", testSector2, exchange.Location.Coordinates + RandomVector(3.0f),
                universe.GetPlayer(), 1.0f);
            supplierShip.Cargo.AddItem(earth, 10);
            supplierShip.Cargo.AddItem(seeds, 10);

            //Need to update the sector for the additions to propagate to the actual bodies' list

            testSector1.ForcePropagateChanges();

            universe.DiscoverGate(universe.GetPlayer(), testSector1.Contents.OfType<Gate>().First());
            cabbageBuyer.Balance = 200;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Arial");
            TextureProvider.LoadTextures(Content);

            SetupUI();
            SetupTestSector(); //Initialize the test sector here because we only here have access to the textures
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            universe.Update(timeDifference);

            sectorScreen.ProcessInput(gameTime, Mouse.GetState());

            base.Update(gameTime);
        }

        private Vector3 RandomVector(float max)
        {
            return new Vector3((float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f)) * max;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            sectorScreen.Render(spriteBatch);

            //Restore the state changed by the SpriteBatch
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);
        }
    }
}