using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TetrisUI
{
    public class InputManager
    {
        // Define delegates for events
        public event Action<KeyboardKeyEventArgs>? KeyPressed;
        public event Action<MouseButtonEventArgs>? MouseClicked;

        public InputManager(GameWindow window)
        {
            // Subscribe to keyboard and mouse events
            window.KeyDown += HandleKeyDown;
            window.MouseDown += HandleMouseDown;
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
        }

        public void HandleInput(Screen screen)
        {
            // Handle input
        }
    }
}
