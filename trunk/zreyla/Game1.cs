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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;
using System.Threading;

namespace zreyla
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model cube;
        Model craparena;
        SpriteFont lucidaConsoleBold;
        Building building;

        float MoveSpeed = 50f;
        bool CoolModeActive = false;
        float CoolModeVelocityDown = 0f;
        bool held = false;  // is _what_ held???
        bool lastStartButtonState = false;

        float modelRotation = 0f;

        int rsincelast = 60;

        string fps = "calculating.";
        bool fpsOn = true;
        bool fpsWaitingForSeqRelease = false;

        List<Entity> Entities;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            // Defaults.
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferMultiSampling = true;
            
            Content.RootDirectory = "Content";
        }

        private void DemoChangeRes()
        {
            if (GamePad.GetState(PlayerIndex.One).IsButtonUp(Buttons.Y))
            {
                graphics.PreferredBackBufferHeight = 720;
                graphics.PreferredBackBufferWidth = 1280;
            }
            else if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
            {
                graphics.PreferredBackBufferHeight = 576;
                graphics.PreferredBackBufferWidth = 720;
            }
            else
            {
                graphics.PreferredBackBufferWidth = 1920;
                graphics.PreferredBackBufferHeight = 1080;
            }

            graphics.PreferMultiSampling = GamePad.GetState(PlayerIndex.One).IsButtonUp(Buttons.RightShoulder);

            graphics.SynchronizeWithVerticalRetrace = GamePad.GetState(PlayerIndex.One).IsButtonUp(Buttons.LeftShoulder);

            graphics.ApplyChanges();
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

            cube = Content.Load<Model>("xsmoothmonkeyman");
            lucidaConsoleBold = Content.Load<SpriteFont>("LucidaConsoleBold");

            Entities = new List<Entity>();

            Entities.Add(new TempGround(this));
            building = new Building(this);
            Entities.Add(building);

            foreach (Entity e in Entities)
                e.Initialize();
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
            GamePadState gpState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (gpState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (gpState.IsButtonDown(Buttons.LeftShoulder) && gpState.IsButtonDown(Buttons.A))
            {
                if (!held)
                {
                    Entities.Remove(building);
                    building = new Building(this);
                    Entities.Add(building);
                    held = true;
                }
            }
            else
                held = false;


            Vector3 playerLook = Camera.Look;
            Vector3 playerRight = Camera.Right;

            // Fix Y coord if 'cool mode' is on.
            if (CoolModeActive && gpState.IsButtonUp(Buttons.LeftTrigger))
            {
                playerLook.Y = 0;
                playerLook.Normalize();
                playerRight.Y = 0;
                playerRight.Normalize();
            }
            
            foreach (Entity e in Entities)
                e.Update(gameTime);

            Vector3 prevPos = Camera.Position;
            
            if (gpState.ThumbSticks.Left != Vector2.Zero)
                Camera.Position += gpState.ThumbSticks.Left.X * playerRight * (MoveSpeed / TimeSpan.TicksPerSecond) * gameTime.ElapsedGameTime.Ticks + gpState.ThumbSticks.Left.Y * playerLook * (MoveSpeed / TimeSpan.TicksPerSecond) * gameTime.ElapsedGameTime.Ticks;
            if (gpState.ThumbSticks.Right != Vector2.Zero)
                Camera.Angle += new Vector3(-(gpState.ThumbSticks.Right.Y * 0.1f), gpState.ThumbSticks.Right.X * 0.1f, 0f);

            if (gpState.IsButtonDown(Buttons.DPadUp))
                MoveSpeed += 5.0f * gameTime.ElapsedGameTime.Ticks / (float) TimeSpan.TicksPerSecond;
            if (gpState.IsButtonDown(Buttons.DPadDown))
                MoveSpeed -= 5.0f * gameTime.ElapsedGameTime.Ticks / (float) TimeSpan.TicksPerSecond;

            if (gpState.IsButtonDown(Buttons.Start) && !lastStartButtonState)
                CoolModeActive = !CoolModeActive;
            lastStartButtonState = gpState.IsButtonDown(Buttons.Start);

            if (CoolModeActive)
            {
                bool fail = false;

                if (gpState.IsButtonDown(Buttons.LeftTrigger))
                    CoolModeVelocityDown = 0f;
                else
                    CoolModeVelocityDown += (((9.81f) / TimeSpan.TicksPerSecond) * (float) gameTime.ElapsedGameTime.Ticks);

                Camera.Position += Vector3.Down * CoolModeVelocityDown;

                BoundingBox playerBox = new BoundingBox(Camera.Position - new Vector3(1.6f, 6.0f, 1.6f), Camera.Position + new Vector3(1.6f, 1.0f, 1.6f));
                //Ray downRay = new Ray(Camera.Position, Vector3.Down);

                // TODO: Move this code somewhere better.
                // TODO: Stop duplicating box collision code inside case EntityCollisionType.BuildingSpecial!
                foreach (Entity e in Entities)
                {
                    switch (e.CollisionType)
                    {
                        case EntityCollisionType.BuildingSpecial:
                            // TODO: Cleanup properly like.
                            foreach (Entity emp in ((Building)e).Model.ModelParts)
                            {
                                Debug.Assert(emp.CollisionType == EntityCollisionType.BoundingBox, "BuildingModelPart does not have BoundingBox collision!");

                                // --- SLIDING COLLISION CODE ---
                                if (emp.BoundingBox.Contains(playerBox) != ContainmentType.Disjoint)
                                {
                                    foreach (Plane p in emp.GetPlanesFromBoundingBox())
                                    {
                                        if (p.Intersects(playerBox) == PlaneIntersectionType.Intersecting)
                                        {
                                            Vector3 shove = (Camera.Position - prevPos) * p.Normal;

                                            if ((p.Normal.X != 0f && (p.Normal.X > 0f && shove.X < 0f))
                                                || (p.Normal.Y != 0f && (p.Normal.Y > 0f && shove.Y < 0f))
                                                || (p.Normal.Z != 0f && (p.Normal.Z > 0f && shove.Z < 0f)))
                                                shove = Vector3.Zero - shove;

                                            Camera.Position += shove;
                                            
                                            playerBox = new BoundingBox(Camera.Position - new Vector3(1.6f, 6.0f, 1.6f), Camera.Position + new Vector3(1.6f, 1.0f, 1.6f));

                                            // making up for ground collision code
                                            if (p.Normal.Y == 1f)
                                                fail = true;
                                        }
                                    }
                                }
                            }
                            break;

                        case EntityCollisionType.BoundingBox:
                            BoundingBox b = e.BoundingBox;

                            // --- SLIDING COLLISION CODE ---
                            if (b.Intersects(playerBox))
                            {
                                foreach (Plane p in e.GetPlanesFromBoundingBox())
                                {
                                    if (p.Intersects(playerBox) == PlaneIntersectionType.Intersecting)
                                    {
                                        Vector3 shove = (Camera.Position - prevPos) * p.Normal;

                                        if ((p.Normal.X != 0f && (p.Normal.X > 0f && shove.X < 0f))
                                            || (p.Normal.Y != 0f && (p.Normal.Y > 0f && shove.Y < 0f))
                                            || (p.Normal.Z != 0f && (p.Normal.Z > 0f && shove.Z < 0f)))
                                            shove = Vector3.Zero - shove;

                                        Camera.Position += shove;

                                        playerBox = new BoundingBox(Camera.Position - new Vector3(1.6f, 6.0f, 1.6f), Camera.Position + new Vector3(1.6f, 1.0f, 1.6f));

                                        // making up for ground collision code
                                        if (p.Normal.Y == 1f)
                                            fail = true;
                                    }
                                }
                            }

                            // --- GROUND COLLISION CODE ---
                            /*{
                                //BoundingBox tmphack = new BoundingBox(new Vector3(b.Min.X, b.Max.Y - 5f, b.Min.Z), b.Max);

                                if (b.Intersects(playerBox))
                                {
                                    float? distFromFloor = downRay.Intersects(b);

                                    if (distFromFloor < 0)
                                        Debug.WriteLine(":)");

                                    if (distFromFloor == null || distFromFloor == float.NaN || distFromFloor >= 6.0f || distFromFloor < 0) // cam is 6.0 units from ground
                                        continue;

                                    // Otherwise...
                                    fail = true;

                                    // we say in the boundingbox playerBox that the camera should be 60 units from the ground
                                    float distIntersectingFloor = ((float)distFromFloor) - 6.0f;

                                    distIntersectingFloor = MathHelper.Clamp(distIntersectingFloor, -6.0f, 0);

                                    Camera.Position.Y -= distIntersectingFloor;

                                    playerBox = new BoundingBox(Camera.Position - new Vector3(1.6f, 6.0f, 1.6f), Camera.Position + new Vector3(1.6f, 1.0f, 1.6f));
                                    downRay = new Ray(Camera.Position, Vector3.Down);
                                }
                            }*/

                            break;
                            
                        case EntityCollisionType.None:
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                if (fail)
                    CoolModeVelocityDown = 0f;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Camera.RebuildMatrix();

            // TODO: Add your drawing code here

            modelRotation += (float)((Math.PI / 4f) / 60f);

            Matrix ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                        GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);

            RenderMonkey(ProjectionMatrix);

            // Disable culling - building generation is subtly broken atm.
            graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;

            foreach (Entity e in Entities)
                e.Draw(graphics, gameTime, Camera.ViewMatrix, ProjectionMatrix);

            DebugDraw(gameTime);

            base.Draw(gameTime);
        }

        private void DebugDraw(GameTime gameTime)
        {
            if (fpsOn)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

                if (rsincelast == 0)
                {
                    fps = Math.Round(1.0 / gameTime.ElapsedRealTime.TotalSeconds, 2).ToString() + " FPS\nMax generation: " + GC.MaxGeneration.ToString() + "\nTotal memory used: " + (GC.GetTotalMemory(false) / (1024f * 1024f)).ToString() + "megabytes";
                    rsincelast = 60;
                }
                else
                    rsincelast--;

                spriteBatch.DrawString(lucidaConsoleBold, fps, new Vector2(1800, 140) - lucidaConsoleBold.MeasureString(fps), Color.White);
                spriteBatch.End();
            }

            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftShoulder))
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                spriteBatch.DrawString(lucidaConsoleBold, "DEBUG MODE.\nLS+DPad-Left to change resolution\nLS+A to regenerate buildings\nLS+DPad-Right to show/hide FPS\nJust LT lets you fly.\nPressing [START] toggles COOL MODE", new Vector2(300f, 300f), Color.White);
                spriteBatch.End();

                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadLeft))
                {
                    Thread.Sleep(2000);
                    DemoChangeRes();
                }

                if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadRight))
                {
                    if (!fpsWaitingForSeqRelease)
                    {
                        fpsOn = !fpsOn;
                        fpsWaitingForSeqRelease = true;
                    }
                }
                else
                    fpsWaitingForSeqRelease = false;
            }
            else
                fpsWaitingForSeqRelease = false;
        }

        private void RenderMonkey(Matrix projectionMatrix)
        {
            foreach (ModelMesh mm in cube.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mm.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = mm.ParentBone.Transform * Matrix.CreateRotationY(modelRotation) * Matrix.CreateTranslation(0f, 0f, 5f);
                    effect.View = Camera.ViewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.PreferPerPixelLighting = true;
                }
                // Draw the mesh, using the effects set above.
                mm.Draw();
            }
        }
    }
}
