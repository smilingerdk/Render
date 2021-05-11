using System;
using System.IO;
using System.Linq;
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

            DrawTriangle(image, a, b, c, AntiAlias.SuperSampling2x2);
            ExportImage(image, "output"); //DateTime.Now.ToString());
        }

        private static void DrawTriangle(double[,] image, Point2D a, Point2D b, Point2D c, AntiAlias antiAlias)
        {
            // TODO only check bounding box of triangle
            // TODO squares with early in/out

            var width = image.GetLength(0);
            var height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (antiAlias)
                    {
                        case AntiAlias.None:
                        {
                            var inTriangle = Tools2D.PointInTriangle(new Point2D(x + .5, y + .5), a, b, c);
                            image[x, y] = inTriangle ? 0 : 1;
                            break;
                        }
                        case AntiAlias.SuperSampling2x2:
                        {
                            Point2D[] points = {
                                new Point2D(x + .2, y + .2),
                                new Point2D(x + .2, y + .8),
                                new Point2D(x + .8, y + .2),
                                new Point2D(x + .8, y + .8)
                            };

                            var r = (double)points.Count(p => !Tools2D.PointInTriangle(p, a, b, c)) / points.Count();

                            image[x, y] = r;

                            break;
                        }
                    }
                    
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

    public enum AntiAlias { None, SuperSampling2x2 }
}
