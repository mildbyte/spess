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
    class SectorScreen
    {
        List<ContextMenu> contextMenus;
        List<FloatingLabel> floatingLabels;
        Sector displayedSector;

        Camera camera;
        Matrix perspProjectionMatrix;
        Matrix orthoProjectionMatrix;

        BasicEffect gridEffect;
        BasicEffect iconEffect;

        GraphicsDevice graphicsDevice;
        Game game;

        double timePassed = 0;
        int fps = 0;
        int totalFrames = 0;

        FloatingLabel currLabel = null;
        ContextMenu currMenu = null;

        public List<ContextMenu> ContextMenus { get { return contextMenus; } }
        public List<FloatingLabel> FloatingLabels { get { return floatingLabels; } }
        public Sector CurrentSector { get { return displayedSector; } set { displayedSector = value; } }
        public SpriteFont Font { get; set; }


        public SectorScreen(GraphicsDevice graphicsDevice, Game game)
        {
            this.graphicsDevice = graphicsDevice;
            this.game = game;

            contextMenus = new List<ContextMenu>();
            floatingLabels = new List<FloatingLabel>();

            camera = new Camera(graphicsDevice, game);

            perspProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphicsDevice.Viewport.AspectRatio, 0.3f, 200.0f);
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


        private SpaceBody PickBody(Vector2 mousePos, Sector sector)
        {
            Vector2 center = new Vector2(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f);
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

            Vector3 nearPoint = graphicsDevice.Viewport.Unproject(new Vector3(mousePos, 0), orthoProjectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = graphicsDevice.Viewport.Unproject(new Vector3(mousePos, 1), orthoProjectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            Ray ray = new Ray(nearPoint, direction);

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

        BoundingBox GetIconQuadBBox(SpaceBody body)
        {
            float halfSide = body.IconSize * 0.5f;

            Vector3 diagVector = new Vector3(halfSide, halfSide, 0);

            Vector3 vProj = graphicsDevice.Viewport.Project(body.Location.Coordinates, perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            return new BoundingBox(vProj - diagVector, vProj + diagVector);
        }

        void BatchDrawIcons(Matrix projectionMatrix, Matrix viewMatrix, Matrix worldMatrix, Sector sector)
        {
            VertexPositionTexture[] iconVertices = new VertexPositionTexture[sector.Contents.Count * 4];

            int currVertex = 0;
            foreach (SpaceBody item in sector.Contents)
            {
                float halfSide = item.IconSize * 0.5f;
                Vector3 vProj = graphicsDevice.Viewport.Project(item.Location.Coordinates, projectionMatrix, viewMatrix, worldMatrix);

                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, halfSide, 0), new Vector2(0, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(-halfSide, -halfSide, 0), new Vector2(0, 0));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, halfSide, 0), new Vector2(1, 1));
                iconVertices[currVertex++] = new VertexPositionTexture(vProj + new Vector3(halfSide, -halfSide, 0), new Vector2(1, 0));
            }

            graphicsDevice.BlendState = BlendState.AlphaBlend;

            for (int i = 0; i < sector.Contents.Count; i++)
            {
                iconEffect.Texture = sector.Contents[i].IconTexture;
                foreach (EffectPass pass in iconEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, iconVertices, i * 4, 2);
                }
            }

            graphicsDevice.BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Draws a grid at y = 0 in the xz plane
        /// </summary>
        void DrawGrid(Color color)
        {
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

            foreach (EffectPass pass in gridEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 201 * 2);
            }
        }

        void RenderSector(Sector sector)
        {
            BatchDrawIcons(perspProjectionMatrix, camera.ViewMatrix, Matrix.Identity, displayedSector);
        }

        public void Render(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);

            totalFrames++;
            DrawGrid(Color.White);
            RenderSector(displayedSector);

            //spriteBatch.Begin();
            //spriteBatch.DrawString(Font, "FPS: " + fps, new Vector2(10, 10), Color.White);
            //spriteBatch.End();
        }

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

            SpaceBody mouseOverBody = PickBody(mousePos, displayedSector);

            if (mouseOverBody != null)
            {
                currLabel = new FloatingLabel(mouseOverBody.ToString(), mousePos);
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (mouseOverBody is Gate)
                    {
                        displayedSector = ((Gate)mouseOverBody).Destination.Sector;
                    }
                    else if (mouseOverBody is Ship)
                    {
                        ((Ship)mouseOverBody).GoalQueue.AddGoal(
                            new AI.MoveAndDockAt((Ship)mouseOverBody, mouseOverBody.Universe.Sectors[1].Contents.OfType<ProductionStation>().First(), null));
                    }
                }

                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    currMenu = new ContextMenu(Font);
                    currMenu.Items.Add(new ContextMenuItem("Item 1", delegate() { mouseOverBody.Location.Coordinates = Vector3.Zero; }));
                    currMenu.Items.Add(new ContextMenuItem("Item 2", null));
                    currMenu.Items.Add(new ContextMenuItem("Item 3", null));
                    currMenu.Items.Add(new ContextMenuItem("Item 4", null));

                    currMenu.Open(mouseState.X, mouseState.Y);
                }
            }
            else currLabel = null;

        }
    }
}
