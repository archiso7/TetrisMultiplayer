using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace TetrisUI
{
    public class InputManager
    {
        // Define delegates for events
        public event Action<KeyboardKeyEventArgs>? KeyPressed;
        public event Action<MouseButtonEventArgs>? MouseClicked;
        public TetrisGameWindow Window;

        public InputManager(TetrisGameWindow window)
        {
            // Subscribe to keyboard and mouse events
            Window = window;
            Window.KeyDown += HandleKeyDown;
            Window.MouseDown += HandleMouseDown;
        }

        private void HandleKeyDown(KeyboardKeyEventArgs args)
        {
            // Invoke the KeyPressed event when a key is pressed
            KeyPressed?.Invoke(args);
        }

        private void HandleMouseDown(MouseButtonEventArgs args)
        {
            // Invoke the MouseClicked event when a mouse button is clicked
            MouseClicked?.Invoke(args);
            CheckButtonClicks(args);
        }

        private void CheckButtonClicks(MouseButtonEventArgs args)
        {
            // Get the mouse position
            var mouseState = Window.MouseState;
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 worldMousePos = ScreenToWorld(mousePos);

            Screen screen = Window.currentScreen;

            foreach (GameObject obj in screen.objects)
            {
                if (obj is Button button && button.ContainsPoint(worldMousePos))
                {
                    button.OnClick();
                    break;
                }
            }
        }

        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            float worldX = (screenPos.X - Window.Size.X / 2f) * 2f;
            float worldY = -(screenPos.Y - Window.Size.Y / 2f) * 2f;
            return new Vector2(worldX, worldY);
        }

        public void HandleInput(Screen screen)
        {
            
        }
    }
}
