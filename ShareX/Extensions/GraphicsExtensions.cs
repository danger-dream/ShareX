using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public static class GraphicsExtensions
    {
        public static void DrawRectangleProper(this Graphics g, Pen pen, RectangleF rect)
        {
            if (pen.Width == 1)
            {
                rect = rect.SizeOffset(-1);
            }

            if (rect.Width > 0 && rect.Height > 0)
            {
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        public static void DrawRectangleProper(this Graphics g, Pen pen, int x, int y, int width, int height)
        {
            DrawRectangleProper(g, pen, new Rectangle(x, y, width, height));
        }

        public static void DrawDiamond(this Graphics g, Pen pen, Rectangle rect)
        {
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddDiamond(rect);
                g.DrawPath(pen, gp);
            }
        }

        public static void DrawTextWithShadow(this Graphics g, string text, PointF position, Font font, Brush textBrush, Brush shadowBrush)
        {
            DrawTextWithShadow(g, text, position, font, textBrush, shadowBrush, new Point(1, 1));
        }

        public static void DrawTextWithShadow(this Graphics g, string text, PointF position, Font font, Brush textBrush, Brush shadowBrush, Point shadowOffset)
        {
            g.DrawString(text, font, shadowBrush, position.X + shadowOffset.X, position.Y + shadowOffset.Y);
            g.DrawString(text, font, textBrush, position.X, position.Y);
        }
    }
}