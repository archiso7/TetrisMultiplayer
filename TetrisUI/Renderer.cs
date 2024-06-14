using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;

namespace TetrisUI
{
    public class Renderer
    {
        private int shaderProgram;
        private Vector2 windowSize;

        public Renderer(Vector2 initialWindowSize)
        {
            windowSize = initialWindowSize;
            string vertexShaderSource = LoadShaderSource("vertex_shader.glsl");
            string fragmentShaderSource = LoadShaderSource("fragment_shader.glsl");
            shaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
        }

        public void UpdateWindowSize(Vector2 newSize)
        {
            windowSize = newSize;
        }

        public void Render(Screen screen, GameWindow window)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int windowSizeLocation = GL.GetUniformLocation(shaderProgram, "uWindowSize");
            GL.Uniform2(windowSizeLocation, new Vector2(1280, 720));

            if (screen.objects is not null)
            {
                foreach (var obj in screen.objects)
                {
                    // Calculate scaling factors based on window size and aspect ratio
                    float scaleX = windowSize.X / 1280f; // Y is the base height (720)
                    float scaleY = windowSize.Y / 720f; // Maintain aspect ratio
                    float scale = Math.Min(scaleX, scaleY);

                    // Update the scaling factors in the shader
                    int scaleLocation = GL.GetUniformLocation(shaderProgram, "uScale");
                    GL.Uniform2(scaleLocation, new Vector2(scale, scale));

                    // Calculate the position offset for centering after scaling
                    Vector2 positionOffset = CalculatePositionOffset(windowSize, new Vector2(1280f, 720f));

                    // Update the position offset in the shader
                    int positionOffsetLocation = GL.GetUniformLocation(shaderProgram, "uPositionOffset");
                    GL.Uniform2(positionOffsetLocation, positionOffset);

                    // Render the object
                    obj.Render(shaderProgram);
                }
            }

            window.SwapBuffers();
        }

        private Vector2 CalculatePositionOffset(Vector2 windowSize, Vector2 baseSize)
        {
            return (windowSize - baseSize) / 2f;
        }

        public static int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(shaderProgram);
                throw new Exception($"Error linking shader program: {infoLog}");
            }
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        public static string LoadShaderSource(string filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }

        public static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type}: {infoLog}");
            }
            return shader;
        }
    }
}
