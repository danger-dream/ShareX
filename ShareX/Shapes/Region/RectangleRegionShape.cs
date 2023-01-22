using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public class RectangleRegionShape : BaseRegionShape
    {
        public override ShapeType ShapeType => ShapeType.RegionRectangle;

        public int CornerRadius { get; set; }

        public override void OnConfigLoad()
        {
            CornerRadius = AnnotationOptions.RegionCornerRadius;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.RegionCornerRadius = CornerRadius;
        }

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddRoundedRectangle(rect, CornerRadius);
        }
    }
}