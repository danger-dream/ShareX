using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ShareX
{
    public static partial class NativeMethods
    {

        public static bool GetBorderSize(IntPtr handle, out Size size)
        {
            var wi = WINDOWINFO.Create();
            var result = GetWindowInfo(handle, ref wi);
            size = result ? new Size((int)wi.cxWindowBorders, (int)wi.cyWindowBorders) : Size.Empty;
            return result;
        }

        public static bool IsDWMEnabled()
        {
            return Helpers.IsWindowsVistaOrGreater() && DwmIsCompositionEnabled();
        }

        public static bool GetExtendedFrameBounds(IntPtr handle, out Rectangle rectangle)
        {
            int result = DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect, Marshal.SizeOf(typeof(RECT)));
            rectangle = rect;
            return result == 0;
        }


        public static Rectangle GetWindowRect(IntPtr handle)
        {
            GetWindowRect(handle, out RECT rect);
            return rect;
        }

        public static Rectangle GetClientRect(IntPtr handle)
        {
            GetClientRect(handle, out RECT rect);
            Point position = rect.Location;
            ClientToScreen(handle, ref position);
            return new Rectangle(position, rect.Size);
        }

        public static Rectangle MaximizedWindowFix(IntPtr handle, Rectangle windowRect)
        {
            if (GetBorderSize(handle, out Size size))
            {
                windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height, windowRect.Width - (size.Width * 2), windowRect.Height - (size.Height * 2));
            }

            return windowRect;
        }

        public static bool IsWindowCloaked(IntPtr handle)
        {
            if (IsDWMEnabled())
            {
                int result = DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.DWMWA_CLOAKED, out int cloaked, sizeof(int));
                return result == 0 && cloaked != 0;
            }

            return false;
        }

    }
}