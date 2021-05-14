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

            Vector3[] colors =
            {
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)
            };

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

            DrawTriangle(image, screen[0], screen[1], screen[2], colors[0], colors[1], colors[2], AntiAlias.SuperSampling2x2);
            ExportImage(image, "output");
            //ExportImage(image, DateTime.Now.ToString());
        }

        private static void DrawTriangle(
            Vector3[,] image,
            Vector2 vertexA,
            Vector2 vertexB,
            Vector2 vertexC,
            Vector3 colorA,
            Vector3 colorB,
            Vector3 colorC,
            AntiAlias antiAlias)
        {
            // TODO squares with early in/out

            float[] xValues = { vertexA.X, vertexB.X, vertexC.X };
            float[] yValues = { vertexA.Y, vertexB.Y, vertexC.Y };

            var minX = (int)Math.Floor(xValues.Min());
            var maxX = (int)Math.Ceiling(xValues.Max());
            var minY = (int)Math.Floor(yValues.Min());
            var maxY = (int)Math.Ceiling(yValues.Max());
            
            float Area(Vector2 a, Vector2 b, Vector2 c) => .5f * Tools2D.Cross(Vector2.Subtract(b, a), Vector2.Subtract(c, a));

            var area = Area(vertexA, vertexB, vertexC);

            float PhiA(Vector2 x) => Area(x, vertexB, vertexC) / area;
            float PhiB(Vector2 x) => Area(x, vertexC, vertexA) / area;
            float PhiC(Vector2 x) => Area(x, vertexA, vertexB) / area;

            Vector3 Color(Vector2 x) => colorA * PhiA(x) + colorB * PhiB(x) + colorC * PhiC(x);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var fragment = new Vector2(x + .5f, y + .5f); // TODO: is fragment the right name?
                    var color = Color(fragment);

                    switch (antiAlias)
                    {
                        case AntiAlias.None:
                        {
                            if (Tools2D.PointInTriangle(fragment, vertexA, vertexB, vertexC))
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

                            var r = (float)points.Count(p => Tools2D.PointInTriangle(p, vertexA, vertexB, vertexC)) / points.Count();

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