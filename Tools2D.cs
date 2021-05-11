using System;
namespace Render
{
    public static class Tools2D
    {

        public static bool PointInTriangle(Point2D p, Point2D a, Point2D b, Point2D c)
        {
            // Test if p lies inside triangle abc (counter clockwise)

            // If p is to the right of ab then outside triangle
            if (Point2D.Cross(p - a, b - a) < 0)
                return false;

            // If p is to the right of bc then outside triangle
            if (Point2D.Cross(p - b, c - b) < 0)
                return false;

            // If p is to the right of ca then outside triangle
            if (Point2D.Cross(p - c, a - c) < 0)
                return false;

            return true;
        }

        public static void UnitTest_PointInTriangle()
        {
            var a = new Point2D(0, 3);
            var b = new Point2D(-3, -3);
            var c = new Point2D(3, -3);

            var test1 = PointInTriangle(new Point2D(0, 0), a, b, c);
            var test2 = PointInTriangle(new Point2D(1, 4), a, b, c);
            var test3 = PointInTriangle(new Point2D(4, 1), a, b, c);
            var test4 = PointInTriangle(new Point2D(1, 1), a, b, c);

            Console.WriteLine("test1: " + test1);
            Console.WriteLine("test2: " + test2);
            Console.WriteLine("test3: " + test3);
            Console.WriteLine("test4: " + test4);
        }
    }
}
