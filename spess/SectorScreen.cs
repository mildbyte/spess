using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace spess.UI
{
    public delegate void IconClickDelegate(SpaceBody spaceBody, MouseState mouseState);
    public delegate void IconMouseoverDelegate(SpaceBody spaceBody, MouseState mouseState);
    public delegate void IconMouseEnterDelegate(SpaceBody spaceBody, MouseState mouseState);
    public delegate void IconMouseLeaveDelegate(SpaceBody spaceBody, MouseState mouseState);

    class SectorScreen
    {
        List<ContextMenu> contextMenus;
        List<FloatingLabel> floatingLabels;
        Sector displayedSector;

        Camera camera;
        Matrix perspProjectionMatrix;
        Matrix orthoProjectionMatrix;
        Skybox skybox;

        BasicEffect gridEffect;
        BasicEffect iconEffect;

        GraphicsDevice graphicsDevice;
        Game game;

        double timePassed = 0;
        int fps = 0;
        int totalFrames = 0;

        bool mouseClickRegistered = false;
        SpaceBody mouseOverBody = null;

        public List<ContextMenu> ContextMenus { get { return contextMenus; } }
        public List<FloatingLabel> FloatingLabels { get { return floatingLabels; } }
        public Sector CurrentSector { get { return displayedSector; } set { displayedSector = value; } }
        public SpriteFont Font { get; set; }
        public IconClickDelegate OnIconClicked;
        public IconMouseoverDelegate OnIconMouseover;
        public IconMouseEnterDelegate OnIconMouseEnter;
        public IconMouseLeaveDelegate OnIconMouseLeave;
        public Camera Camera { get { return camera; } }

        public SectorScreen(GraphicsDevice graphicsDevice, Game game)
        {
            this.graphicsDevice = graphicsDevice;
            this.game = game;

            contextMenus = new List<ContextMenu>();
            floatingLabels = new List<FloatingLabel>();

            camera = new Camera(graphicsDevice, game);
            skybox = new Skybox(game.Content);

            perspProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphicsDevice.Viewport.AspectRatio, 0.3f, 300.0f);
            orthoProjectionMatrix = Matrix.CreateOrthographic(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 1);

            gridEffect = new BasicEffect(graphicsDevice)
            {
                World = Matrix.Identity,
                Projection = perspProjectionMatrix,
                VertexColorEnabled = true,
            };

            Vector2 center = new Vector2(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f);
            iconEffect = new BasicEffect(graphicsDevice)
            {
                World = Matrix.Identity,
                Projection = orthoProjectionMatrix,
                TextureEnabled = true,
                VertexColorEnabled = false,
                View = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0)),
            };
        }

        /// <summary>
        /// Returns the body that the mouse is pointing at
        /// </summary>
        /// <param name="mousePos">Mouse coordinates</param>
        /// <param name="sector">The sector in which we're looking</param>
        /// <returns></returns>
        private SpaceBody PickBody(Vector2 mousePos, Sector sector)
        {
            // Create a ray from the screen into the world
            Vector2 center = new Vector2(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f);
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(new Vector3(mousePos, 0), orthoProjectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(new Vector3(mousePos, 1), orthoProjectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            Ray ray = new Ray(nearPoint, direction);

            // Find the closest body that the ray hits.
            SpaceBody closestBody = null;
            float closestBodyDist = 0.0f;

            foreach (SpaceBody currBody in sector.Contents)
            {
                BoundingBox bb = GetIconQuadBBox(currBody); ;

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
        /// Projects a body position to the screen and surrounds the icon with a bounding box
        /// (for object picking)
        /// </summary>
        /// <param name="body">Space body to get the BBox of</param>
        /// <returns>The bounding box (with zero depth) that contains the icon.</returns>
        BoundingBox GetIconQuadBBox(SpaceBody body)
        {
            float halfSide = body.IconSize * 0.5f;

            Vector3 diagVector = new Vector3(halfSide, halfSide, 0);

            Vector3 vProj = graphicsDevice.Viewport.Project(body.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            return new BoundingBox(vProj - diagVector, vProj + diagVector);
        }

        /// <summary>
        /// Draws all the space bodies currently in the sector as screenspace icons (constant scale)
        /// </summary>
        /// <param name="projectionMatrix">Perspective projection matrix used by the UI</param>
        /// <param name="viewMatrix">Camera view matrix</param>
        /// <param name="worldMatrix">Model-view matrix</param>
        /// <param name="sector">Sector to render the objects from</param>
        void BatchDrawIcons(Matrix projectionMatrix, Matrix viewMatrix, Matrix worldMatrix, Sector sector)
        {
            // TODO: solve the issue with multiple icons close to each other blinking and being difficult
            // /impossible to select

            //Create an array of quad vertices representing icons.
            VertexPositionTexture[] iconVertices = new VertexPositionTexture[sector.Contents.Count * 4];

            int currVertex = 0;
            foreach (SpaceBody item in sector.Contents)
            {
                float halfSide = item.IconSize * 0.5f;

                // Project the space body location into the screen space and draw a quad around it
                Vector3 vProj = graphicsDevice.Viewport.Project(item.Location.Coordinates, projectionMatrix, viewMatrix, worldMatrix);

                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }


            // Render every quad in orthographic projection (size stays constant)
            for (int i = 0; i < sector.Contents.Count; i++)
            {
                iconEffect.Texture = sector.Contents[i].IconTexture;
                foreach (EffectPass pass in iconEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, iconVertices, i * 4, 2);
                }
            }
        }

        /// <summary>
        /// Draws a grid at y = 0 in the xz plane
        /// </summary>
        void DrawGrid(Color color)
        {
            // Array of lines representing the grid
            VertexPositionColor[] vertices = new VertexPositionColor[201 * 2 * 2];

            int i = 0;
            for (int ix = -100; ix <= 100; ix += 10)
            {
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, -100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(ix, 0, 100), color);
                vertices[i++] = new VertexPositionColor(new Vector3(-100, 0, ix), color);
                vertices[i++] = new VertexPositionColor(new Vector3(100, 0, ix), color);
            }

            gridEffect.View = camera.ViewMatrix;

            // Render the lines
            foreach (EffectPass pass in gridEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 201 * 2);
            }
        }
        
        /// <summary>
        /// Renders the sector using the current UI settings.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing UI elements</param>
        public void Render(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);

            RasterizerState originalRasterizerState = graphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(camera.ViewMatrix, perspProjectionMatrix, camera.Position);
            graphicsDevice.RasterizerState = originalRasterizerState;

            totalFrames++;
            DrawGrid(Color.White);
            BatchDrawIcons(perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, displayedSector);

            contextMenus.ForEach(m => m.Render(spriteBatch, TextureProvider.dialogTex));
            floatingLabels.ForEach(l => l.Render(spriteBatch, TextureProvider.dialogTex));

            spriteBatch.Begin();
            spriteBatch.DrawString(Font, "FPS: " + fps, new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Processes the input changes that occurred between frames
        /// </summary>
        /// <param name="gameTime">GameTime parameter passed by MonoGame to the Update method in the Game class</param>
        /// <param name="mouseState">Mouse state</param>
        public void ProcessInput(GameTime gameTime, MouseState mouseState)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            camera.ProcessInput(timeDifference);

            timePassed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timePassed >= 1000.0)
            {
                fps = totalFrames;
                totalFrames = 0;
                timePassed = 0;
            }

            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            SpaceBody newMouseOverBody = PickBody(mousePos, displayedSector);

            // Update the open context menus. Remove those that closed themselves.
            contextMenus.ForEach(m => m.NotifyMouseStateChange(mouseState));
            contextMenus.RemoveAll(m => !m.IsOpen);

            // Call the relevant events
            if (newMouseOverBody == null && mouseOverBody != null) {
                OnIconMouseLeave(mouseOverBody, mouseState);
            } else if (newMouseOverBody != null && mouseOverBody == null) {
                OnIconMouseEnter(newMouseOverBody, mouseState);
                OnIconMouseover(newMouseOverBody, mouseState);
            }
            else if (newMouseOverBody != null && mouseOverBody != null && newMouseOverBody != mouseOverBody)
            {
                // Mouse moved from a body to another body without hitting the void, need to send all events
                OnIconMouseLeave(mouseOverBody, mouseState);
                OnIconMouseEnter(newMouseOverBody, mouseState);
                OnIconMouseover(newMouseOverBody, mouseState);
            }
            else if (newMouseOverBody != null)
            {
                // General mouseover events.
                OnIconMouseover(newMouseOverBody, mouseState);

                // Click event dispatch with flags to ensure only one click is registered.
                // If a context menu is open, the click belonged to it and we don't pass it to the icon
                if (!mouseClickRegistered && !contextMenus.Any() &&
                    (mouseState.LeftButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed))
                {
                    mouseClickRegistered = true;
                    OnIconClicked(mouseOverBody, mouseState);
                }
                else if (mouseClickRegistered)
                {
                    mouseClickRegistered = false;
                }
            }

            mouseOverBody = newMouseOverBody;
        }
    }
}
