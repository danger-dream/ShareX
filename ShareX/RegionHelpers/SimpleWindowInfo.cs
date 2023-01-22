using System;
using System.Drawing;

namespace ShareX
{
    public class SimpleWindowInfo
    {
        public IntPtr Handle { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool IsWindow { get; set; }

        public SimpleWindowInfo(IntPtr handle)
        {
            Handle = handle;
        }

        public SimpleWindowInfo(IntPtr handle, Rectangle rect)
        {
            Handle = handle;
            Rectangle = rect;
        }
    }
}