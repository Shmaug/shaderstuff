using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace shaderstuff {
    public class Main : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Effect postfx;
        Effect worldfx;
        Model sphere;
        Texture2D pixel;
        Vector3 SunPos = new Vector3(100, 100, 100);

        float camAng = 0;
        float potAng = 0;

        KeyboardState ks, lastks;
        MouseState ms, lastms;

        RenderTarget2D mainTarg;
        RenderTarget2D bloomTarg;
        RenderTarget2D blurTarg;

        World world;

        Matrix Projection;
        Matrix View;

        Vector3 CamPos = Vector3.Up;
        Vector3 CamRot = Vector3.Zero;

        public Main() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            graphics.PreferMultiSampling = true;
            IsMouseVisible = false;
            Window.ClientSizeChanged += (object sender, EventArgs e) => {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                mainTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
                blurTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
                bloomTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
                graphics.ApplyChanges();
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f), graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight, .1f, 10000f);
            };
        }


        protected override void Initialize() {
            world = new World();

            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            postfx = Content.Load<Effect>("post");
            worldfx = Content.Load<Effect>("world");
            sphere = Content.Load<Model>("sphere");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData<Color>(new Color[] { Color.White });

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f), graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight, .1f, 10000f);
            View = Matrix.CreateLookAt(Vector3.Backward * 3f + Vector3.Left * 2f + Vector3.Up * 1f, Vector3.Up * 1f, Vector3.Up);

            mainTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
            bloomTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
            blurTarg = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 2, RenderTargetUsage.PreserveContents);
        }

        protected override void UnloadContent() {

        }

        protected override void Update(GameTime gameTime) {
            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            float a = MathHelper.ToRadians(10f);
            if (ks.IsKeyDown(Keys.A))
                a *= 10;
            else if (ks.IsKeyDown(Keys.D))
                a *= -10;
            else
                a *= 0;
            camAng += (float)gameTime.ElapsedGameTime.TotalSeconds * a;
            potAng += (float)gameTime.ElapsedGameTime.TotalSeconds * MathHelper.ToRadians(50f);

            if (!IsMouseVisible) {
                float dx = (ms.X - Window.ClientBounds.Width / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds * .3f;
                float dy = (ms.Y - Window.ClientBounds.Height / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds * .3f;
                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
                CamRot.X -= dy;
                CamRot.Y -= dx;

                Matrix rot = Matrix.CreateRotationX(CamRot.X) * Matrix.CreateRotationY(CamRot.Y);

                Vector3 mv = Vector3.Zero;
                if (ks.IsKeyDown(Keys.W))
                    mv += Vector3.Forward;
                if (ks.IsKeyDown(Keys.S))
                    mv += Vector3.Backward;
                if (ks.IsKeyDown(Keys.A))
                    mv += Vector3.Left;
                if (ks.IsKeyDown(Keys.D))
                    mv += Vector3.Right;
                if (mv != Vector3.Zero)
                    mv.Normalize();
                if (ks.IsKeyDown(Keys.LeftShift))
                    mv *= 10f;
                mv = Vector3.Transform(mv, rot);
                CamPos += mv * (float)gameTime.ElapsedGameTime.TotalSeconds;
                View = Matrix.CreateLookAt(CamPos, CamPos + rot.Forward, rot.Up);
            }

            for (int i = 0; i < world.Nodes.Length; i++) {
                world.Nodes[i].splitIfIntersect(new BoundingSphere(CamPos, 10), 8);
            }

            if (ks.IsKeyDown(Keys.LeftAlt) && lastks.IsKeyUp(Keys.LeftAlt))
                IsMouseVisible = !IsMouseVisible;

            lastms = ms;
            lastks = ks;
            base.Update(gameTime);
        }

        void Render(Model mod, Matrix world) {
            Matrix[] transfs = new Matrix[mod.Bones.Count];
            mod.CopyAbsoluteBoneTransformsTo(transfs);
            foreach (ModelMesh m in mod.Meshes) {
                worldfx.Parameters["W"].SetValue(transfs[m.ParentBone.Index] * world);
                worldfx.Parameters["WIT"].SetValue(Matrix.Invert(Matrix.Transpose(transfs[m.ParentBone.Index] * world)));
                foreach (ModelMeshPart p in m.MeshParts) {
                    p.Effect = worldfx;
                }
                m.Draw();
            }
        }

        void DrawScene() {
            worldfx.Parameters["LightPos"].SetValue(SunPos);
            worldfx.Parameters["VP"].SetValue(View * Projection);
            worldfx.Parameters["W"].SetValue(Matrix.CreateTranslation(world.Position));
            worldfx.Parameters["WIT"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.CreateTranslation(world.Position))));

            GraphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            world.Render(GraphicsDevice, worldfx);
            GraphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid };
        }

        protected override void Draw(GameTime gameTime) {

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            GraphicsDevice.SetRenderTarget(mainTarg);
            GraphicsDevice.Clear(Color.Transparent);

            worldfx.Parameters["LightColor"].SetValue(Color.LightYellow.ToVector4());
            worldfx.Parameters["W"].SetValue(Matrix.CreateTranslation(SunPos));
            worldfx.CurrentTechnique = worldfx.Techniques["Light"];
            sphere.Meshes[0].MeshParts[0].Effect = worldfx;
            sphere.Meshes[0].Draw();

            worldfx.CurrentTechnique = worldfx.Techniques["Occuld"];
            DrawScene();

            #region bloom extract
            // Extract the lights from the previous render
            GraphicsDevice.SetRenderTarget(bloomTarg);
            GraphicsDevice.Clear(Color.Transparent);
            postfx.Parameters["Scale"].SetValue(4f); // scale down for bigger blurs
            postfx.CurrentTechnique = postfx.Techniques["BloomExtract"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, postfx);
            spriteBatch.Draw(mainTarg, Vector2.Zero, Color.White);
            spriteBatch.End();
            postfx.Parameters["Scale"].SetValue(1f);
            #endregion
            
            #region blur
            // Blur lights X
            GraphicsDevice.SetRenderTarget(blurTarg);
            GraphicsDevice.Clear(Color.Transparent);
            postfx.CurrentTechnique = postfx.Techniques["Blur"];
            postfx.Parameters["BlurAxis"].SetValue(Vector2.UnitX);
            postfx.Parameters["pixel"].SetValue(new Vector2(1f / bloomTarg.Width, 1f / bloomTarg.Height));
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, postfx);
            spriteBatch.Draw(bloomTarg, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Blur lights Y
            GraphicsDevice.SetRenderTarget(bloomTarg);
            GraphicsDevice.Clear(Color.Transparent);
            postfx.Parameters["BlurAxis"].SetValue(Vector2.UnitY);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, postfx);
            spriteBatch.Draw(blurTarg, Vector2.Zero, Color.White);
            spriteBatch.End();
            #endregion
            
            #region resize
            GraphicsDevice.SetRenderTarget(mainTarg);
            GraphicsDevice.Clear(Color.Transparent);
            postfx.CurrentTechnique = postfx.Techniques["Normal"];
            postfx.Parameters["Scale"].SetValue(.25f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, postfx);
            spriteBatch.Draw(bloomTarg, Vector2.Zero, Color.White);
            spriteBatch.End();
            postfx.Parameters["Scale"].SetValue(1);
            #endregion

            #region final render
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            worldfx.CurrentTechnique = worldfx.Techniques["Lambert"];
            DrawScene();
            #endregion
            
            #region bloom overlay
            Vector3 lightProj = GraphicsDevice.Viewport.Project(SunPos, Projection, View, Matrix.Identity) / new Vector3(mainTarg.Width, mainTarg.Height, 1);
            postfx.Parameters["LightUV"].SetValue(new Vector2(lightProj.X, lightProj.Y));
            postfx.CurrentTechnique = postfx.Techniques["Crepuscular"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, postfx);
            spriteBatch.Draw(mainTarg, Vector2.Zero, Color.White);
            spriteBatch.End();
            #endregion

            base.Draw(gameTime);
        }
    }
}
