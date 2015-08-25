using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace zreyla
{
    public static class Camera
    {
        public static Vector3 Position = Vector3.Zero;
        public static Vector3 Angle = Vector3.Zero;
        public static Vector3 Up = Vector3.Up;
        public static Vector3 Look = Vector3.Forward;
        public static Vector3 Right = Vector3.Right;
        private static Matrix viewMatrix;
        private static object viewMatrixLockObj = new object(); // TODO: Something neater

        public static Matrix ViewMatrix
        {
            get
            {
                lock (viewMatrixLockObj)
                    return viewMatrix;
            }
        }



        public static void RebuildMatrix()
        {
            Vector3 up;
            Vector3 look;
            Vector3 right;

            Matrix rollMatrix = Matrix.CreateFromAxisAngle(Vector3.Forward, -Angle.Z);

            up = Vector3.Transform(Vector3.Up, rollMatrix);
            right = Vector3.Transform(Vector3.Right, rollMatrix);

            Matrix yawMatrix = Matrix.CreateFromAxisAngle(up, -Angle.Y);

            look = Vector3.Transform(Vector3.Forward, yawMatrix);
            right = Vector3.Transform(right, yawMatrix);

            Matrix pitchMatrix = Matrix.CreateFromAxisAngle(right, -Angle.X);

            look = Vector3.Transform(look, pitchMatrix);
            up = Vector3.Transform(up, pitchMatrix);

            Up = up;
            Look = look;
            Right = right;

            //Vector3 adjustedPos = Position; // Stop this filth!
            //adjustedPos.X = 0 - adjustedPos.X;

            lock (viewMatrixLockObj)
                viewMatrix = Matrix.CreateLookAt(Position, Position + look, up);
        }
    }
}
