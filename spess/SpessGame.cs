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
            // TODO: Add your initialization logic here

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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Owner testOwner = new Owner();
            Sector testSector = new Sector();
            Ship testShip = new Ship("SHIP!!1", new Location(testSector, new Vector3(0, 0, 0)), testOwner, 10.0);
            testSector.AddShip(testShip);

            Vector3 center = testShip.Location.Coordinates;

            VertexPositionTexture[] vertices = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(0,1,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(1,0,0),
                                new Vector2(1,0))
                        };

            basicEffect.World = Matrix.CreateConstrainedBillboard(new Vector3(0.5f, 0.5f, 0.0f), camera.Position, Vector3.Up, null, null);
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = shipTex;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}