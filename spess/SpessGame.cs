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

        Vector3 cameraPosition = new Vector3(0, 0, 0);
        float leftrightRot = MathHelper.PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 30.0f;
        MouseState originalMouseState;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                UpdateViewMatrix();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);
            AddToCameraPosition(moveVector * amount);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

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

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);
            UpdateViewMatrix();

            basicEffect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };

            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();
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

            // TODO: Add your update logic here

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            ProcessInput(timeDifference);
 

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //Owner testOwner = new Owner();
            //Sector testSector = new Sector();
            //Ship testShip = new Ship("SHIP!!1", new Location(testSector, new Vector3(0, 0, 0)), testOwner, 10.0);
            //testSector.AddShip(testShip);

            //VertexPositionTexture[] vertices = new VertexPositionTexture[6];
            //int i = 0;

            //Vector3 center = testShip.Location.Coordinates;

            //vertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
            //vertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
            //vertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));
            //vertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
            //vertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
            //vertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));

            Vector3 textPosition = new Vector3(0, 0, 10);
            
            basicEffect.World = Matrix.CreateConstrainedBillboard(textPosition, cameraPosition, Vector3.Up, null, null);
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            const string message = "hello, world!";
            Vector2 textOrigin = font.MeasureString(message) / 2;
            const float textSize = 0.75f;

            spriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);
            spriteBatch.DrawString(font, message, Vector2.Zero, Color.White, 0, textOrigin, textSize, 0, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
