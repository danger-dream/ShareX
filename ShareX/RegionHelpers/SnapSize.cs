using System;
using System.Drawing;

namespace ShareX
{
    public class SnapSize
    {
        private const int MinimumWidth = 2;

        private int width;

        public int Width
        {
            get => width;
            set => width = Math.Max(value, MinimumWidth);
        }

        private const int MinimumHeight = 2;

        private int height;

        public int Height
        {
            get => height;
            set => height = Math.Max(value, MinimumHeight);
        }

        public SnapSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static implicit operator Size(SnapSize size)
        {
            return new Size(size.Width, size.Height);
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}