using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public static class ImageHelpers
    {
        public static Bitmap CropBitmap(Bitmap bmp, Rectangle rect)
        {
            if (bmp != null && rect.X >= 0 && rect.Y >= 0 && rect.Width > 0 && rect.Height > 0 && new Rectangle(0, 0, bmp.Width, bmp.Height).Contains(rect))
            {
                return bmp.Clone(rect, bmp.PixelFormat);
            }

            return null;
        }

        public static InterpolationMode GetInterpolationMode(ImageInterpolationMode interpolationMode)
        {
            switch (interpolationMode)
            {
                default:
                case ImageInterpolationMode.HighQualityBicubic:
                    return InterpolationMode.HighQualityBicubic;
                case ImageInterpolationMode.Bicubic:
                    return InterpolationMode.Bicubic;
                case ImageInterpolationMode.HighQualityBilinear:
                    return InterpolationMode.HighQualityBilinear;
                case ImageInterpolationMode.Bilinear:
                    return InterpolationMode.Bilinear;
                case ImageInterpolationMode.NearestNeighbor:
                    return InterpolationMode.NearestNeighbor;
            }
        }

    }
}