using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ShareX
{
    public class CursorData
    {
        public IntPtr Handle { get; private set; }
        public Point Position { get; private set; }
        public Size Size { get; private set; }
        public float SizeMultiplier { get; private set; }
        public bool IsDefaultSize => SizeMultiplier == 1f;
        public Point Hotspot { get; private set; }
        public Point DrawPosition => new Point(Position.X - Hotspot.X, Position.Y - Hotspot.Y);
        public bool IsVisible { get; private set; }

        public CursorData()
        {
            UpdateCursorData();
        }

        public void UpdateCursorData()
        {
            Handle = IntPtr.Zero;
            Position = Point.Empty;
            IsVisible = false;

            var cursorInfo = new CursorInfo();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);

            if (!NativeMethods.GetCursorInfo(out cursorInfo)) return;
            Handle = cursorInfo.hCursor;
            Position = cursorInfo.ptScreenPos;
            Size = Size.Empty;
            SizeMultiplier = GetCursorSizeMultiplier();
            IsVisible = cursorInfo.flags == NativeConstants.CURSOR_SHOWING;

            if (!IsVisible) return;
            IntPtr iconHandle = NativeMethods.CopyIcon(Handle);

            if (iconHandle == IntPtr.Zero) return;
            if (NativeMethods.GetIconInfo(iconHandle, out IconInfo iconInfo))
            {
                Hotspot = IsDefaultSize ? new Point(iconInfo.xHotspot, iconInfo.yHotspot) : new Point((int)Math.Round(iconInfo.xHotspot * SizeMultiplier), (int)Math.Round(iconInfo.yHotspot * SizeMultiplier));

                if (iconInfo.hbmColor != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(iconInfo.hbmColor);
                }

                if (iconInfo.hbmMask != IntPtr.Zero)
                {
                    if (!IsDefaultSize)
                    {
                        using (var bmpMask = Image.FromHbitmap(iconInfo.hbmMask))
                        {
                            var cursorWidth = bmpMask.Width;
                            var cursorHeight = iconInfo.hbmColor != IntPtr.Zero ? bmpMask.Height : bmpMask.Height / 2;
                            Size = new Size((int)Math.Round(cursorWidth * SizeMultiplier), (int)Math.Round(cursorHeight * SizeMultiplier));
                        }
                    }
                    NativeMethods.DeleteObject(iconInfo.hbmMask);
                }
            }
            NativeMethods.DestroyIcon(iconHandle);
        }

        public static float GetCursorSizeMultiplier()
        {
            float sizeMultiplier = 1f;

            int? cursorSize = RegistryHelpers.GetValueDWord(@"SOFTWARE\Microsoft\Accessibility", "CursorSize");

            if (cursorSize != null && cursorSize > 1)
            {
                sizeMultiplier = 1f + ((cursorSize.Value - 1) * 0.5f);
            }

            return sizeMultiplier;
        }

        public Bitmap ToBitmap()
        {
            if (IsDefaultSize || Size.IsEmpty)
            {
                Icon icon = Icon.FromHandle(Handle);
                return icon.ToBitmap();
            }

            Bitmap bmp = new Bitmap(Size.Width, Size.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = g.GetHdc();

                NativeMethods.DrawIconEx(hdcDest, 0, 0, Handle, Size.Width, Size.Height, 0, IntPtr.Zero, NativeConstants.DI_NORMAL);

                g.ReleaseHdc(hdcDest);
            }

            return bmp;
        }
    }
}