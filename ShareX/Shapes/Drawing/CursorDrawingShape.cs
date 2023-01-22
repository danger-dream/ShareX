using System.Drawing;

namespace ShareX
{
    public class CursorDrawingShape : ImageDrawingShape
    {
        public override ShapeType ShapeType => ShapeType.DrawingCursor;

        public void UpdateCursor(Bitmap bmpCursor, Point position)
        {
            Image = bmpCursor;
            Rectangle = new Rectangle(position, Image.Size);
        }


        public override void OnCreating()
        {
            Manager.IsMoving = true;
            OnCreated();
        }

        public override void OnDraw(Graphics g)
        {
            if (Image == null) return;
            g.DrawImage(Image, Rectangle);

            if (!Manager.IsRenderingOutput && Manager.CurrentTool == ShapeType.DrawingCursor)
            {
                Manager.DrawRegionArea(g, Rectangle.Round(), false);
            }
        }

        public override void Resize(int x, int y, bool fromBottomRight)
        {
            Move(x, y);
        }
    }
}