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