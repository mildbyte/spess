using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace spess
{
    class Camera
    {
        Vector3 cameraPosition = new Vector3(0, 0, 0);
        float leftrightRot = MathHelper.PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        GraphicsDevice graphics;
        Matrix viewMatrix;
        bool isFreeLookMode = false;
        Game game;
        
        MouseState originalMouseState;

        public float RotationSpeed { get; set; }
        public float MoveSpeed { get; set; }

        public Matrix ViewMatrix { get { return viewMatrix; } }
        public Vector3 Position { get { return cameraPosition; } }

        public Camera(GraphicsDevice graphics, Game game)
        {
            this.graphics = graphics;
            this.game = game;
            RotationSpeed = 0.3f;
            MoveSpeed = 20.0f;

            Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();
            game.IsMouseVisible = true;
        }

        public void ProcessInput(float amount)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Space))
            {
                if (!isFreeLookMode)
                {
                    isFreeLookMode = true;
                    Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
                    game.IsMouseVisible = false;
                }    

                MouseState currentMouseState = Mouse.GetState();
                if (currentMouseState != originalMouseState)
                {
                    float xDifference = currentMouseState.X - originalMouseState.X;
                    float yDifference = currentMouseState.Y - originalMouseState.Y;
                    leftrightRot -= RotationSpeed * xDifference * amount;
                    updownRot -= RotationSpeed * yDifference * amount;
                    Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
                    UpdateViewMatrix();
                }
            }
            else
            {
                if (isFreeLookMode)
                {
                    isFreeLookMode = false;
                    game.IsMouseVisible = true;
                }
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
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
            cameraPosition += MoveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

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


    }
}
