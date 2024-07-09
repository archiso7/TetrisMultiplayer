using OpenTK.Windowing.Common;

namespace TetrisUI
{
    public class Menu : Screen
    {
        public List<GameObject> objects {get;set;}

        public Menu()
        {
            objects = new List<GameObject>();
        }

        public void HandleInput(KeyboardKeyEventArgs e)
        {
            
        }

        public void Update()
        {

        }
    }
}
