using System;
using System.Numerics;

namespace Render
{
    public static class Tools2D
    {
        public static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // Test if p lies inside triangle abc (counter clockwise)

            // If p is to the right of ab then outside triangle
            if (Cross(p - a, b - a) < 0)
                return false;

            // If p is to the right of bc then outside triangle
            if (Cross(p - b, c - b) < 0)
                return false;

            // If p is to the right of ca then outside triangle
            if (Cross(p - c, a - c) < 0)
                return false;

            return true;
        }

        public static float Cross(Vector2 u, Vector2 v)
        {
            return u.Y * v.X - u.X * v.Y;
        }

        public static Matrix4x4 Multiply(params Matrix4x4[] matrices)
        {
            var result = Matrix4x4.Identity;

            foreach (var m in matrices)
                result = Matrix4x4.Multiply(result, m);

            return result;
        }
    }
}
