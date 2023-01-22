using System.Drawing;

namespace ShareX
{
    public class AnnotationOptions
    {
        public static readonly Color PrimaryColor = Color.FromArgb(242, 60, 60);
        public static readonly Color TransparentColor = Color.FromArgb(0, 0, 0, 0);
        // Region
        public int RegionCornerRadius { get; set; } = 0;

        // Drawing
        public Color BorderColor { get; set; } = PrimaryColor;
        public int BorderSize { get; set; } = 4;
        public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;
        public Color FillColor { get; set; } = TransparentColor;
        public bool Shadow { get; set; } = true;
        public Color ShadowColor { get; set; } = Color.FromArgb(125, 0, 0, 0);
        public Point ShadowOffset { get; set; } = new Point(0, 1);

        // Image drawing
        public ImageInterpolationMode ImageInterpolationMode = ImageInterpolationMode.NearestNeighbor;
    }
}