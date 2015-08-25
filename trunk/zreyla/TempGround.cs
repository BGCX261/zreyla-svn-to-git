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
    public class TempGround : Entity
    {
        BasicEffect effect; // TODO: Is creating a new EffectPool every time a bad thing?
        VertexBuffer vb;
        VertexDeclaration vpntDec;

        public TempGround(Game game)
            : base(game)
        {
            vb = new VertexBuffer(game.GraphicsDevice, VertexPositionNormalTexture.SizeInBytes * 6, BufferUsage.WriteOnly);

            VertexPositionNormalTexture[] vpnt = new VertexPositionNormalTexture[6];
            vpnt[0] = new VertexPositionNormalTexture(new Vector3(-384, 0, -384), Vector3.Up, new Vector2(0f, 1f));
            vpnt[1] = vpnt[3] = new VertexPositionNormalTexture(new Vector3(384, 0, -384), Vector3.Up, new Vector2(1f, 1f));
            vpnt[2] = vpnt[5] = new VertexPositionNormalTexture(new Vector3(-384, 0, 384), Vector3.Up, new Vector2(0f, 0f));
            vpnt[4] = new VertexPositionNormalTexture(new Vector3(384, 0, 384), Vector3.Up, new Vector2(1f, 0f));

            vb.SetData<VertexPositionNormalTexture>(vpnt);

            effect = new BasicEffect(game.GraphicsDevice, new EffectPool());
            effect.DiffuseColor = Color.GreenYellow.ToVector3();
            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;

            vpntDec = new VertexDeclaration(game.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            this.CollisionType = EntityCollisionType.BoundingBox;
            this.BoundingBox = new BoundingBox(new Vector3(-384f, -10f, -384f), new Vector3(384f, 0f, 384f));
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
            graphics.GraphicsDevice.Vertices[0].SetSource(vb, 0, VertexPositionNormalTexture.SizeInBytes);

            Matrix worldMatrix = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Angle.Y, Angle.X, Angle.Z) * Matrix.CreateTranslation(Position);

            effect.World = worldMatrix;
            effect.Projection = projectionMatrix;
            effect.View = viewMatrix;


            effect.Begin();

            foreach (EffectPass ep in effect.CurrentTechnique.Passes)
            {
                ep.Begin();
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                ep.End();
            }

            effect.End();
        }
    }
}