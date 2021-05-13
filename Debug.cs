using System;
using System.Numerics;

namespace Render
{
    public static class Debug
    {
        public static void DrawTriangle()
        {
            /*var width = 200;
            var height = 100;
            var image = new double[width, height];

            var a = new Vector2(50, 10);
            var b = new Vector2(80, 75);
            var c = new Vector2(15, 92);

            DrawTriangle(image, a, b, c, AntiAlias.SuperSampling2x2);
            ExportImage(image, "output"); //DateTime.Now.ToString());*/
        }

        public static void UnitTest_PointInTriangle()
        {
            var a = new Vector2(0, 3);
            var b = new Vector2(-3, -3);
            var c = new Vector2(3, -3);

            var test1 = Tools2D.PointInTriangle(new Vector2(0, 0), a, b, c);
            var test2 = Tools2D.PointInTriangle(new Vector2(1, 4), a, b, c);
            var test3 = Tools2D.PointInTriangle(new Vector2(4, 1), a, b, c);
            var test4 = Tools2D.PointInTriangle(new Vector2(1, 1), a, b, c);

            Console.WriteLine("test1: " + test1); // Expected: True
            Console.WriteLine("test2: " + test2); // Expected: False
            Console.WriteLine("test3: " + test3); // Expected: False
            Console.WriteLine("test4: " + test4); // Expected: True
        }
    }
}
