#region Using Statements
using System;
using System.Collections.Generic;
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
        BasicEffect basicEffect;
        Camera camera;
        Matrix perspProjectionMatrix;
        Matrix orthoProjectionMatrix;

        double timePassed = 0;
        int fps = 0;
        int totalFrames = 0;

        Texture2D shipTex;
        Texture2D gateTex;
        Texture2D stationTex;

        Owner testOwner;
        Sector testSector;

        Random rand = new Random();

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
            testOwner = new Owner();
            testSector = new Sector();

            for (int i = 0; i < 10; i++)
            {
                Ship testShip = new Ship(i.ToString(), new Location(testSector, RandomVector(20.0f)), testOwner, 10.0);
                testShip.Velocity = RandomVector(1.0f);
                testSector.AddShip(testShip);
            }

            for (int i = 0; i < 5; i++)
            {
                ProductionStation testStation = new ProductionStation(i.ToString(), new Location(testSector, RandomVector(30.0f)), null, 100);
                testSector.Stations.Add(testStation);
            }

            testSector.Gates.Add(new Gate(new Location(testSector, new Vector3(-30, 0, -30)), null));
            testSector.Gates.Add(new Gate(new Location(testSector, new Vector3(-30, 0, 30)), null));
            testSector.Gates.Add(new Gate(new Location(testSector, new Vector3(30, 0, -30)), null));
            testSector.Gates.Add(new Gate(new Location(testSector, new Vector3(30, 0, 30)), null));

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 760;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Arial");
            shipTex = Content.Load<Texture2D>("ship");
            gateTex = Content.Load<Texture2D>("gate");
            stationTex = Content.Load<Texture2D>("station");

            perspProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 100.0f);
            orthoProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1);

            basicEffect = new BasicEffect(GraphicsDevice);

            camera = new Camera(GraphicsDevice, this);

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

            foreach (Ship s in testSector.Ships) {
                s.Update(timeDifference);
            }

            timePassed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timePassed >= 1000.0)
            {
                fps = totalFrames;
                totalFrames = 0;
                timePassed = 0;
                Window.Title = "spess (" + fps + " FPS)";
            }

            base.Update(gameTime);
        }

        private Vector3 RandomVector(float max)
        {
            return new Vector3((float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f),
                (float)(rand.NextDouble() - 0.5f)) * max;
        }

        /// <summary>
        /// Draws a grid at y = 0 in the xz plane
        /// </summary>
        void DrawGrid(Color color)
        {
            BasicEffect gridEffect = new BasicEffect(GraphicsDevice)
            {
                World = Matrix.Identity,
                View = camera.ViewMatrix,
                Projection = perspProjectionMatrix,
                VertexColorEnabled = true,
            };

            VertexPositionColor[] vertices = new VertexPositionColor[201 * 2 * 2];

            int i = 0;
            for (int ix = -100; ix <= 100; ix += 10) {
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, -100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, 100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(-100, 0, ix), color);
                vertices[i++] = new VertexPositionColor(new Vector3(100, 0, ix), color);
            }

            foreach (EffectPass pass in gridEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 201 * 2);
            }
        }

        //TODO: generalize rendering.

        void RenderShips(List<Ship> ships)
        {
            int shipCount = ships.Count;
            VertexPositionTexture[] vertices = new VertexPositionTexture[shipCount * 4];

            int currVertex = 0;
            foreach (Ship s in ships)
            {
                Vector3 shipCoords = GraphicsDevice.Viewport.Project(s.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

                float halfSide = 24.0f;

                vertices[currVertex++] = new VertexPositionTexture(shipCoords + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                vertices[currVertex++] = new VertexPositionTexture(shipCoords + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                vertices[currVertex++] = new VertexPositionTexture(shipCoords + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                vertices[currVertex++] = new VertexPositionTexture(shipCoords + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            basicEffect.World = Matrix.Identity;
            basicEffect.Projection = orthoProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = shipTex;

            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);
            basicEffect.View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            for (int i = 0; i < shipCount; i++)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        void RenderStations(List<ProductionStation> stations)
        {
            int stationCount = stations.Count;
            VertexPositionTexture[] vertices = new VertexPositionTexture[stationCount * 4];

            int currVertex = 0;
            foreach (ProductionStation s in stations)
            {
                Vector3 stationCoords = GraphicsDevice.Viewport.Project(s.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

                float halfSide = 24.0f;

                vertices[currVertex++] = new VertexPositionTexture(stationCoords + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                vertices[currVertex++] = new VertexPositionTexture(stationCoords + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                vertices[currVertex++] = new VertexPositionTexture(stationCoords + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                vertices[currVertex++] = new VertexPositionTexture(stationCoords + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            basicEffect.World = Matrix.Identity;
            basicEffect.Projection = orthoProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = stationTex;

            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);
            basicEffect.View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            for (int i = 0; i < stationCount; i++)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        void RenderGates(List<Gate> gates)
        {
            int gateCount = gates.Count;
            VertexPositionTexture[] vertices = new VertexPositionTexture[gateCount * 4];

            int currVertex = 0;
            foreach (Gate g in gates)
            {
                Vector3 gateCoords = GraphicsDevice.Viewport.Project(g.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

                float halfSide = 24.0f;

                vertices[currVertex++] = new VertexPositionTexture(gateCoords + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                vertices[currVertex++] = new VertexPositionTexture(gateCoords + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                vertices[currVertex++] = new VertexPositionTexture(gateCoords + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                vertices[currVertex++] = new VertexPositionTexture(gateCoords + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            basicEffect.World = Matrix.Identity;
            basicEffect.Projection = orthoProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = gateTex;

            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);
            basicEffect.View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            for (int i = 0; i < gateCount; i++)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }
        

        void RenderSector(Sector sector)
        {
            RenderShips(sector.Ships);
            RenderStations(sector.Stations);
            RenderGates(sector.Gates);
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
            RenderSector(testSector);
            base.Draw(gameTime);
        }
    }
}