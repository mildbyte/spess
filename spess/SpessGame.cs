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
        Matrix projectionMatrix;

        Texture2D shipTex;

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

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);
            //projectionMatrix = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width/10, GraphicsDevice.Viewport.Height/10, 0.3f, 1000.0f);

            basicEffect = new BasicEffect(GraphicsDevice);

            camera = new Camera(GraphicsDevice);

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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int shipCount = testSector.Ships.Count;
            VertexPositionTexture[] vertices = new VertexPositionTexture[shipCount * 4];

            int currVertex = 0;
            foreach (Ship s in testSector.Ships)
            {
                Vector3 center = s.Location.Coordinates;

                vertices[currVertex++] = new VertexPositionTexture(center + new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1));
                vertices[currVertex++] = new VertexPositionTexture(center + new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0));
                vertices[currVertex++] = new VertexPositionTexture(center + new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1));
                vertices[currVertex++] = new VertexPositionTexture(center + new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0));
            }

            basicEffect.World = Matrix.Identity;
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = shipTex;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;


            for (int i = 0; i < shipCount; i++)
            {
                //basicEffect.World = Matrix.CreateConstrainedBillboard(testSector.Ships[i].Location.Coordinates, camera.Position, Vector3.Up, null, null);
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, i * 4, 2);
                }
            }

            GraphicsDevice.BlendState = BlendState.Opaque;

            base.Draw(gameTime);
        }
    }
}