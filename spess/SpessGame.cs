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
        Camera camera;
        Matrix perspProjectionMatrix;
        Matrix orthoProjectionMatrix;

        BasicEffect gridEffect;
        BasicEffect iconEffect;

        double timePassed = 0;
        int fps = 0;
        int totalFrames = 0;

        Universe universe;

        Sector currSector;

        Random rand = new Random();

        FloatingLabel currLabel = null;
        ContextMenu currMenu = null;

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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Arial");
            TextureProvider.LoadTextures(Content);

            camera = new Camera(GraphicsDevice, this);

            perspProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 200.0f);
            orthoProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1);

            gridEffect = new BasicEffect(GraphicsDevice)
            {
                World = Matrix.Identity,
                Projection = perspProjectionMatrix,
                VertexColorEnabled = true,
            };

            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);
            iconEffect = new BasicEffect(GraphicsDevice)
            {
                World = Matrix.Identity,
                Projection = orthoProjectionMatrix,
                TextureEnabled = true,
                VertexColorEnabled = false,
                View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0)),
            };

            //Initialize the test sector here because we only here have access to the textures
            universe = new Universe();

            Sector testSector1 = universe.AddSector("Sector 1");
            Sector testSector2 = universe.AddSector("Sector 2");

            universe.JoinSectors(testSector1, testSector2, new Vector3(30, 0, 0), new Vector3(-30, 0, 0));

            currSector = testSector2;

            ProductionRule dummy = new ProductionRule(new Dictionary<Good, int>(), new Dictionary<Good, int>(), 9000.0f);

            for (int i = 0; i < 5; i++)
            {
                //TODO: production station has no owner
                //TODO: sector has AddShips for ships and have to use the List object to add gates and stations
                ProductionStation testStation = universe.AddProductionStation("Station " + i, testSector1, RandomVector(10.0f), dummy, 100);
            }

            ProductionStation destStation = universe.AddProductionStation("Grand Central Station", testSector2, Vector3.Zero, dummy, 100);
            universe.AddExchange("Exchange", testSector2, RandomVector(30.0f));

            for (int i = 0; i < 10; i++)
            {
                Ship testShip = universe.AddShip("Ship #" + i, testSector1, RandomVector(20.0f), universe.GetPlayer(), 1.0f);
                testShip.Velocity = RandomVector(0.5f);
            }

            //Need to update the sector for the additions to propagate to the actual bodies' list

            testSector1.ForcePropagateChanges();

            universe.DiscoverGate(universe.GetPlayer(), testSector1.Contents.OfType<Gate>().First());
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
            camera.ProcessInput(timeDifference);

            universe.Update(timeDifference);

            timePassed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timePassed >= 1000.0)
            {
                fps = totalFrames;
                totalFrames = 0;
                timePassed = 0;
                Window.Title = "spess (" + fps + " FPS)";
            }

            MouseState ms = Mouse.GetState();

            Vector2 mousePos = new Vector2(ms.X, ms.Y);

            if (currMenu != null) currMenu.NotifyMouseStateChange(ms);


            SpaceBody mouseOverBody = PickBody(mousePos, currSector);

            if (mouseOverBody != null)
            {
                currLabel = new FloatingLabel(mouseOverBody.ToString(), mousePos);
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    if (mouseOverBody is Gate)
                    {
                        currSector = ((Gate)mouseOverBody).Destination.Sector;
                    }
                    else if (mouseOverBody is Ship)
                    {
                        ((Ship)mouseOverBody).GoalQueue.AddGoal(
                            new AI.MoveAndDockAt((Ship)mouseOverBody, mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(), null));
                    }
                }

                if (ms.RightButton == ButtonState.Pressed)
                {
                    currMenu = new ContextMenu(font);
                    currMenu.Items.Add(new ContextMenuItem("Item 1", delegate() { mouseOverBody.Location.Coordinates = RandomVector(10.0f); }));
                    currMenu.Items.Add(new ContextMenuItem("Item 2", null));
                    currMenu.Items.Add(new ContextMenuItem("Item 3", null));
                    currMenu.Items.Add(new ContextMenuItem("Item 4", null));

                    currMenu.Open(ms.X, ms.Y);
                }
            }
            else currLabel = null;

            base.Update(gameTime);
        }

        private Vector3 RandomVector(float max)
        {
            return new Vector3((float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f)) * max;
        }

        private SpaceBody PickBody(Vector2 mousePos, Sector sector) {
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mousePos, 0), orthoProjectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mousePos, 1), orthoProjectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            Ray ray = new Ray(nearPoint, direction);

            SpaceBody closestBody = null;
            float closestBodyDist = 0.0f;

            foreach (SpaceBody currBody in sector.Contents)
            {
                BoundingBox bb = GetIconQuadBBox(currBody);;

                float? interDist = ray.Intersects(bb);
                if (interDist == null) continue;
                if (closestBody == null || closestBodyDist > interDist)
                {
                    closestBody = currBody;
                    closestBodyDist = (float)interDist;
                }
            }

            return closestBody;
        }

        /// <summary>
        /// Draws a grid at y = 0 in the xz plane
        /// </summary>
        void DrawGrid(Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[201 * 2 * 2];

            int i = 0;
            for (int ix = -100; ix <= 100; ix += 10) {
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, -100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, 100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(-100, 0, ix), color);
                vertices[i++] = new VertexPositionColor(new Vector3(100, 0, ix), color);
            }

            gridEffect.View = camera.ViewMatrix;

            foreach (EffectPass pass in gridEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 201 * 2);
            }
        }

        BoundingBox GetIconQuadBBox(SpaceBody body) {
            float halfSide = body.IconSize * 0.5f;

            Vector3 diagVector = new Vector3(halfSide, halfSide, 0);

            Vector3 vProj = GraphicsDevice.Viewport.Project(body.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            return new BoundingBox(vProj - diagVector, vProj + diagVector);
        }

        void BatchDrawIcons(Matrix projectionMatrix, Matrix viewMatrix, Matrix worldMatrix, Sector sector)
        {
            VertexPositionTexture[] iconVertices = new VertexPositionTexture[sector.Contents.Count * 4];

            int currVertex = 0;
            foreach (SpaceBody item in sector.Contents)
            {
                float halfSide = item.IconSize * 0.5f;
                Vector3 vProj = GraphicsDevice.Viewport.Project(item.Location.Coordinates, projectionMatrix, viewMatrix, worldMatrix);

                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            for (int i = 0; i < sector.Contents.Count; i++)
            {
                iconEffect.Texture = sector.Contents[i].IconTexture;
                foreach (EffectPass pass in iconEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, iconVertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }
        
        void RenderSector(Sector sector)
        {
            BatchDrawIcons(perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, currSector);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            totalFrames++;
            DrawGrid(Color.White);
            RenderSector(currSector);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FPS: " + fps, new Vector2(10, 10), Color.White);
            spriteBatch.End();

            if (currLabel != null) currLabel.Render(spriteBatch, font, TextureProvider.dialogTex);
            if (currMenu != null) currMenu.Render(spriteBatch, TextureProvider.dialogTex);

            //Restore the state changed by the SpriteBatch
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);
        }
    }
}