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
            var image = new bool[width, height];

            var a = new Point2D(50, 10);
            var b = new Point2D(80, 75);
            var c = new Point2D(15, 92);

            DrawTriangle(image, a, b, c);
            ExportImage(image);
        }

        private static void DrawTriangle(bool[,] image, Point2D a, Point2D b, Point2D c)
        {
            // TODO only check bounding box of triangle
            // TODO squares with early in/out

            var width = image.GetLength(0);
            var height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Tools2D.PointInTriangle(new Point2D(x, y), a, b, c))
                        image[x, y] = true;
                }
            }
        }

        private static void ExportImage(bool[,] image)
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
                    if (image[x, y])
                        line += "0 0 0\t";
                    else
                        line += "255 255 255\t";
                }

                builder.AppendLine(line);
            }

            File.WriteAllText("output.ppm", builder.ToString());
        }
    }
}
