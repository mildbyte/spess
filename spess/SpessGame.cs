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
        Good metal = new Good("Metal", "I am a metal fan", 2);
        Good crystals = new Good("Crystals", "Crystals!", 1);
        Good lasergun = new Good("Laser gun", "firing mah etc", 1);
        Good cabbageAmmo = new Good("Cabbage charges", "Used in laser guns", 1);

        ProductionRule cabbageProd;
        ProductionRule lasergunProd;
        ProductionRule metalProd;
        ProductionRule crystalProd;
        ProductionRule cabbageAmmoProd;

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
                        currMenu.Items.Add(new ContextMenuItem("Sell soil and cabbage seeds", delegate()
                        {
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDepositGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    seeds, 10, null));
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndDepositGoods((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    earth, 10, null));
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndPlaceSellOrder((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    seeds, 10, 10, null));
                            ((Ship)mouseOverBody).GoalQueue.AddGoal(
                                new AI.MoveAndPlaceSellOrder((Ship)mouseOverBody,
                                    mouseOverBody.Universe.Sectors[1].Contents.OfType<Exchange>().First(),
                                    earth, 10, 10, null));
                        }));
                        currMenu.Items.Add(new ContextMenuItem("Stop all orders", delegate()
                        {
                            (mouseOverBody as Ship).GoalQueue.CancelAllOrders();
                            (mouseOverBody as Ship).GoalQueue.AddGoal(
                                new AI.Stop((mouseOverBody as Ship), null));
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

            Sector[,] sectors = new Sector[5,5];

            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    sectors[i, j] = universe.AddSector("Sector (" + i + ", " + j + ")");
                    sectors[i, j].Dimensions = new BoundingBox(new Vector3(-30, -30, -30), new Vector3(30, 30, 30));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    universe.JoinSectors(sectors[i, j], sectors[i + 1, j], new Vector3(30, 0, 0), new Vector3(-30, 0, 0));
                    universe.JoinSectors(sectors[i, j], sectors[i, j + 1], new Vector3(0, 0, 30), new Vector3(0, 0, -30));
                }
            }

            cabbageProd = new ProductionRule(new Dictionary<Good, int>() { {earth, 1}, {seeds, 1} }, new Dictionary<Good, int>() { {cabbages, 1} }, 1.0f);
            cabbageAmmoProd = new ProductionRule(new Dictionary<Good, int>() { { cabbages, 1 } }, new Dictionary<Good, int>() { { cabbageAmmo, 1 } }, 10.0f);
            crystalProd = new ProductionRule(new Dictionary<Good, int>(), new Dictionary<Good, int>() { { crystals, 1 } }, 10.0f);
            lasergunProd = new ProductionRule(new Dictionary<Good, int>() { { metal, 1 }, { crystals, 2 } }, new Dictionary<Good, int>() { { lasergun, 1 } }, 15.0f);
            metalProd = new ProductionRule(new Dictionary<Good, int>(), new Dictionary<Good, int>() { { metal, 1 } }, 10.0f);

            Owner cabbageFarmer = universe.AddOwner();
            Owner metalMiner = universe.AddOwner();
            Owner crystalMiner = universe.AddOwner();
            Owner gunMaker = universe.AddOwner();
            Owner cabbageAmmoMaker = universe.AddOwner();

            universe.AddProductionStation("Cabbage Farm", sectors[0, 1], RandomVector(10.0f), cabbageFarmer, cabbageProd, 20);
            universe.AddProductionStation("Crystal Mine", sectors[0, 1], RandomVector(10.0f), crystalMiner, crystalProd, 20);
            universe.AddProductionStation("Metal Mine", sectors[0, 1], RandomVector(10.0f), metalMiner, metalProd, 20);
            universe.AddProductionStation("Lasgun Factory", sectors[0, 1], RandomVector(10.0f), gunMaker, lasergunProd, 20);
            universe.AddProductionStation("Lasgun Ammo Foundry", sectors[0, 1], RandomVector(10.0f), cabbageAmmoMaker, cabbageAmmoProd, 20);

            sectorScreen.CurrentSector = sectors[0, 1];

            Exchange exchange = universe.AddExchange("Space Exchange", sectors[0, 1], RandomVector(20.0f));

            Ship supplierShip = universe.AddShip("Seeds and Earth seller ship", sectors[0, 1], exchange.Location.Coordinates + RandomVector(3.0f),
                universe.GetPlayer(), 1.0f);

            supplierShip.Cargo.AddItem(earth, 10);
            supplierShip.Cargo.AddItem(seeds, 10);

            AIShip stationSupplierShip = universe.AddAIShip("Cabbage Farm supplier", sectors[0, 1], exchange.Location.Coordinates + RandomVector(10.0f),
                cabbageFarmer, 5.0f);
            stationSupplierShip.Role = AIShipRole.Supplier;
            AIShip metalMinerShip = universe.AddAIShip("Metal Mine supplier", sectors[0, 1], exchange.Location.Coordinates + RandomVector(10.0f),
                metalMiner, 5.0f);
            metalMinerShip.Role = AIShipRole.Supplier;
            AIShip crystalMinerShip = universe.AddAIShip("Crystal Mine supplier", sectors[0, 1], exchange.Location.Coordinates + RandomVector(10.0f),
                crystalMiner, 5.0f);
            crystalMinerShip.Role = AIShipRole.Supplier;
            AIShip lasgunAmmoShip = universe.AddAIShip("Lasgun Ammo Foundry supplier", sectors[0, 1], exchange.Location.Coordinates + RandomVector(10.0f),
                cabbageAmmoMaker, 5.0f);
            lasgunAmmoShip.Role = AIShipRole.Supplier;
            AIShip lasgunShip = universe.AddAIShip("Lasgun Factory supplier", sectors[0, 1], exchange.Location.Coordinates + RandomVector(10.0f),
                gunMaker, 5.0f);
            lasgunShip.Role = AIShipRole.Supplier;

            cabbageFarmer.Balance = 200;
            metalMiner.Balance = 200;
            crystalMiner.Balance = 200;
            gunMaker.Balance = 200;
            cabbageAmmoMaker.Balance = 200;
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