using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Render
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var width = 200;
            var height = 100;
            var image = new Vector3[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    image[x, y] = Vector3.One; // Set color of image to white

            // World coordinates
            // Camera and model

            Vector3[] triangle =
            {
                new Vector3(-3, -3, 0),
                new Vector3(3, -3, 0),
                new Vector3(0, 3, 0),
            };

            var color = new Vector3(1, 0, 0);

            Debug.UnitTest_PointInTriangle();

            var cameraPosition = new Vector3(0, 0, -2);
            var cameraTarget = new Vector3(0, 0, 0);
            var cameraDirection = Vector3.Normalize(cameraPosition - cameraTarget); // Note: this is actually pointing in the opposite direction

            // Camera is looking down at -z direction, all objects are expressed relative to camera

            // Translate camera to origin, rotate to look in -z direction
            Vector3 up = new Vector3(0, 1, 0); // Some up vector, not orthogonal to the camera
            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(up, cameraDirection));
            Vector3 cameraUp = Vector3.Cross(cameraDirection, cameraRight);

            var lookAt = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUp); // View
            var perspective = Matrix4x4.CreatePerspective(10, 5, 1, 10); // Projection

            Matrix4x4[] matrices = {
                lookAt,
                perspective,
                Matrix4x4.CreateScale(1, -1, 1),
                Matrix4x4.CreateTranslation(1, 1, 0),
                Matrix4x4.CreateScale(width * .5f, height * .5f, 1),
            };
 
            var transform = Tools2D.Multiply(matrices);
 
            var transformed = triangle.Select(p => Vector4.Transform(p, transform)).ToArray();
            var screen = transformed.Select(p => new Vector2(p.X / p.W, p.Y / p.W)).ToArray();

            DrawTriangle(image, screen[0], screen[1], screen[2], color, AntiAlias.SuperSampling2x2);
            //ExportImage(image, "output");
            ExportImage(image, DateTime.Now.ToString());
        }

        private static void DrawTriangle(Vector3[,] image, Vector2 a, Vector2 b, Vector2 c, Vector3 color, AntiAlias antiAlias)
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
                            if (Tools2D.PointInTriangle(new Vector2(x + .5f, y + .5f), a, b, c))
                                image[x, y] = color;

                            break;
                        }
                        case AntiAlias.SuperSampling2x2:
                        {
                            Vector2[] points = {
                                new Vector2(x + .2f, y + .2f),
                                new Vector2(x + .2f, y + .8f),
                                new Vector2(x + .8f, y + .2f),
                                new Vector2(x + .8f, y + .8f)
                            };

                            var r = (float)points.Count(p => Tools2D.PointInTriangle(p, a, b, c)) / points.Count();

                            image[x, y] = Vector3.Lerp(image[x, y], color, r);

                            break;
                        }
                    }
                }
            }
        }

        private static void ExportImage(Vector3[,] image, string filename)
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
                    var r = (int)Math.Round(image[x, y].X * 255);
                    var g = (int)Math.Round(image[x, y].Y * 255);
                    var b = (int)Math.Round(image[x, y].Z * 255);
                    line += $"{r} {g} {b}\t";
                }

                builder.AppendLine(line);
            }

            File.WriteAllText(filename + ".ppm", builder.ToString());
        }
    }

    public enum AntiAlias { None, SuperSampling2x2 }
}
