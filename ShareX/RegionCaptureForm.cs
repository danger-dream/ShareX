using ShareX.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareX
{
    public sealed class RegionCaptureForm : Form
    {

        public RegionCaptureOptions Options { get; set; } = new RegionCaptureOptions();
        public Rectangle ScreenBounds { get; set; }
        public Rectangle ClientArea { get; private set; }
        public Bitmap Canvas { get; private set; }
        public RectangleF CanvasRectangle { get; internal set; }

        public Point CurrentPosition { get; private set; }

        private const float ZoomFactor = 1;

        internal PointF ScaledClientMousePosition => InputManager.ClientMousePosition.Scale(1 / ZoomFactor);
        internal PointF ScaledClientMouseVelocity => InputManager.MouseVelocity.Scale(1 / ZoomFactor);

        internal ShapeManager ShapeManager { get; }
        internal bool IsClosing { get; private set; }

        internal Bitmap DimmedCanvas;

        private InputManager InputManager => ShapeManager.InputManager;
        private TextureBrush backgroundBrush, backgroundHighlightBrush;
        private GraphicsPath regionFillPath, regionDrawPath;
        private readonly Pen borderPen, borderDotPen, borderDotStaticPen, textOuterBorderPen, textInnerBorderPen, markerPen, canvasBorderPen;
        private readonly Brush textBrush, textShadowBrush, textBackgroundBrush;
        private readonly Font infoFont;
        private bool pause;
        private readonly Cursor defaultCursor;
        private readonly Color canvasBackgroundColor;
        

        public RegionCaptureForm()
        {
            var canvas = CaptureFullscreen();

            ScreenBounds = CaptureHelpers.GetScreenBounds();
            ClientArea = new Rectangle(0, 0, ScreenBounds.Width, ScreenBounds.Height);
            CanvasRectangle = ClientArea;


            borderPen = new Pen(Color.Black);
            borderDotPen = new Pen(Color.White) { DashPattern = new float[] { 5, 5 } };
            borderDotStaticPen = new Pen(Color.White) { DashPattern = new float[] { 5, 5 } };
            infoFont = new Font("Verdana", 9);
            markerPen = new Pen(Color.FromArgb(200, Color.Red));

            canvasBackgroundColor = Color.FromArgb(200, 200, 200);
            var canvasBorderColor = Color.FromArgb(176, 176, 176);
            var textColor = Color.White;
            var textShadowColor = Color.Black;
            var textBackgroundColor = Color.FromArgb(200, Color.FromArgb(42, 131, 199));
            var textOuterBorderColor = Color.FromArgb(200, Color.White);
            var textInnerBorderColor = Color.FromArgb(200, Color.FromArgb(0, 81, 145));

            canvasBorderPen = new Pen(canvasBorderColor);
            textBrush = new SolidBrush(textColor);
            textShadowBrush = new SolidBrush(textShadowColor);
            textBackgroundBrush = new SolidBrush(textBackgroundColor);
            textOuterBorderPen = new Pen(textOuterBorderColor);
            textInnerBorderPen = new Pen(textInnerBorderColor);

            ShapeManager = new ShapeManager(this)
            {
                WindowCaptureMode = true,
                IncludeControls = true
            };
            var cursorData = new CursorData();
            ShapeManager.AddCursor(cursorData.ToBitmap(), PointToClient(cursorData.DrawPosition));
            InitBackground(canvas);

            var handle = Handle;
            Task.Run(() =>
            {
                var wla = new WindowsRectangleList
                {
                    IgnoreHandle = handle,
                    IncludeChildWindows = ShapeManager.IncludeControls
                };
                ShapeManager.Windows = wla.GetWindowInfoListAsync(5000);
            });
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.None;
            defaultCursor = Helpers.CreateCursor(Resources.Crosshair);
            SetDefaultCursor();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            StartPosition = FormStartPosition.Manual;

            FormBorderStyle = FormBorderStyle.None;
            Bounds = ScreenBounds;
            ShowInTaskbar = false;
#if !DEBUG
                TopMost = true;
#endif
            Shown += RegionCaptureForm_Shown;
            KeyDown += RegionCaptureForm_KeyDown;
            Resize += RegionCaptureForm_Resize;
            LocationChanged += RegionCaptureForm_LocationChanged;
            GotFocus += RegionCaptureForm_GotFocus;
            LostFocus += RegionCaptureForm_LostFocus;
            ResumeLayout(false);
        }

        public Bitmap CaptureFullscreen()
        {
            var rect = CaptureHelpers.GetScreenBounds();
            var bounds = CaptureHelpers.GetScreenBounds();
            rect = Rectangle.Intersect(bounds, rect);

            var handle = NativeMethods.GetDesktopWindow();
            if (rect.Width == 0 || rect.Height == 0)
            {
                return null;
            }
            var hdcSrc = NativeMethods.GetWindowDC(handle);
            var hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            var hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            var hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(handle, hdcSrc);
            var bmp = Image.FromHbitmap(hBitmap);
            NativeMethods.DeleteObject(hBitmap);
            return bmp;
        }

        internal void InitBackground(Bitmap canvas)
        {
            Canvas?.Dispose();
            backgroundBrush?.Dispose();
            backgroundHighlightBrush?.Dispose();

            Canvas = canvas;

            DimmedCanvas?.Dispose();
            DimmedCanvas = (Bitmap)Canvas.Clone();

            using (var g = Graphics.FromImage(DimmedCanvas))
            using (Brush brush = new SolidBrush(Color.FromArgb(30, Color.Black)))
            {
                g.FillRectangle(brush, 0, 0, DimmedCanvas.Width, DimmedCanvas.Height);
                backgroundBrush = new TextureBrush(DimmedCanvas) { WrapMode = WrapMode.Clamp };
            }

            backgroundHighlightBrush = new TextureBrush(Canvas) { WrapMode = WrapMode.Clamp };
        }

        private void OnMoved()
        {
            if (ShapeManager != null)
            {
                UpdateCoordinates();
            }
        }

        public void SetDefaultCursor()
        {
            if (Cursor != defaultCursor)
            {
                Cursor = defaultCursor;
            }
        }


        private void RegionCaptureForm_Shown(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                if (!Visible)
                {
                    Show();
                }

                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }

                BringToFront();
                Activate();
            }
            OnMoved();
        }

        private void RegionCaptureForm_Resize(object sender, EventArgs e)
        {
            OnMoved();
        }

        private void RegionCaptureForm_LocationChanged(object sender, EventArgs e)
        {
            OnMoved();
        }

        private void RegionCaptureForm_GotFocus(object sender, EventArgs e)
        {
            pause = false;
            Invalidate();
        }

        private void RegionCaptureForm_LostFocus(object sender, EventArgs e)
        {
            pause = true;
        }
        

        internal void RegionCaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                CloseWindow();
            }
            else if (e.KeyData == Keys.H)
            {
                var hex = "#" + ColorHelpers.ColorToHex(ShapeManager.GetCurrentColor()).ToUpper();
                ClipboardHelpers.CopyText("color:" + hex);
                CloseWindow(true);
            }
        }


        public void CopyImage()
        {
            using (var bmp = ApplyRegionPathToImage(Canvas, regionFillPath, out var rect))
            {
                var res = ShapeManager.RenderOutputImage(bmp, rect.Location);
                if (res == null)
                {
                    ClipboardHelpers.CopyText("ocr:empty");
                    return;
                }
                using (var stream = new MemoryStream())
                {
                    res.Save(stream, ImageFormat.Png);
                    ClipboardHelpers.CopyText("ocr:" + Convert.ToBase64String(stream.ToArray()));
                }
            }
        }

        public static Bitmap ApplyRegionPathToImage(Bitmap bmp, GraphicsPath gp, out Rectangle resultArea)
        {
            if (bmp != null && gp != null)
            {
                var regionArea = Rectangle.Round(gp.GetBounds());
                var screenRectangle = CaptureHelpers.GetScreenBounds();
                resultArea = Rectangle.Intersect(regionArea, new Rectangle(0, 0, screenRectangle.Width, screenRectangle.Height));

                if (resultArea.IsValid())
                {
                    using (var bmpResult = bmp.CreateEmptyBitmap())
                    using (var g = Graphics.FromImage(bmpResult))
                    using (var brush = new TextureBrush(bmp))
                    {
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        g.FillPath(brush, gp);

                        return ImageHelpers.CropBitmap(bmpResult, resultArea);
                    }
                }
            }

            resultArea = Rectangle.Empty;
            return null;
        }


        internal void CloseWindow(bool isSuccess = false)
        {
            if (!isSuccess)
            {
                ClipboardHelpers.CopyText("ocr:empty");
            }
            Close();
        }

        private void UpdateCoordinates()
        {
            ClientArea = ClientRectangle;
            InputManager.Update(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            UpdateCoordinates();
            ShapeManager.Update();

            var g = e.Graphics;
            ShapeManager.CurrentDpi.X = g.DpiX;
            ShapeManager.CurrentDpi.Y = g.DpiY;

            using (new GraphicsQualityManager(g, false))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(backgroundBrush.Image, CanvasRectangle);
                g.CompositingMode = CompositingMode.SourceOver;
            }
            DrawShapes(g);
            if (!pause)
            {
                Invalidate();
            }
        }

        private void DrawShapes(Graphics g)
        {
            var areas = ShapeManager.ValidRegions.ToList();
            if (areas.Count > 0)
            {
                // Create graphics path from all regions
                UpdateRegionPath();
                // If background is dimmed then draw non dimmed background to region selections
                if (Options.UseDimming)
                {
                    using (var region = new Region(regionDrawPath))
                    {
                        g.Clip = region;
                        g.FillRectangle(backgroundHighlightBrush, ClientArea);
                        g.ResetClip();
                    }
                }
                g.DrawPath(borderPen, regionDrawPath);
                g.DrawPath(borderDotStaticPen, regionDrawPath);
            }

            // Draw animated rectangle on hover area
            if (ShapeManager.IsCurrentHoverShapeValid)
            {
                
                using (var hoverDrawPath = new GraphicsPath { FillMode = FillMode.Winding })
                {
                    ShapeManager.CurrentHoverShape.AddShapePath(hoverDrawPath, -1);
                    g.DrawPath(borderPen, hoverDrawPath);
                    g.DrawPath(borderDotPen, hoverDrawPath);
                }
            }
            // Draw animated rectangle on selection area
            if (ShapeManager.IsCurrentShapeTypeRegion && ShapeManager.IsCurrentShapeValid)
            {
                DrawRegionArea(g, ShapeManager.CurrentRectangle, true);
            }
            // Draw all regions rectangle info
            if (Options.ShowInfo)
            {
                // Add hover area to list so rectangle info can be shown
                if (ShapeManager.IsCurrentShapeTypeRegion && ShapeManager.IsCurrentHoverShapeValid && areas.All(area => area.Rectangle != ShapeManager.CurrentHoverShape.Rectangle))
                {
                    areas.Add(ShapeManager.CurrentHoverShape);
                }
                foreach (var regionInfo in areas)
                {
                    if (!regionInfo.Rectangle.IsValid()) continue;
                    var areaText = GetAreaText(regionInfo.Rectangle);
                    DrawAreaText(g, areaText, regionInfo.Rectangle);
                }
            }
            // Draw resize nodes
            ShapeManager.DrawObjects(g);
            // Draw magnifier
            DrawCursorGraphics(g);

            const int offset = 5;
            var mousePos = ScaledClientMousePosition;
            PointF left = new PointF(mousePos.X - offset, mousePos.Y), left2 = new PointF(0, mousePos.Y);
            PointF right = new PointF(mousePos.X + offset, mousePos.Y), right2 = new PointF((ClientArea.Width - 1) / ZoomFactor, mousePos.Y);
            PointF top = new PointF(mousePos.X, mousePos.Y - offset), top2 = new PointF(mousePos.X, 0);
            PointF bottom = new PointF(mousePos.X, mousePos.Y + offset), bottom2 = new PointF(mousePos.X, (ClientArea.Height - 1) / ZoomFactor);

            if (left.X - left2.X > 10)
            {
                g.DrawLine(borderPen, left, left2);
                g.DrawLine(borderDotPen, left, left2);
            }

            if (right2.X - right.X > 10)
            {
                g.DrawLine(borderPen, right, right2);
                g.DrawLine(borderDotPen, right, right2);
            }

            if (top.Y - top2.Y > 10)
            {
                g.DrawLine(borderPen, top, top2);
                g.DrawLine(borderDotPen, top, top2);
            }

            if (!(bottom2.Y - bottom.Y > 10)) return;
            g.DrawLine(borderPen, bottom, bottom2);
            g.DrawLine(borderDotPen, bottom, bottom2);
        }

        internal void DrawRegionArea(Graphics g, RectangleF rect, bool isAnimated)
        {
            g.DrawRectangleProper(borderPen, rect);

            g.DrawRectangleProper(isAnimated ? borderDotPen : borderDotStaticPen, rect);
        }

        private void DrawInfoText(Graphics g, string text, RectangleF rect, Font font, Point padding)
        {
            g.FillRectangle(textBackgroundBrush, rect.Offset(-2));
            g.DrawRectangleProper(textInnerBorderPen, rect.Offset(-1));
            g.DrawRectangleProper(textOuterBorderPen, rect);

            g.DrawTextWithShadow(text, rect.LocationOffset(padding.X, padding.Y).Location, font, textBrush, textShadowBrush);

        }

        internal void DrawAreaText(Graphics g, string text, RectangleF area)
        {
            const int offset = 6;
            const int backgroundPadding = 3;
            var textSize = g.MeasureString(text, infoFont).ToSize();

            var textPos = area.Y - offset - textSize.Height - (backgroundPadding * 2) < ClientArea.Y ? new PointF(area.X + offset + backgroundPadding, area.Y + offset + backgroundPadding) : new PointF(area.X + backgroundPadding, area.Y - offset - backgroundPadding - textSize.Height);

            if (textPos.X + textSize.Width + backgroundPadding >= ClientArea.Width)
            {
                textPos.X = ClientArea.Width - textSize.Width - backgroundPadding;
            }

            var backgroundRect = new RectangleF(textPos.X - backgroundPadding, textPos.Y - backgroundPadding, textSize.Width + (backgroundPadding * 2), textSize.Height + (backgroundPadding * 2));
            DrawInfoText(g, text, backgroundRect, infoFont, new Point(backgroundPadding, backgroundPadding));
        }

        internal string GetAreaText(RectangleF rect)
        {
            return $"X: {rect.X} Y: {rect.Y} W: {rect.Width} H: {rect.Height}";
        }

        private void DrawCursorGraphics(Graphics g)
        {
            const int cursorOffsetX = 10;
            const int cursorOffsetY = 10;
            const int itemGap = 10;
            var totalSize = Size.Empty;


            var magnifierPosition = totalSize.Height;
            var magnifier = Magnifier(Canvas, ScaledClientMousePosition, Options.MagnifierPixelCount, Options.MagnifierPixelCount, Options.MagnifierPixelSize);
            totalSize.Width = Math.Max(totalSize.Width, magnifier.Width);

            totalSize.Height += magnifier.Height;

            const int infoTextPadding = 3;
            var infoTextRect = Rectangle.Empty;

            totalSize.Height += itemGap;
            var infoTextPosition = totalSize.Height;

            CurrentPosition = InputManager.MousePosition;
            //  当前鼠标颜色
            var color = ShapeManager.GetCurrentColor();
            var infoText =
                $"RGB: {color.R}, {color.G}, {color.B}\r\nHex: #{ColorHelpers.ColorToHex(color).ToUpper()}\r\nX: {CurrentPosition.X} Y: {CurrentPosition.Y}";
            var textSize = g.MeasureString(infoText, infoFont).ToSize();
            infoTextRect.Size = new Size(textSize.Width + (infoTextPadding * 2), textSize.Height + (infoTextPadding * 2));
            totalSize.Width = Math.Max(totalSize.Width, infoTextRect.Width);

            totalSize.Height += infoTextRect.Height;

            var mousePos = InputManager.ClientMousePosition;
            var activeScreenClientRect = RectangleToClient(CaptureHelpers.GetActiveScreenBounds());
            var x = mousePos.X + cursorOffsetX;

            if (x + totalSize.Width > activeScreenClientRect.Right)
            {
                x = mousePos.X - cursorOffsetX - totalSize.Width;
            }
            var y = mousePos.Y + cursorOffsetY;
            if (y + totalSize.Height > activeScreenClientRect.Bottom)
            {
                y = mousePos.Y - cursorOffsetY - totalSize.Height;
            }
            var initialTransform = g.Transform;
            if (Options.UseSquareMagnifier)
            {
                g.DrawImage(magnifier, x, y + magnifierPosition, magnifier.Width, magnifier.Height);
                g.DrawRectangleProper(Pens.White, x - 1, y + magnifierPosition - 1, magnifier.Width + 2, magnifier.Height + 2);
                g.DrawRectangleProper(Pens.Black, x, y + magnifierPosition, magnifier.Width, magnifier.Height);
            }
            else
            {
                using (new GraphicsQualityManager(g, true))
                using (var brush = new TextureBrush(magnifier))
                {
                    brush.TranslateTransform(x, y + magnifierPosition);

                    g.FillEllipse(brush, x, y + magnifierPosition, magnifier.Width, magnifier.Height);
                    g.DrawEllipse(Pens.White, x - 1, y + magnifierPosition - 1, magnifier.Width + 2 - 1, magnifier.Height + 2 - 1);
                    g.DrawEllipse(Pens.Black, x, y + magnifierPosition, magnifier.Width - 1, magnifier.Height - 1);
                }
            }
            g.Transform = initialTransform;
            infoTextRect.Location = new Point(x + (totalSize.Width / 2) - (infoTextRect.Width / 2), y + infoTextPosition);
            var padding = new Point(infoTextPadding, infoTextPadding);

            DrawInfoText(g, infoText, infoTextRect, infoFont, padding);
            g.Transform = initialTransform;
        }

        private Bitmap Magnifier(Image img, PointF position, int horizontalPixelCount, int verticalPixelCount, int pixelSize)
        {
            horizontalPixelCount = (horizontalPixelCount | 1).Clamp(1, 101);
            verticalPixelCount = (verticalPixelCount | 1).Clamp(1, 101);
            pixelSize = pixelSize.Clamp(1, 1000);

            if (horizontalPixelCount * pixelSize > ClientArea.Width || verticalPixelCount * pixelSize > ClientArea.Height)
            {
                horizontalPixelCount = verticalPixelCount = 15;
                pixelSize = 10;
            }

            RectangleF srcRect = new RectangleF(position.X - (horizontalPixelCount / 2) - CanvasRectangle.X,
                position.Y - (verticalPixelCount / 2) - CanvasRectangle.Y, horizontalPixelCount, verticalPixelCount).Round();

            var width = horizontalPixelCount * pixelSize;
            var height = verticalPixelCount * pixelSize;
            var bmp = new Bitmap(width - 1, height - 1);

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                if (!new RectangleF(0, 0, img.Width, img.Height).Contains(srcRect))
                {
                    g.Clear(canvasBackgroundColor);
                }

                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(img, new Rectangle(0, 0, width, height), srcRect, GraphicsUnit.Pixel);
                g.PixelOffsetMode = PixelOffsetMode.None;

                using (var crosshatchBrush = new SolidBrush(Color.FromArgb(125, Color.LightBlue)))
                {
                    g.FillRectangle(crosshatchBrush, new Rectangle(0, (height - pixelSize) / 2, (width - pixelSize) / 2, pixelSize)); // Left
                    g.FillRectangle(crosshatchBrush, new Rectangle((width + pixelSize) / 2, (height - pixelSize) / 2, (width - pixelSize) / 2, pixelSize)); // Right
                    g.FillRectangle(crosshatchBrush, new Rectangle((width - pixelSize) / 2, 0, pixelSize, (height - pixelSize) / 2)); // Top
                    g.FillRectangle(crosshatchBrush, new Rectangle((width - pixelSize) / 2, (height + pixelSize) / 2, pixelSize, (height - pixelSize) / 2)); // Bottom
                }

                using (var pen = new Pen(Color.FromArgb(75, Color.Black)))
                {
                    for (var x = 1; x < horizontalPixelCount; x++)
                    {
                        g.DrawLine(pen, new Point((x * pixelSize) - 1, 0), new Point((x * pixelSize) - 1, height - 1));
                    }

                    var y = 1;
                    for (; y < verticalPixelCount; y++)
                    {
                        g.DrawLine(pen, new Point(0, (y * pixelSize) - 1), new Point(width - 1, (y * pixelSize) - 1));
                    }
                }

                g.DrawRectangle(Pens.Black, ((width - pixelSize) / 2) - 1, ((height - pixelSize) / 2) - 1, pixelSize, pixelSize);

                if (pixelSize >= 6)
                {
                    g.DrawRectangle(Pens.White, (width - pixelSize) / 2, (height - pixelSize) / 2, pixelSize - 2, pixelSize - 2);
                }
            }

            return bmp;
        }

        internal void UpdateRegionPath()
        {
            if (regionFillPath != null)
            {
                regionFillPath.Dispose();
                regionFillPath = null;
            }

            if (regionDrawPath != null)
            {
                regionDrawPath.Dispose();
                regionDrawPath = null;
            }

            var areas = ShapeManager.ValidRegions;

            if (areas == null || areas.Length <= 0) return;
            regionFillPath = new GraphicsPath { FillMode = FillMode.Winding };
            regionDrawPath = new GraphicsPath { FillMode = FillMode.Winding };

            foreach (var regionShape in ShapeManager.ValidRegions)
            {
                regionShape.AddShapePath(regionFillPath);
                regionShape.AddShapePath(regionDrawPath, -1);
            }
        }


        protected override void Dispose(bool disposing)
        {
            IsClosing = true;
            ShapeManager?.Dispose();
            backgroundBrush?.Dispose();
            backgroundHighlightBrush?.Dispose();
            borderPen?.Dispose();
            borderDotPen?.Dispose();
            borderDotStaticPen?.Dispose();
            infoFont?.Dispose();
            textBrush?.Dispose();
            textShadowBrush?.Dispose();
            textBackgroundBrush?.Dispose();
            textOuterBorderPen?.Dispose();
            textInnerBorderPen?.Dispose();
            markerPen?.Dispose();
            canvasBorderPen?.Dispose();
            defaultCursor?.Dispose();
            regionDrawPath?.Dispose();
            DimmedCanvas?.Dispose();
            Canvas?.Dispose();
            base.Dispose(disposing);
        }

    }
}