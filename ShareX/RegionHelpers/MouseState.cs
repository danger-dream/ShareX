using System.Drawing;
using System.Windows.Forms;

namespace ShareX
{
    public struct MouseState
    {
        public MouseButtons Buttons { get; private set; }
        public Point Position { get; private set; }
        public Point ClientPosition { get; private set; }

        public void Update(Control control)
        {
            Buttons = Control.MouseButtons;
            Position = Control.MousePosition;

            ClientPosition = control?.PointToClient(Position) ?? CaptureHelpers.ScreenToClient(Position);
        }
    }
}