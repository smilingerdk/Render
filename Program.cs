using System;
using System.IO;
using System.Text;

namespace Render
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var width = 100;
            var height = 100;
            var image = new double[width, height];

            var a = new Point2D(50, 10);
            var b = new Point2D(80, 75);
            var c = new Point2D(15, 92);

            DrawTriangle(image, a, b, c);
            ExportImage(image, "output"); //DateTime.Now.ToString());
        }

        private static void DrawTriangle(double[,] image, Point2D a, Point2D b, Point2D c)
        {
            // TODO only check bounding box of triangle
            // TODO squares with early in/out

            var width = image.GetLength(0);
            var height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var inTriangle = Tools2D.PointInTriangle(new Point2D(x, y), a, b, c);
                    image[x, y] = inTriangle ? 0 : 1;
                }
            }
        }

        private static void ExportImage(double[,] image, string filename)
        {
            var width = image.GetLength(0);
            var height = image.GetLength(1);
            var builder = new StringBuilder();

            builder.AppendLine("P3"); // ASCII PPM file header
            builder.AppendLine(width + " " + height); // Dimensions of image
            builder.AppendLine("255"); // Maximum colour value

            for (int y = 0; y < height; y++)
            {
                var line = "";

                for (int x = 0; x < width; x++)
                {
                    var color = (int)Math.Round(image[x, y] * 255);
                    line += $"{color} {color} {color}\t";
                }

                builder.AppendLine(line);
            }

            File.WriteAllText(filename + ".ppm", builder.ToString());
        }
    }
}
