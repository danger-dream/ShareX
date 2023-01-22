using System;
using System.Drawing;

namespace ShareX
{
    public static class ColorHelpers
    {
        #region Convert Color to ...

        public static string ColorToHex(Color color, ColorFormat format = ColorFormat.RGB)
        {
            switch (format)
            {
                default:
                case ColorFormat.RGB:
                    return $"{color.R:X2}{color.G:X2}{color.B:X2}";
                case ColorFormat.RGBA:
                    return $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                case ColorFormat.ARGB:
                    return $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }

        public static HSB ColorToHsb(Color color)
        {
            HSB hsb = new HSB();

            int max, min;

            if (color.R > color.G)
            {
                max = color.R;
                min = color.G;
            }
            else
            {
                max = color.G;
                min = color.R;
            }

            if (color.B > max) max = color.B;
            else if (color.B < min) min = color.B;

            int diff = max - min;

            hsb.Brightness = (double)max / 255;

            if (max == 0) hsb.Saturation = 0;
            else hsb.Saturation = (double)diff / max;

            double q;
            if (diff == 0) q = 0;
            else q = (double)60 / diff;

            if (max == color.R)
            {
                if (color.G < color.B) hsb.Hue = (360 + (q * (color.G - color.B))) / 360;
                else hsb.Hue = q * (color.G - color.B) / 360;
            }
            else if (max == color.G) hsb.Hue = (120 + (q * (color.B - color.R))) / 360;
            else if (max == color.B) hsb.Hue = (240 + (q * (color.R - color.G))) / 360;
            else hsb.Hue = 0.0;

            hsb.Alpha = color.A;

            return hsb;
        }

        public static CMYK ColorToC(Color color)
        {
            if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                return new CMYK(0, 0, 0, 1, color.A);
            }

            var c = 1 - (color.R / 255d);
            double m = 1 - (color.G / 255d);
            double y = 1 - (color.B / 255d);
            double k = Math.Min(c, Math.Min(m, y));

            c = (c - k) / (1 - k);
            m = (m - k) / (1 - k);
            y = (y - k) / (1 - k);

            return new CMYK(c, m, y, k, color.A);
        }

        #endregion Convert Color to ...


        #region Convert HSB to ...

        public static Color HsbToColor(HSB hsb)
        {
            int mid;
            int max = (int)Math.Round(hsb.Brightness * 255);
            int min = (int)Math.Round((1.0 - hsb.Saturation) * (hsb.Brightness / 1.0) * 255);
            double q = (double)(max - min) / 255;

            if (hsb.Hue >= 0 && hsb.Hue <= (double)1 / 6)
            {
                mid = (int)Math.Round((((hsb.Hue - 0) * q) * 1530) + min);
                return Color.FromArgb(hsb.Alpha, max, mid, min);
            }

            if (hsb.Hue <= (double)1 / 3)
            {
                mid = (int)Math.Round((-((hsb.Hue - ((double)1 / 6)) * q) * 1530) + max);
                return Color.FromArgb(hsb.Alpha, mid, max, min);
            }

            if (hsb.Hue <= 0.5)
            {
                mid = (int)Math.Round((((hsb.Hue - ((double)1 / 3)) * q) * 1530) + min);
                return Color.FromArgb(hsb.Alpha, min, max, mid);
            }

            if (hsb.Hue <= (double)2 / 3)
            {
                mid = (int)Math.Round((-((hsb.Hue - 0.5) * q) * 1530) + max);
                return Color.FromArgb(hsb.Alpha, min, mid, max);
            }

            if (hsb.Hue <= (double)5 / 6)
            {
                mid = (int)Math.Round((((hsb.Hue - ((double)2 / 3)) * q) * 1530) + min);
                return Color.FromArgb(hsb.Alpha, mid, min, max);
            }

            if (!(hsb.Hue <= 1.0)) return Color.FromArgb(hsb.Alpha, 0, 0, 0);
            mid = (int)Math.Round((-((hsb.Hue - ((double)5 / 6)) * q) * 1530) + max);
            return Color.FromArgb(hsb.Alpha, max, min, mid);

        }

        #endregion Convert HSB to ...

        #region Convert CMYK to ...

        public static Color CToColor(CMYK cmyk)
        {
            if (cmyk.Cyan == 0 && cmyk.Magenta == 0 && cmyk.Yellow == 0 && cmyk.Key == 1)
            {
                return Color.FromArgb(cmyk.Alpha, 0, 0, 0);
            }

            double c = (cmyk.Cyan * (1 - cmyk.Key)) + cmyk.Key;
            double m = (cmyk.Magenta * (1 - cmyk.Key)) + cmyk.Key;
            double y = (cmyk.Yellow * (1 - cmyk.Key)) + cmyk.Key;

            int r = (int)Math.Round((1 - c) * 255);
            int g = (int)Math.Round((1 - m) * 255);
            int b = (int)Math.Round((1 - y) * 255);

            return Color.FromArgb(cmyk.Alpha, r, g, b);
        }

        #endregion Convert CMYK to ...

        public static double ValidColor(double number)
        {
            return number.Clamp(0, 1);
        }

        public static int ValidColor(int number)
        {
            return number.Clamp(0, 255);
        }

    }
}