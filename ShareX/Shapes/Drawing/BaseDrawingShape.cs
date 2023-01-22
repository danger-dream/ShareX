﻿using System.Drawing;

namespace ShareX
{
    public abstract class BaseDrawingShape : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Drawing;

        public Color BorderColor { get; set; }
        public int BorderSize { get; set; }
        public BorderStyle BorderStyle { get; set; }
        public Color FillColor { get; set; }

        public bool Shadow { get; set; }
        public Color ShadowColor { get; set; }
        public Point ShadowOffset { get; set; }


        public override void OnConfigLoad()
        {
            BorderColor = AnnotationOptions.BorderColor;
            BorderSize = AnnotationOptions.BorderSize;
            BorderStyle = AnnotationOptions.BorderStyle;
            FillColor = AnnotationOptions.FillColor;
            Shadow = AnnotationOptions.Shadow;
            ShadowColor = AnnotationOptions.ShadowColor;
            ShadowOffset = AnnotationOptions.ShadowOffset;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.BorderColor = BorderColor;
            AnnotationOptions.BorderSize = BorderSize;
            AnnotationOptions.BorderStyle = BorderStyle;
            AnnotationOptions.FillColor = FillColor;
            AnnotationOptions.Shadow = Shadow;
            AnnotationOptions.ShadowColor = ShadowColor;
            AnnotationOptions.ShadowOffset = ShadowOffset;
        }

        public abstract void OnDraw(Graphics g);
    }
}