using OpenTK.Mathematics; // for Vector2
using OpenTK.Graphics.OpenGL4; // for Color

namespace TetrisUI
{
    public class Menu : Screen
    {
        public List<GameObject> objects {get;set;}

        public Menu()
        {
            objects = new List<GameObject>();
        }

        public void Render()
        {

        }

        public void HandleInput()
        {
            
        }

        public void Update()
        {

        }
    }

    public class Button : Poly
    {
        public event EventHandler? Clicked;

        public Button(List<Vector2> vertices, Color4 color, Vector2 position)
            : base(vertices, color, position)
        {
        }

        public void OnClick()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
