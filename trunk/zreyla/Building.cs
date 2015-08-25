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


namespace zreyla
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Building : Entity
    {
        BasicEffect effect; // TODO: Is creating a new EffectPool every time a bad thing?
        VertexDeclaration vpntDec;

        //Texture2D texture;

        public BuildingModel Model;

        public Building(Game game)
            : base(game)
        {
            Model = CreateBuildingModelAuto(game, game.GraphicsDevice);

            effect = new BasicEffect(game.GraphicsDevice, new EffectPool());
            effect.DiffuseColor = Color.Gray.ToVector3();
            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;

            vpntDec = new VertexDeclaration(game.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            this.CollisionType = EntityCollisionType.BuildingSpecial;

            //texture = new Texture2D(game.GraphicsDevice, 
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix)
        {
            graphics.GraphicsDevice.VertexDeclaration = vpntDec;
            graphics.GraphicsDevice.Vertices[0].SetSource(Model.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);

            Matrix worldMatrix = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Angle.Y, Angle.X, Angle.Z) * Matrix.CreateTranslation(Position);

            effect.World = worldMatrix;
            effect.Projection = projectionMatrix;
            effect.View = viewMatrix;


            effect.Begin();

            foreach (EffectPass ep in effect.CurrentTechnique.Passes)
            {
                ep.Begin();
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Model.ModelParts.Length * 2 * 6);
                ep.End();
            }

            effect.End();
        }

        

        // TODO: Use a fat indexbuffer instead, or render using the same vertex buffer and different world matrices. idk...
        private static BuildingModel CreateBuildingModelAuto(Game game, GraphicsDevice gd)
        {
            List<BoundingBox> lbb = GenerateBuilding();

            BuildingModel bm = new BuildingModel();

            //BoundingBox ground = new BoundingBox(new Vector3(-384, -20, -384), new Vector3(384, 0, 384));
            //lbb.Add(ground);

            List<BuildingModelPart> lbmp = new List<BuildingModelPart>();

            VertexBuffer vb = new VertexBuffer(gd, new VertexPositionNormalTexture().GetType(), 36 * lbb.Count, BufferUsage.WriteOnly);

            int v = 0;

            foreach (BoundingBox bb in lbb)
            {
                BuildingModelPart bmp = new BuildingModelPart(game);
                bmp.BoundingBox = bb;
                bmp.StartVertex = v;
                bmp.VertexCount = 36;

                VertexPositionNormalTexture[] lvpc = new VertexPositionNormalTexture[36];

                Vector3[] vpos = new Vector3[36];

                // ccw draw -- build all triangles going counterclockwise.
                // FRONT
                vpos[0] = new Vector3(bb.Min.X, bb.Min.Y, bb.Min.Z);
                vpos[1] = vpos[4] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
                vpos[2] = vpos[3] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
                vpos[5] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);

                // BACK
                vpos[6] = vpos[11] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
                vpos[7] = new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z);
                vpos[8] = vpos[9] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
                vpos[10] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);

                // TOP
                vpos[12] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
                vpos[13] = vpos[16] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
                vpos[14] = vpos[15] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
                vpos[17] = new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z);

                // BOTTOM
                vpos[18] = new Vector3(bb.Min.X, bb.Min.Y, bb.Min.Z);
                vpos[19] = vpos[22] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
                vpos[20] = vpos[21] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
                vpos[23] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);

                // LEFT
                vpos[24] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
                vpos[25] = vpos[28] = new Vector3(bb.Min.X, bb.Min.Y, bb.Min.Z);
                vpos[26] = vpos[27] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
                vpos[29] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);

                // RIGHT
                vpos[30] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
                vpos[31] = vpos[34] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
                vpos[32] = vpos[33] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
                vpos[35] = new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z);

                for (int i = 0; i < 36; i++)
                {
                    Vector3 normal;

                    switch ((i - (i % 6)) / 6)
                    {
                        case 0: //Front!
                            normal = new Vector3(0f, 0f, -1f);
                            break;
                        case 1: // Back!
                            normal = new Vector3(0f, 0f, 1f);
                            break;
                        case 2: // Top!
                            normal = new Vector3(0f, 1f, 0f);
                            break;
                        case 3: // Bottom!
                            normal = new Vector3(0f, -1f, 0f);
                            break;
                        case 4: // Left!
                            normal = new Vector3(-1f, 0f, 0f);
                            break;
                        case 5: // Right!
                            normal = new Vector3(1f, 0f, 0f);
                            break;
                        default:
                            throw new Exception("calc fail");
                    }

                    lvpc[i] = new VertexPositionNormalTexture(vpos[i], normal, Vector2.Zero);
                }


                vb.SetData<VertexPositionNormalTexture>(VertexPositionNormalTexture.SizeInBytes * v, lvpc, 0, lvpc.Length, VertexPositionNormalTexture.SizeInBytes);

                v += 36;

                lbmp.Add(bmp);
            }

            bm.ModelParts = lbmp.ToArray();
            bm.VertexBuffer = vb;

            return bm;
        }

        private static List<BoundingBox> GenerateBuilding()
        {
            Random generator = new Random();
            List<BoundingBox> bounders = new List<BoundingBox>();

            while (bounders.Count <= 3)
            {
                // TODO: Any particular reason not to use 'generator' here?
                for (int i = 0; i < (new Random().Next(6) + 1); i++)
                {
                    // TODO: Fix yoffset->zoffset

                    //width,depth,positionx,positiony
                    int width = generator.Next(30, 256);
                    int Xoffset = generator.Next(0, 256 - width);
                    int depth = generator.Next(30, 256);
                    int Yoffset = generator.Next(0, 256 - width);
                    float height = generator.Next(256, 256 * 3);
                    Vector3 min = new Vector3(Xoffset, 0, Yoffset);
                    Vector3 max = new Vector3(Xoffset + width, height, Yoffset + depth);
                    BoundingBox bound = new BoundingBox(min, max);
                    bool dontadd = false;



                    foreach (BoundingBox b in bounders)
                    {
                        if (b.Contains(bound) == ContainmentType.Contains || b.Contains(bound) == ContainmentType.Disjoint)
                        {
                            dontadd = true;
                            break;
                        }
                    }

                    if (!dontadd)
                        bounders.Add(bound);

                }
            }
            return bounders;
        }

    }

    public class BuildingModel
    {
        public VertexBuffer VertexBuffer;
        public BuildingModelPart[] ModelParts;
    }

    public class BuildingModelPart : Entity
    {
        public int StartVertex;
        public int VertexCount;

        public BuildingModelPart(Game game)
            : base(game)
        {
            this.CollisionType = EntityCollisionType.BoundingBox;
        }

        public override void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix)
        {
            //throw new NotImplementedException();
        }
    }
}
