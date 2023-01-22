namespace ShareX
{
    public enum ColorFormat
    {
        RGB, RGBA, ARGB
    }

    public enum ImageInterpolationMode
    {
        HighQualityBicubic,
        Bicubic,
        HighQualityBilinear,
        Bilinear,
        NearestNeighbor
    }
    
    internal enum NodeShape
    {
        Square, Circle, Diamond, CustomNode
    }

    public enum RegionCaptureAction // Localized
    {
        CancelCapture,
        RemoveShapeCancelCapture,
        RemoveShape,
        SwapToolType,
        CaptureFullscreen,
        CaptureActiveMonitor,
        CaptureLastRegion
    }

    public enum ShapeCategory
    {
        Region,
        Drawing,
        Effect
    }

    public enum ShapeType // Localized
    {
        RegionRectangle,
        DrawingCursor,
        DrawingImage
    }

    public enum BorderStyle // Localized
    {
        Solid
    }
}