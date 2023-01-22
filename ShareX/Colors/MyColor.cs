using System.Drawing;

namespace ShareX
{
    public struct MyColor
    {
        public Rgba RGBA;
        public HSB HSB;
        public CMYK CMYK;


        public MyColor(Color color)
        {
            RGBA = color;
            HSB = color;
            CMYK = color;
        }

        public static implicit operator MyColor(Color color)
        {
            return new MyColor(color);
        }

        public static implicit operator Color(MyColor color)
        {
            return color.RGBA;
        }

        public static bool operator ==(MyColor left, MyColor right)
        {
            return (left.RGBA == right.RGBA) && (left.HSB == right.HSB) && (left.CMYK == right.CMYK);
        }

        public static bool operator !=(MyColor left, MyColor right)
        {
            return !(left == right);
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