using System;
using System.Collections.Generic;
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
            var zBuffer = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    image[x, y] = Vector3.One; // Set color of image to white
                    zBuffer[x, y] = float.PositiveInfinity;
                }
            }

            // World coordinates
            // Camera and model

            var triangles = TriangulateCube(1.5f);

            /*var triangles = new List<Vector3[]>
            {
                new Vector3[]
                {
                    new Vector3(-3, -3, 0),
                    new Vector3(3, -3, 0),
                    new Vector3(0, 3, 0),
                },
                new Vector3[]
                {
                    new Vector3(-3, -3, -1),
                    new Vector3(3, -3, -1),
                    new Vector3(0, 3, -1),
                },
                new Vector3[]
                {
                    new Vector3(-3, -3, 1),
                    new Vector3(3, -3, 1),
                    new Vector3(0, 3, 1),
                }
            };*/

            var red = new Vector3(1, 0, 0);
            var green = new Vector3(0, 1, 0);
            var blue = new Vector3(0, 0, 1);
            var yellow = new Vector3(1, 1, 0);
            var aqua = new Vector3(0, 1, 1);
            var fuschia = new Vector3(1, 0, 1);

            var colors = new List<Vector3>
            {
                red, red, green, green, blue, blue, yellow, yellow, aqua, aqua, fuschia, fuschia 
                //red, green, blue, red, green, blue, red, green, blue, red, green, blue, red, green, blue, red, green, blue
            };

            var cameraPosition = new Vector3(2, 2, 2);
            var cameraTarget = new Vector3(0, 0, 0);
            var cameraDirection = Vector3.Normalize(cameraPosition - cameraTarget); // Note: this is actually pointing in the opposite direction

            // Camera is looking down at -z direction, all objects are expressed relative to camera

            // Translate camera to origin, rotate to look in -z direction
            Vector3 up = new Vector3(0, 1, 0); // Some up vector, not orthogonal to the camera
            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(up, cameraDirection));
            Vector3 cameraUp = Vector3.Cross(cameraDirection, cameraRight);

            var lookAt = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUp); // View
            var perspective = Matrix4x4.CreatePerspective(6, 3, 1, 10); // Projection

            Matrix4x4[] matrices = {
                lookAt,
                perspective,
                Matrix4x4.CreateScale(1, -1, 1),
                Matrix4x4.CreateTranslation(1, 1, 0),
                Matrix4x4.CreateScale(width * .5f, height * .5f, 1),
            };
 
            var transform = Tools2D.Multiply(matrices);

            foreach (var triangle in triangles)
            {
                var transformed = triangle.Select(p => Vector4.Transform(p, transform)).ToArray();
                var screen = transformed.Select(p => new Vector2(p.X / p.W, p.Y / p.W)).ToArray();
                var depths = transformed.Select(v => v.Z / v.W).ToArray();
                var color = colors[triangles.IndexOf(triangle)];

                DrawTriangle(image, zBuffer, screen[0], screen[1], screen[2], color, color, color, depths, AntiAlias.None);
            }

            ExportImage(image, "output");
            ExportImage(ZBufferToImage(zBuffer), "zbuffer");
            //ExportImage(image, DateTime.Now.ToString());
        }

        private static void DrawTriangle(
            Vector3[,] image,
            float [,] zBuffer,
            Vector2 vertexA,
            Vector2 vertexB,
            Vector2 vertexC,
            Vector3 colorA,
            Vector3 colorB,
            Vector3 colorC,
            float[] depths,
            AntiAlias antiAlias)
        {
            // TODO squares with early in/out

            int imageWidth = image.GetLength(0);
            int imageHeight = image.GetLength(1);

            float[] xValues = { vertexA.X, vertexB.X, vertexC.X };
            float[] yValues = { vertexA.Y, vertexB.Y, vertexC.Y };

            int minX = Math.Max(0, (int)Math.Floor(xValues.Min()));
            int maxX = Math.Min(imageWidth - 1, (int)Math.Ceiling(xValues.Max()));
            int minY = Math.Max(0, (int)Math.Floor(yValues.Min()));
            int maxY = Math.Min(imageHeight - 1, (int)Math.Ceiling(yValues.Max()));


            // TODO: move some of these functions out

            float Area(Vector2 a, Vector2 b, Vector2 c) => .5f * Tools2D.Cross(Vector2.Subtract(b, a), Vector2.Subtract(c, a));

            var area = Area(vertexA, vertexB, vertexC);

            float PhiA(Vector2 x) => Area(x, vertexB, vertexC) / area;
            float PhiB(Vector2 x) => Area(x, vertexC, vertexA) / area;
            float PhiC(Vector2 x) => Area(x, vertexA, vertexB) / area;

            Vector3 Color(Vector2 x) => colorA * PhiA(x) + colorB * PhiB(x) + colorC * PhiC(x);
            float Depth(Vector2 x) => depths[0] * PhiA(x) + depths[1] * PhiB(x) + depths[2] * PhiC(x);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var fragment = new Vector2(x + .5f, y + .5f); // TODO: is fragment the right name?
                    var color = Color(fragment);
                    var depth = Depth(fragment);

                    if (depth > zBuffer[x, y])
                        continue;

                    switch (antiAlias)
                    {
                        case AntiAlias.None:
                        {
                            if (Tools2D.PointInTriangle(fragment, vertexA, vertexB, vertexC))
                            {
                                image[x, y] = color;
                                zBuffer[x, y] = depth;
                            }

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

                            if (r > 0)
                            {
                                image[x, y] = Vector3.Lerp(image[x, y], color, r);
                                zBuffer[x, y] = depth;
                            }

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

        public static List<Vector3[]> TriangulateCube(float scale = 1)
        {
            var a = Vector3.Multiply(scale, new Vector3(1, 1, 1));
            var b = Vector3.Multiply(scale, new Vector3(1, 1, -1));
            var c = Vector3.Multiply(scale, new Vector3(1, -1, 1));
            var d = Vector3.Multiply(scale, new Vector3(1, -1, -1));
            var e = Vector3.Multiply(scale, new Vector3(-1, 1, 1));
            var f = Vector3.Multiply(scale, new Vector3(-1, 1, -1));
            var g = Vector3.Multiply(scale, new Vector3(-1, -1, 1));
            var h = Vector3.Multiply(scale, new Vector3(-1, -1, -1));

            var result = new List<Vector3[]>();

            result.AddRange(TriangulateFace(a, c, b, d));
            result.AddRange(TriangulateFace(a, b, e, f));
            result.AddRange(TriangulateFace(e, f, h, g));
            result.AddRange(TriangulateFace(c, d, h, g));
            result.AddRange(TriangulateFace(a, c, g, e));
            result.AddRange(TriangulateFace(b, d, h, f));

            return result;
        }

        public static List<Vector3[]> TriangulateFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return new List<Vector3[]>
            {
                new Vector3[] { a, b, c },
                new Vector3[] { c, d, a },
            };
        }

        public static List<Vector3[]> TriangulateTetrahedron(float scale = 1f)
        {
            float z = 1.0f / (float)Math.Sqrt(2);

            var a = new Vector3(1, 0, -z);
            var b = new Vector3(-1, 0, -z);
            var c = new Vector3(0, 1, z);
            var d = new Vector3(0, -1, z);

            a = Vector3.Multiply(scale, a);
            b = Vector3.Multiply(scale, b);
            c = Vector3.Multiply(scale, c);
            d = Vector3.Multiply(scale, d);

            return new List<Vector3[]> {
                new Vector3[] { a, b, c },
                new Vector3[] { a, b, d },
                new Vector3[] { a, c, d },
                new Vector3[] { b, c, d },
            };
        }

        public static Vector3[,] ZBufferToImage(float[,] zBuffer)
        {
            var width = zBuffer.GetLength(0);
            var height = zBuffer.GetLength(1);

            var result = new Vector3[width, height];

            IEnumerable<float> allValues = zBuffer.Cast<float>();
            float min = allValues.Where(v => !float.IsInfinity(v)).Min();
            float max = allValues.Where(v => !float.IsInfinity(v)).Max();
            float size = max - min;
            bool noDepth = Math.Abs(min - max) < 0.0001;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var value = zBuffer[x, y];

                    if (value == float.PositiveInfinity)
                    {
                        result[x, y] = Vector3.Zero;
                    }
                    else
                    {
                        if (noDepth)
                            result[x, y] = float.IsInfinity(value) ? Vector3.Zero : Vector3.One;
                        else
                            result[x, y] = new Vector3(Lerp(1f, 0.2f, (value - min) / size));
                    }
                }
            }

            return result;
        }

        public static float Lerp(float v1, float v2, float ratio)
        {
            return v1 * (1 - ratio) + v2 * ratio;
        }
    }

    public enum AntiAlias { None, SuperSampling2x2 }
}