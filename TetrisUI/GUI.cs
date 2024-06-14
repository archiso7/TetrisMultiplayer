using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using static TetrisUI.Renderer;

namespace TetrisUI
{
    public interface Screen
    {
        public void Render();
        public void HandleInput();
        public void Update();
        List<GameObject> objects { get; set; }
    }

    public interface GameObject
    {
        public void Render(int _shaderProgram);
    }

    public class Poly : GameObject
    {
        protected int _vao;
        protected int _vbo;

        public List<Vector2> Vertices { get; set; }
        public List<Tri> Triangles { get; set; }
        public Color4 Color { get; set; }
        public Vector2 Position { get; set; }

        public Poly(List<Vector2> vertices, Color4 color, Vector2 position)
        {
            Vertices = vertices ?? new List<Vector2>(); // Initialize Vertices
            Color = color;
            Position = position;
            Triangles = Tri.Triangulate(Vertices);
        }

        public void Render(int _shaderProgram)
        {
            foreach (Tri triangle in Triangles)
            {
                triangle.Render(Position, Color, _shaderProgram);
            }

            // debug to see individual triangles
            // for (int i = 0; i < Triangles.Count; i++)
            // {
            //     float c = ((float)i/(float)Triangles.Count)+0.2f;
            //     Triangles[i].Render(Position, new Color4(c,c,c,1f));
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

    public class TextObject : GameObject
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Color4 Color { get; set; }

        public TextObject(string text, Vector2 position, Color4 color)
        {
            Text = text;
            Position = position;
            Color = color;
        }

        public void Render(int _shaderProgram)
        {

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

        public void Render(Vector2 position, Color4 color, int _shaderProgram)
        {
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
            GL.Uniform2(positionLocation, position);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, verts.Length / 2);
            GL.BindVertexArray(0);
        }

        public static Vertex VertFromVector(Vector2 vector)
        {
            return new Vertex(vector.X, vector.Y);
        }

        public static Vector2 VectorFromVert(Vertex vertex)
        {
            return new Vector2((float)vertex.X, (float)vertex.Y);
        }

        public static List<Tri> Triangulate(List<Vector2> points)
        {
            if (points == null || points.Count == 0)
                // throw new ArgumentException("Points list cannot be null or empty.");
                return new List<Tri>();

            Polygon polygon = new Polygon();

            foreach (Vector2 point in points)
            {
                polygon.Add(VertFromVector(point));
            }

            var mesh = polygon.Triangulate();

            List<Tri> tris = new List<Tri>();

            foreach (var triangle in mesh.Triangles)
            {
                tris.Add(new Tri(new List<Vector2>
                {
                    VectorFromVert(triangle.GetVertex(0)),
                    VectorFromVert(triangle.GetVertex(1)),
                    VectorFromVert(triangle.GetVertex(2))
                }));
            }

            return tris;
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