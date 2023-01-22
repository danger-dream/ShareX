using System.Drawing;

namespace ShareX
{
    public struct Rgba
    {
        private int red, green, blue, alpha;

        public int Red
        {
            get => red;
            set => red = ColorHelpers.ValidColor(value);
        }

        public int Green
        {
            get => green;
            set => green = ColorHelpers.ValidColor(value);
        }

        public int Blue
        {
            get => blue;
            set => blue = ColorHelpers.ValidColor(value);
        }

        public int Alpha
        {
            get => alpha;
            set => alpha = ColorHelpers.ValidColor(value);
        }

        public Rgba(int red, int green, int blue, int alpha = 255) : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public Rgba(Color color) : this(color.R, color.G, color.B, color.A)
        {
        }

        public static implicit operator Rgba(Color color)
        {
            return new Rgba(color);
        }

        public static implicit operator Color(Rgba color)
        {
            return color.ToColor();
        }

        public static implicit operator HSB(Rgba color)
        {
            return color.ToHsb();
        }

        public static implicit operator CMYK(Rgba color)
        {
            return color.ToC();
        }

        public static bool operator ==(Rgba left, Rgba right)
        {
            return (left.Red == right.Red) && (left.Green == right.Green) && (left.Blue == right.Blue) && (left.Alpha == right.Alpha);
        }

        public static bool operator !=(Rgba left, Rgba right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"R: {Red}, G: {Green}, B: {Blue}, A: {Alpha}";
        }

        public Color ToColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }

        public HSB ToHsb()
        {
            return ColorHelpers.ColorToHsb(this);
        }

        public CMYK ToC()
        {
            return ColorHelpers.ColorToC(this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}