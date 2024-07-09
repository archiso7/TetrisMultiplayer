using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using Microsoft.FSharp.Collections;

namespace TetrisUI
{
    public interface Screen
    {
        public void HandleInput(KeyboardKeyEventArgs e);
        public void Update();
        List<GameObject> objects { get; set; }
    }

    public interface GameObject
    {
        public void Render(int _shaderProgram, Vector2? scale = null, Vector2? offset = null);
    }

    public class Poly : GameObject
    {
        protected int _vao;
        protected int _vbo;

        public List<Vector2> Vertices { get; set; }
        public List<Tri> Triangles { get; set; }
        public Color4 Color { get; set; }
        public Vector2 Position { get; set; }
        public List<List<Vector2>>? Holes { get; set; }

        public Poly(List<Vector2> vertices, Color4 color, Vector2 position, List<List<Vector2>>? holes = null)
        {
            (Vertices, List<Tri> Tris) = holes != null ? AddHoles(vertices ?? new List<Vector2>(), holes) : (vertices ?? new List<Vector2>(), new List<Tri>{});
            Holes = holes;
            Color = color;
            Position = position;
            Triangles = Tri.Triangulate(Vertices);
            Triangles.AddRange(Tris);
        }

        public (List<Vector2>, List<Tri>) AddHoles(List<Vector2> vertices, List<List<Vector2>> holes)
        {
            List<Vector2> resultVerts = new List<Vector2>(vertices); // Start with the original vertices
            List<Tri> resultTris = new List<Tri>();

            foreach (List<Vector2> hole in holes)
            {
                if (hole.Count == 0)
                {
                    continue; // Skip empty holes
                }

                Vector2 holeFirstPoint = hole[0];
                int closestIndex = 0;
                float minDistanceSquared = Vector2.DistanceSquared(holeFirstPoint, resultVerts[0]);

                for (int i = 1; i < resultVerts.Count; i++)
                {
                    float distanceSquared = Vector2.DistanceSquared(holeFirstPoint, resultVerts[i]);
                    if (distanceSquared < minDistanceSquared)
                    {
                        closestIndex = i;
                        minDistanceSquared = distanceSquared;
                    }
                }

                Vector2 closestPoint = resultVerts[closestIndex];
                Vector2 nextClosest = resultVerts[(closestIndex + 1) % resultVerts.Count];
                Vector2 holeLastPoint = hole[hole.Count - 1];

                // Add triangles to cover the gap created by the hole
                resultTris.Add(new Tri(new List<Vector2>{ holeFirstPoint, closestPoint, holeLastPoint }));
                resultTris.Add(new Tri(new List<Vector2> { closestPoint, nextClosest, holeLastPoint }));

                
                // Insert the hole's points after the closest point
                resultVerts.InsertRange(closestIndex + 1, hole);

                foreach (Vector2 point in resultVerts)
                {
                    Tri.Log("Vector2(" + point.X.ToString() + ", " + point.Y.ToString() + "),");
                }
            }

            return (resultVerts, resultTris);
        }

        public void Render(int _shaderProgram, Vector2? scale = null, Vector2? offset = null)
        {
            foreach (Tri triangle in Triangles)
            {
                triangle.Render(Position, Color, _shaderProgram, scale, offset);
            }

            // debug to see individual triangles
            // for (int i = 0; i < Triangles.Count; i++)
            // {
            //     float c = ((float)i/(float)Triangles.Count)+0.2f;
            //     Triangles[i].Render(Position, new Color4(c,c,c,1f), _shaderProgram, scale);
            // }
        }
    }

    public class Rectangle : Poly
    {
        public Rectangle(Vector2 size, Vector2 position, Color4 color)
            : base(new List<Vector2>(), color, position)
        {
            Vertices = VertsFromSize(size);
            Triangles = Tri.Triangulate(Vertices);
        }

        private List<Vector2> VertsFromSize(Vector2 size)
        {
            float halfWidth = size.X / 2;
            float halfHeight = size.Y / 2;

            return new List<Vector2>
            {
                new Vector2(-halfWidth, -halfHeight), // Bottom left
                new Vector2(halfWidth, -halfHeight),  // Bottom right
                new Vector2(halfWidth, halfHeight),   // Top right
                new Vector2(-halfWidth, halfHeight)   // Top left
            };
        }
    }

    public class Button : GameObject
    {
        public event EventHandler? Clicked;
        public Poly Polygon;
        public TextObject Text;
        public Color4 Color;
        public Vector2 Position;

        public Button(Poly polygon, TextObject text, Color4 color, Vector2 position)
        {
            Polygon = polygon;
            Text = text;
            Color = color;
            Position = position;
        }

        public void OnClick()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public bool ContainsPoint(Vector2 point)
        {
            float minX = Polygon.Vertices.Min(v => v.X) + Position.X;
            float maxX = Polygon.Vertices.Max(v => v.X) + Position.X;
            float minY = Polygon.Vertices.Min(v => v.Y) + Position.Y;
            float maxY = Polygon.Vertices.Max(v => v.Y) + Position.Y;

            return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        }

        public void Render(int _shaderProgram, Vector2? scale = null, Vector2? offset = null)
        {
            Vector2 Offset = offset ?? new Vector2(0,0);
            Offset += Position;
            Polygon.Render(_shaderProgram, scale, Offset);
            Text.Render(_shaderProgram, scale, Offset);
        }
    }

    public class TextObject : GameObject
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Color4 Color { get; set; }
        public Font Font { get; set; }
        private List<Poly> textPolygons { get; set; }

