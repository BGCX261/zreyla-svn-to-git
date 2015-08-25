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
    public enum EntityCollisionType
    {
        None,
        BoundingBox,
        BoundingSphere,
        BuildingSpecial
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Entity : Microsoft.Xna.Framework.GameComponent
    {
        public Entity(Game game)
            : base(game)
        {
        }

        //public Entity Parent = null;

        // TODO: Add events so entities can handle collision events etc. if they want to.

        // TODO: Stop constantly building matrices all the time every single goddamn frame
        // (i.e. make some kind of helper func here to handle cacheing matrices.)

        public Vector3 Position = Vector3.Zero;
        public Vector3 Angle = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public EntityCollisionType CollisionType = EntityCollisionType.None;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;

        private Plane[] cachedPlanes = null;
        private BoundingBox cachedPlaneLastBoxUsed;

        public Plane[] GetPlanesFromBoundingBox()
        {
            if (CollisionType != EntityCollisionType.BoundingBox)
                return new Plane[0];

            if (cachedPlaneLastBoxUsed != BoundingBox)
            {
                cachedPlanes = new Plane[6];

                cachedPlanes[0] = new Plane(Vector3.Up, -BoundingBox.Max.Y);
                cachedPlanes[1] = new Plane(Vector3.Down, BoundingBox.Min.Y);
                cachedPlanes[2] = new Plane(Vector3.Left, BoundingBox.Min.X);
                cachedPlanes[3] = new Plane(Vector3.Right, BoundingBox.Max.X);
                cachedPlanes[4] = new Plane(Vector3.Forward, BoundingBox.Min.Z);
                cachedPlanes[5] = new Plane(Vector3.Backward, -BoundingBox.Max.Z);

                //cachedPlanes[0] = new Plane(BoundingBox.Min, BoundingBox.Min + new Vector3(0f, BoundingBox.Max.Y, 0f), BoundingBox.Min + new Vector3(0f, 0f, BoundingBox.Max.Z));
                

                cachedPlaneLastBoxUsed = BoundingBox;
            }

            return cachedPlanes;
        }

        public abstract void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix);
    }
}
