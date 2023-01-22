using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShareX
{
    public static class Extensions
    {

        public static bool IsValid(this Rectangle rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }

        public static bool IsValid(this RectangleF rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }

        public static Point Add(this Point point, int offsetX, int offsetY)
        {
            return new Point(point.X + offsetX, point.Y + offsetY);
        }


        public static PointF Add(this PointF point, float offsetX, float offsetY)
        {
            return new PointF(point.X + offsetX, point.Y + offsetY);
        }

        public static PointF Add(this PointF point, PointF offset)
        {
            return new PointF(point.X + offset.X, point.Y + offset.Y);
        }

        public static PointF Scale(this Point point, float scaleFactor)
        {
            return new PointF(point.X * scaleFactor, point.Y * scaleFactor);
        }

        public static Point Round(this PointF point)
        {
            return Point.Round(point);
        }


        public static Rectangle Offset(this Rectangle rect, int offset)
        {
            return new Rectangle(rect.X - offset, rect.Y - offset, rect.Width + (offset * 2), rect.Height + (offset * 2));
        }

        public static RectangleF Offset(this RectangleF rect, float offset)
        {
            return new RectangleF(rect.X - offset, rect.Y - offset, rect.Width + (offset * 2), rect.Height + (offset * 2));
        }

        public static Rectangle Round(this RectangleF rect)
        {
            return Rectangle.Round(rect);
        }


        public static RectangleF LocationOffset(this RectangleF rect, float x, float y)
        {
            return new RectangleF(rect.X + x, rect.Y + y, rect.Width, rect.Height);
        }

        public static RectangleF SizeOffset(this RectangleF rect, float width, float height)
        {
            return new RectangleF(rect.X, rect.Y, rect.Width + width, rect.Height + height);
        }

        public static RectangleF SizeOffset(this RectangleF rect, float offset)
        {
            return rect.SizeOffset(offset, offset);
        }


        public static Bitmap CreateEmptyBitmap(this Image img, PixelFormat pixelFormat)
        {
            return img.CreateEmptyBitmap(0, 0, pixelFormat);
        }

        public static Bitmap CreateEmptyBitmap(this Image img, int widthOffset = 0, int heightOffset = 0, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Bitmap bmp = new Bitmap(img.Width + widthOffset, img.Height + heightOffset, pixelFormat);
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            return bmp;
        }

        public static T CloneSafe<T>(this T obj) where T : class, ICloneable
        {
            try
            {
                if (obj != null)
                {
                    return obj.Clone() as T;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}