        public TextObject(string text, Vector2 position, Color4 color, Font font)
        {
            Text = text;
            Position = position;
            Color = color;
            Font = font;
            textPolygons = GenerateTextPolygons();
        }

        private List<Poly> GenerateTextPolygons()
        {
            List<Poly> polygons = new List<Poly>();

            var textOptions = new TextOptions(Font)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var glyphs = TextBuilder.GenerateGlyphs(Text, textOptions);

            foreach (var poly in glyphs)
            {
                if (poly is SixLabors.ImageSharp.Drawing.ComplexPolygon compPoly)
                {
                    List<List<Vector2>> paths = new List<List<Vector2>>();
                    foreach (SixLabors.ImageSharp.Drawing.Path path in compPoly.Paths)
                    {
                        List<Vector2> points = new List<Vector2>();
                        foreach (PointF point in path.Points.ToArray())
                        {
                            points.Add(new Vector2(point.X, point.Y));
                        }
                        paths.Add(points);
                    }

                    polygons.Add(new Poly(paths[0], Color, Position, paths.Skip(1).ToList()));
                }
                else
                {
                    List<Vector2> points = new List<Vector2>();
                    if (poly is SixLabors.ImageSharp.Drawing.Path simplePath)
                    {
                        foreach (PointF point in simplePath.Points.ToArray())
                        {
                            points.Add(new Vector2(point.X, point.Y));
                        }

                        polygons.Add(new Poly(points, Color, Position));
                    }
                }
            }

            return polygons;
        }

        private bool IsClockwise(List<Vector2> points)
        {
            float sum = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Count];
                sum += (next.X - current.X) * (next.Y + current.Y);
            }
            return sum > 0; // Clockwise if sum is positive
        }

        public void Render(int _shaderProgram, Vector2? scale = null, Vector2? offset = null)
        {
            Vector2 Scale = scale ?? new Vector2(1,1);
            Scale = Scale * new Vector2(1,-1);
            Vector2 Offset = offset ?? new Vector2(0,0);
            Offset = Offset * new Vector2(1,-1);

            int scaleLocation = GL.GetUniformLocation(_shaderProgram, "uScale");
            float[] currentScale = new float[2];
            GL.GetUniform(_shaderProgram, scaleLocation, currentScale);
            GL.Uniform2(scaleLocation, new Vector2(Scale.X*currentScale[0], Scale.Y*currentScale[1]));

            foreach (Poly polygon in textPolygons)
            {
                polygon.Render(_shaderProgram, scale, Offset);
            }
        }
    }

    public class Tri
    {
        protected int _vao;
        protected int _vbo;
        public float[] verts;

        public Tri(List<Vector2> vertices)
        {
            verts = VectorListToArray(vertices);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // Generate and bind the VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // Upload the vertex data to the GPU
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            // Define the vertex attribute layout
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Unbind the VBO and VAO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Render(Vector2 position, Color4 color, int _shaderProgram, Vector2? scale = null, Vector2? offset = null)
        {
            Vector2 Position = offset ?? new Vector2(0,0);
            Position += position;
            Vector2 Scale = scale ?? new Vector2(1,1);

            GL.UseProgram(_shaderProgram);

            int colorLocation = GL.GetUniformLocation(_shaderProgram, "uColor");
            if (colorLocation == -1)
            {
                Console.WriteLine("uColor uniform not found in shader.");
                return;
            }
            GL.Uniform4(colorLocation, color);

            int positionLocation = GL.GetUniformLocation(_shaderProgram, "uPosition");
            if (positionLocation == -1)
            {
                Console.WriteLine("uPosition uniform not found in shader.");
                return;
            }
            GL.Uniform2(positionLocation, Position);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, verts.Length / 2);
            GL.BindVertexArray(0);
        }

        public static void Log(string message)
        {
            using (StreamWriter sw = File.AppendText("debug_log.txt"))
            {
                sw.WriteLine($"{message}");
            }
        }

        public static List<Tri> Triangulate(List<Vector2> points){
            if (points == null || points.Count == 0)
                return new List<Tri>();

            // convert points to a list of the F# vector2 class
            FSharpList<TetrisCore.Types.Vector2> fsPoints = ConvertToFSList(points);

            //triangulate points
            FSharpList<FSharpList<TetrisCore.Types.Vector2>> triangulatedPoints = TetrisCore.Utils.triangulate(fsPoints);

            //convert to Tris
            List<Tri> tris = new List<Tri>();
            foreach (FSharpList<TetrisCore.Types.Vector2> triangle in triangulatedPoints)
            {
                List<Vector2> vector2s = new List<Vector2>();
                foreach (TetrisCore.Types.Vector2 vector in triangle)
                {
                    vector2s.Add(new Vector2((float)vector.X, (float)vector.Y));
                }
                tris.Add(new (vector2s));
            }

            return tris;
        }

        public static FSharpList<TetrisCore.Types.Vector2> ConvertToFSList(List<Vector2> points)
        {
            var fSharpList = new List<TetrisCore.Types.Vector2>();

            foreach (var point in points)
            {
                fSharpList.Add(new TetrisCore.Types.Vector2(point.X, point.Y));
            }

            return ListModule.OfSeq(fSharpList);
        }

        public float[] VectorListToArray(List<Vector2> vertices)
        {
            float[] array = new float[vertices.Count * 2]; // Only X and Y

            for (int i = 0; i < vertices.Count; i++)
            {
                array[i * 2] = vertices[i].X;
                array[i * 2 + 1] = vertices[i].Y;
            }
            return array;
        }
    }
}