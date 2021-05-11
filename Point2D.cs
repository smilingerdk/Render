using System;
namespace Render
{
    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point2D operator -(Point2D a, Point2D b) => new Point2D(a.X - b.X, a.Y - b.Y);

        public static double Cross(Point2D u, Point2D v)
        {
            return u.Y * v.X - u.X * v.Y;
        }
    }
}
