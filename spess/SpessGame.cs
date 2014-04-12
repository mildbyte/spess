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
        SpriteFont font;
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
            font = Content.Load<SpriteFont>("Arial");
            shipTex = Content.Load<Texture2D>("ship");
            gateTex = Content.Load<Texture2D>("gate");
            stationTex = Content.Load<Texture2D>("station");

            perspProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 100.0f);
            orthoProjectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 1);

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

        void BatchDrawIcons(IEnumerable<Vector3> coords, int iconCount, Matrix projectionMatrix, Matrix viewMatrix, Matrix worldMatrix, Texture2D texture, float size)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[iconCount * 4];

            int currVertex = 0;
            float halfSide = size * 0.5f;
            foreach (Vector3 v in coords)
            {
                Vector3 vProj = GraphicsDevice.Viewport.Project(v, projectionMatrix, viewMatrix, worldMatrix);

                vertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                vertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                vertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                vertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);

            BasicEffect basicEffect = new BasicEffect(GraphicsDevice)
            {
                World = Matrix.Identity,
                Projection = orthoProjectionMatrix,
                TextureEnabled = true,
                Texture = texture,
                View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0)),
            };

            for (int i = 0; i < iconCount; i++)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        void RenderShips(List<Ship> ships)
        {
            BatchDrawIcons(ships.Select(s => s.Location.Coordinates), ships.Count, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, shipTex, 48);
        }

        void RenderStations(List<ProductionStation> stations)
        {
            BatchDrawIcons(stations.Select(s => s.Location.Coordinates), stations.Count, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, stationTex, 48);
        }

        void RenderGates(List<Gate> gates)
        {
            BatchDrawIcons(gates.Select(g => g.Location.Coordinates), gates.Count, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, gateTex, 48);
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