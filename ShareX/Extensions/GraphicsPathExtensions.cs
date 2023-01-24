using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public static class GraphicsPathExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath gp, RectangleF rect, float radius)
        {
            if (radius <= 0f)
            {
                gp.AddRectangle(rect);
            }
            else
            {
                // If the corner radius is greater than or equal to
                // half the width, or height (whichever is shorter)
                // then return a capsule instead of a lozenge
                if (radius >= (Math.Min(rect.Width, rect.Height) / 2.0f))
                {
                    gp.AddCapsule(rect);
                }
                else
                {
                    // Create the arc for the rectangle sides and declare
                    // a graphics path object for the drawing
                    float diameter = radius * 2.0f;
                    SizeF size = new SizeF(diameter, diameter);
                    RectangleF arc = new RectangleF(rect.Location, size);

                    // Top left arc
                    gp.AddArc(arc, 180, 90);

                    // Top right arc
                    arc.X = rect.Right - diameter;
                    gp.AddArc(arc, 270, 90);

                    // Bottom right arc
                    arc.Y = rect.Bottom - diameter;
                    gp.AddArc(arc, 0, 90);

                    // Bottom left arc
                    arc.X = rect.Left;
                    gp.AddArc(arc, 90, 90);

                    gp.CloseFigure();
                }
            }
        }

        public static void AddCapsule(this GraphicsPath gp, RectangleF rect)
        {
            float diameter;
            RectangleF arc;

            try
            {
                if (rect.Width > rect.Height)
                {
                    // Horizontal capsule
                    diameter = rect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(rect.Location, sizeF);
                    gp.AddArc(arc, 90, 180);
                    arc.X = rect.Right - diameter;
                    gp.AddArc(arc, 270, 180);
                }
                else if (rect.Width < rect.Height)
                {
                    // Vertical capsule
                    diameter = rect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(rect.Location, sizeF);
                    gp.AddArc(arc, 180, 180);
                    arc.Y = rect.Bottom - diameter;
                    gp.AddArc(arc, 0, 180);
                }
                else
                {
                    // Circle
                    gp.AddEllipse(rect);
                }
            }
            catch
            {
                gp.AddEllipse(rect);
            }

            gp.CloseFigure();
        }
    }
}