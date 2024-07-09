using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using SixLabors.Fonts;

namespace TetrisUI
{
    public class TetrisGameWindow : GameWindow
    {
        private Game game;
        private Menu mainMenu;
        private Menu multiMenu;
        private Menu pauseMenu;
        private Renderer renderer;
        private InputManager inputManager;
        public Screen currentScreen;
        public List<Screen> ScreenStack =  new List<Screen>();

        public TetrisGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Font f = SystemFonts.CreateFont("FreeSerif", 200, FontStyle.Regular);
            game = new Game();

            game.objects.AddRange(new List<GameObject>{
                new TextObject("TETRIS", new Vector2(0,-500), new Color4(0.1f,0.4f,0.2f,0f), f),
            });

            mainMenu = new Menu();
            
            var singleButtonPoly = new Rectangle(new Vector2(500, 120), new Vector2(0, 0), new Color4(0f, 0.5f, 0f, 1f));
            var singleButtonText = new TextObject("Singleplayer", new Vector2(0, 0), Color4.White, SystemFonts.CreateFont("FreeSerif", 100, FontStyle.Regular));
            var singleButton = new Button(singleButtonPoly, singleButtonText, new Color4(0f, 0.5f, 0f, 1f), new Vector2(0, 0));
            singleButton.Clicked += SingleButton;
            var multiButtonPoly = new Rectangle(new Vector2(500, 120), new Vector2(0), new Color4(0f, 0.5f, 0f, 1f));
            var multiButtonText = new TextObject("Multiplayer", new Vector2(0, 0), Color4.White, SystemFonts.CreateFont("FreeSerif", 100, FontStyle.Regular));
            var multiButton = new Button(multiButtonPoly, multiButtonText, new Color4(0f, 0.5f, 0f, 1f), new Vector2(0, -140));
            multiButton.Clicked += MultiButton;

            mainMenu.objects = new List<GameObject>{
                new TextObject("TETRIS", new Vector2(0,-400), new Color4(0.1f,0.4f,0.2f,0f), f),
                singleButton,
                multiButton,
            };

            multiMenu = new Menu();

            pauseMenu = new Menu();
            renderer = new Renderer(new Vector2(1280, 720));
            inputManager = new InputManager(this);
            currentScreen = mainMenu;
        }

        public void SingleButton(object? sender, EventArgs e)
        {
            currentScreen = game;
        }

        public void MultiButton(object? sender, EventArgs e)
        {
            currentScreen = multiMenu;
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
            currentScreen.Update();
        }
    }
}