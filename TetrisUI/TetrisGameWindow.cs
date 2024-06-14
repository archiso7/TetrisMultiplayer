using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using static TetrisCore.Types;
using static TetrisCore.GameLogic;

namespace TetrisUI
{
    public class TetrisGameWindow : GameWindow
    {
        private Game game;
        private Menu mainMenu;
        private Menu pauseMenu;
        private Renderer renderer;
        private InputManager inputManager;
        private Screen currentScreen;

        public TetrisGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            game = new Game();
            mainMenu = new Menu();
            mainMenu.objects = new List<GameObject>{
                new Rectangle(new Vector2(500f,500f), new Vector2(-500f,0f), new Color4(0.5f,0f,0f,1f)),
                new Rectangle(new Vector2(500f,500f), new Vector2(0f,0f), new Color4(0f,0.5f,0f,1f)),
                new Rectangle(new Vector2(500f,500f), new Vector2(500f,0f), new Color4(0f,0f,0.5f,1f))
            };

            pauseMenu = new Menu();
            renderer = new Renderer(new Vector2(1280, 720));
            inputManager = new InputManager(this);
            currentScreen = mainMenu;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            // Initialization code here
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            renderer.UpdateWindowSize(new Vector2(e.Width, e.Height));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            renderer.Render(currentScreen, this);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            inputManager.HandleInput(currentScreen);
            currentScreen.Update();
        }
    }
}