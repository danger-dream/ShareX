using System.Collections.Generic;
using System.Drawing;

namespace ShareX
{
    public class RegionCaptureOptions
    {
        public const int DefaultMinimumSize = 5;
        public const int MagnifierPixelCountMinimum = 3;
        public const int MagnifierPixelCountMaximum = 35;
        public const int SnapDistance = 30;
        public const int MoveSpeedMinimum = 1;
        public const int MoveSpeedMaximum = 10;

        public bool QuickCrop = true;
        public int MinimumSize = DefaultMinimumSize;
        public bool UseDimming = true;
        public List<SnapSize> SnapSizes = new List<SnapSize>()
        {
            new SnapSize(426, 240), // 240p
            new SnapSize(640, 360), // 360p
            new SnapSize(854, 480), // 480p
            new SnapSize(1280, 720), // 720p
            new SnapSize(1920, 1080) // 1080p
        };
        public bool ShowInfo = true;
        public bool ShowMagnifier = true;
        public int MagnifierPixelCount = 20; // Must be odd number like 11, 13, 15 etc.
        public int MagnifierPixelSize = 10;
        public bool IsFixedSize = false;
        public Size FixedSize = new Size(250, 250);
        // Annotation
        public AnnotationOptions AnnotationOptions = new AnnotationOptions();
    }
}