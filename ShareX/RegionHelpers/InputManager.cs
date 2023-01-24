using System.Drawing;
using System.Windows.Forms;

namespace ShareX
{
    public class InputManager
    {
        public Point MousePosition => mouseState.Position;

        public Point ClientMousePosition => mouseState.ClientPosition;

        public Point PreviousClientMousePosition => oldMouseState.ClientPosition;

        public Point MouseVelocity => new Point(ClientMousePosition.X - PreviousClientMousePosition.X, ClientMousePosition.Y - PreviousClientMousePosition.Y);

        private MouseState mouseState;
        private MouseState oldMouseState;

        public void Update(Control control)
        {
            oldMouseState = mouseState;
            mouseState.Update(control);
        }

        public bool IsMouseDown(MouseButtons button)
        {
            return mouseState.Buttons.HasFlag(button);
        }
    }
}