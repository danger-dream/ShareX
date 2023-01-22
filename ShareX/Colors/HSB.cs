using System.Drawing;

namespace ShareX
{
    public struct HSB
    {
        private double hue;
        private double saturation;
        private double brightness;
        private int alpha;

        public double Hue
        {
            get
            {
                return hue;
            }
            set
            {
                hue = ColorHelpers.ValidColor(value);
            }
        }

        public double Hue360
        {
            get
            {
                return hue * 360;
            }
            set
            {
                hue = ColorHelpers.ValidColor(value / 360);
            }
        }

        public double Saturation
        {
            get
            {
                return saturation;
            }
            set
            {
                saturation = ColorHelpers.ValidColor(value);
            }
        }

        public double Saturation100
        {
            get
            {
                return saturation * 100;
            }
            set
            {
                saturation = ColorHelpers.ValidColor(value / 100);
            }
        }

        public double Brightness
        {
            get
            {
                return brightness;
            }
            set
            {
                brightness = ColorHelpers.ValidColor(value);
            }
        }

        public double Brightness100
        {
            get
            {
                return brightness * 100;
            }
            set
            {
                brightness = ColorHelpers.ValidColor(value / 100);
            }
        }

        public int Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = ColorHelpers.ValidColor(value);
            }
        }

        public HSB(double hue, double saturation, double brightness, int alpha = 255) : this()
        {
            Hue = hue;
            Saturation = saturation;
            Brightness = brightness;
            Alpha = alpha;
        }

        public HSB(int hue, int saturation, int brightness, int alpha = 255) : this()
        {
            Hue360 = hue;
            Saturation100 = saturation;
            Brightness100 = brightness;
            Alpha = alpha;
        }

        public HSB(Color color)
        {
            this = ColorHelpers.ColorToHsb(color);
        }

        public static implicit operator HSB(Color color)
        {
            return ColorHelpers.ColorToHsb(color);
        }

        public static implicit operator Color(HSB color)
        {
            return color.ToColor();
        }

        public static implicit operator Rgba(HSB color)
        {
            return color.ToColor();
        }

        public static implicit operator CMYK(HSB color)
        {
            return color.ToColor();
        }

        public static bool operator ==(HSB left, HSB right)
        {
            return (left.Hue == right.Hue) && (left.Saturation == right.Saturation) && (left.Brightness == right.Brightness);
        }

        public static bool operator !=(HSB left, HSB right)
        {
            return !(left == right);
        }

        public Color ToColor()
        {
            return ColorHelpers.HsbToColor(this);
        }
    }
}