using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX
{
    public static class CaptureHelpers
    {
        public static Rectangle GetScreenBounds()
        {
            return SystemInformation.VirtualScreen;
        }

        public static Rectangle GetActiveScreenBounds()
        {
            return Screen.FromPoint(GetCursorPosition()).Bounds;
        }

        public static Point ScreenToClient(Point p)
        {
            int screenX = NativeMethods.GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN);
            int screenY = NativeMethods.GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN);
            return new Point(p.X - screenX, p.Y - screenY);
        }

        public static Point GetCursorPosition()
        {
            if (NativeMethods.GetCursorPos(out POINT point))
            {
                return (Point)point;
            }

            return Point.Empty;
        }

        public static RectangleF CreateRectangle(float x, float y, float x2, float y2)
        {
            float width, height;

            if (x <= x2)
            {
                width = x2 - x + 1;
            }
            else
            {
                width = x - x2 + 1;
                x = x2;
            }

            if (y <= y2)
            {
                height = y2 - y + 1;
            }
            else
            {
                height = y - y2 + 1;
                y = y2;
            }

            return new RectangleF(x, y, width, height);
        }

        public static RectangleF CreateRectangle(PointF pos, PointF pos2)
        {
            return CreateRectangle(pos.X, pos.Y, pos2.X, pos2.Y);
        }

        public static PointF SnapPositionToDegree(PointF pos, PointF pos2, float degree, float startDegree)
        {
            float angle = MathHelpers.LookAtRadian(pos, pos2);
            float startAngle = MathHelpers.DegreeToRadian(startDegree);
            float snapAngle = MathHelpers.DegreeToRadian(degree);
            float newAngle = ((float)Math.Round((angle + startAngle) / snapAngle) * snapAngle) - startAngle;
            float distance = MathHelpers.Distance(pos, pos2);
            return pos.Add((PointF)MathHelpers.RadianToVector2(newAngle, distance));
        }

        public static PointF CalculateNewPosition(PointF posOnClick, PointF posCurrent, Size size)
        {
            if (posCurrent.X > posOnClick.X)
            {
                return posCurrent.Y > posOnClick.Y ? new PointF(posOnClick.X + size.Width - 1, posOnClick.Y + size.Height - 1) : new PointF(posOnClick.X + size.Width - 1, posOnClick.Y - size.Height + 1);
            }

            return posCurrent.Y > posOnClick.Y ? new PointF(posOnClick.X - size.Width + 1, posOnClick.Y + size.Height - 1) : new PointF(posOnClick.X - size.Width + 1, posOnClick.Y - size.Height + 1);
        }

        public static Rectangle GetWindowRectangle(IntPtr handle)
        {
            Rectangle rect = Rectangle.Empty;

            if (NativeMethods.IsDWMEnabled() && NativeMethods.GetExtendedFrameBounds(handle, out Rectangle tempRect))
            {
                rect = tempRect;
            }

            if (rect.IsEmpty)
            {
                rect = NativeMethods.GetWindowRect(handle);
            }
            if (!(Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= -1) && NativeMethods.IsZoomed(handle))
            {
                rect = NativeMethods.MaximizedWindowFix(handle, rect);
            }

            return rect;
        }
    }
}