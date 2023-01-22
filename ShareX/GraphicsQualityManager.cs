﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public class GraphicsQualityManager : IDisposable
    {
        private readonly CompositingQuality previousCompositingQuality;
        private readonly InterpolationMode previousInterpolationMode;
        private readonly SmoothingMode previousSmoothingMode;
        private readonly PixelOffsetMode previousPixelOffsetMode;
        private readonly Graphics g;

        public GraphicsQualityManager(Graphics g, bool highQuality)
        {
            this.g = g;

            previousCompositingQuality = g.CompositingQuality;
            previousInterpolationMode = g.InterpolationMode;
            previousSmoothingMode = g.SmoothingMode;
            previousPixelOffsetMode = g.PixelOffsetMode;

            if (highQuality)
            {
                SetHighQuality();
            }
            else
            {
                SetLowQuality();
            }
        }

        public void SetHighQuality()
        {
            if (g != null)
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        public void SetLowQuality()
        {
            if (g != null)
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            }
        }

        public void Dispose()
        {
            if (g != null)
            {
                g.CompositingQuality = previousCompositingQuality;
                g.InterpolationMode = previousInterpolationMode;
                g.SmoothingMode = previousSmoothingMode;
                g.PixelOffsetMode = previousPixelOffsetMode;
            }
        }
    }
}