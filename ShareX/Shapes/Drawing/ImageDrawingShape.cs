using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public class ImageDrawingShape : BaseDrawingShape
    {
        public override ShapeType ShapeType => ShapeType.DrawingImage;

        public Image Image { get; protected set; }
        public ImageInterpolationMode ImageInterpolationMode { get; protected set; }

        public override BaseShape Duplicate()
        {
            var imageTemp = Image;
            Image = null;
            var shape = (ImageDrawingShape)base.Duplicate();
            shape.Image = imageTemp.CloneSafe();
            Image = imageTemp;
            return shape;
        }

        public override void OnConfigLoad()
        {
            ImageInterpolationMode = AnnotationOptions.ImageInterpolationMode;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.ImageInterpolationMode = ImageInterpolationMode;
        }

        public override void OnDraw(Graphics g)
        {
            DrawImage(g);
        }

        protected void DrawImage(Graphics g)
        {
            if (Image == null) return;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = ImageHelpers.GetInterpolationMode(ImageInterpolationMode);

            g.DrawImage(Image, Rectangle);

            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.InterpolationMode = InterpolationMode.Bilinear;
        }

        public override void OnMoved()
        {
        }

        public override void Dispose()
        {
            Image?.Dispose();
        }
    }
}