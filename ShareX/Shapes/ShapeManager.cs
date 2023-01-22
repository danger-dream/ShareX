using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShareX
{
    internal class ShapeManager : IDisposable
    {
        public List<BaseShape> Shapes { get; } = new List<BaseShape>();

        private BaseShape currentShape;

        public BaseShape CurrentShape
        {
            get => currentShape;
            private set
            {
                currentShape = value;
                currentShape?.OnConfigSave();

                OnCurrentShapeChanged(currentShape);
            }
        }

        private ShapeType currentTool;

        public ShapeType CurrentTool
        {
            get => currentTool;
            set
            {
                if (currentTool == value) return;

                currentTool = value;
                if (CurrentShape != null)
                {
                    if (CurrentShape.ShapeType != currentTool)
                    {
                        DeselectCurrentShape();
                    }
                }

                OnCurrentShapeTypeChanged(currentTool);
            }
        }

        public RectangleF CurrentRectangle => CurrentShape?.Rectangle ?? RectangleF.Empty;

        public PointF CurrentDpi = new PointF(96f, 96f);

        public bool IsCurrentShapeValid => CurrentShape != null && CurrentShape.IsValidShape;

        public BaseRegionShape[] Regions => Shapes.OfType<BaseRegionShape>().ToArray();

        public BaseShape[] ValidRegions => Regions.Where(x => x.IsValidShape).ToArray();

        public BaseDrawingShape[] DrawingShapes => Shapes.OfType<BaseDrawingShape>().ToArray();

        private BaseShape currentHoverShape;

        public BaseRegionShape CurrentHoverShape
        {
            get => (BaseRegionShape)currentHoverShape;
            private set
            {
                if (currentHoverShape != null)
                {
                    if (PreviousHoverRectangle == Rectangle.Empty || PreviousHoverRectangle != currentHoverShape.Rectangle)
                    {
                        PreviousHoverRectangle = currentHoverShape.Rectangle;
                    }
                }
                else
                {
                    PreviousHoverRectangle = Rectangle.Empty;
                }

                currentHoverShape = value;
            }
        }

        public RectangleF PreviousHoverRectangle { get; private set; }

        public bool IsCurrentHoverShapeValid => CurrentHoverShape != null && CurrentHoverShape.IsValidShape;

        public bool IsCurrentShapeTypeRegion => IsShapeTypeRegion(CurrentTool);

        public bool IsCreating { get; set; }

        private bool isMoving;

        public bool IsMoving
        {
            get => isMoving;
            set
            {
                if (isMoving == value) return;
                isMoving = value;
                if (!isMoving)
                {
                    Form.SetDefaultCursor();
                }
            }
        }

        private bool isPanning;

        public bool IsPanning
        {
            get => isPanning;
            set
            {
                if (isPanning == value) return;
                isPanning = value;
                if (!isPanning)
                {
                    Form.SetDefaultCursor();
                }
            }
        }

        public bool IsResizing { get; set; }
        // Is holding Ctrl?
        public bool IsCtrlModifier { get; private set; }
        public bool IsCornerMoving { get; private set; }
        // Is holding Shift?
        public bool IsProportionalResizing { get; private set; }
        // Is holding Alt?
        public bool IsSnapResizing { get; private set; }
        public bool IsRenderingOutput { get; private set; }
        public PointF RenderOffset { get; private set; }
        public bool IsImageModified { get; internal set; }

        public InputManager InputManager { get; } = new InputManager();
        public List<SimpleWindowInfo> Windows { get; set; }
        public bool WindowCaptureMode { get; set; }
        public bool IncludeControls { get; set; }

        public RegionCaptureOptions Options { get; }

        internal List<ImageEditorControl> DrawableObjects { get; }

        public bool NodesVisible { get; set; }

        public bool IsCursorOnObject => DrawableObjects.Any(x => x.HandleMouseInput && x.IsCursorHover);

        public event Action<BaseShape> CurrentShapeChanged;
        public event Action<ShapeType> CurrentShapeTypeChanged;
        public event Action<BaseShape> ShapeCreated;
        public event Action ImageModified;

        internal RegionCaptureForm Form { get; }

        private bool isLeftPressed, isRightPressed, isUpPressed, isDownPressed;

        public ShapeManager(RegionCaptureForm form)
        {
            Form = form;
            Options = form.Options;

            DrawableObjects = new List<ImageEditorControl>();

            form.LostFocus += form_LostFocus;
            form.MouseDown += form_MouseDown;
            form.MouseUp += form_MouseUp;
            form.MouseDoubleClick += form_MouseDoubleClick;
            form.MouseWheel += form_MouseWheel;
            form.KeyDown += form_KeyDown;
            form.KeyUp += form_KeyUp;

            CurrentShape = null;

            CurrentTool = ShapeType.RegionRectangle;

            foreach (var control in DrawableObjects)
            {
                control.MouseLeave += () => Form.SetDefaultCursor();
            }
        }

        private void OnCurrentShapeChanged(BaseShape shape)
        {
            CurrentShapeChanged?.Invoke(shape);
        }

        private void OnCurrentShapeTypeChanged(ShapeType shapeType)
        {
            CurrentShapeTypeChanged?.Invoke(shapeType);
        }

        private void OnShapeCreated(BaseShape shape)
        {
            ShapeCreated?.Invoke(shape);
        }

        internal void OnImageModified()
        {
            OrderStepShapes();
            IsImageModified = true;

            ImageModified?.Invoke();
        }

        private void form_LostFocus(object sender, EventArgs e)
        {
            ResetModifiers();
        }

        private void form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !IsCreating)
            {
                StartRegionSelection();
            }
        }

        private void form_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (IsMoving || IsCreating)
                {
                    EndRegionSelection();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (IsCreating)
                {
                    DeleteCurrentShape();
                    EndRegionSelection();
                }
                else if (IsShapeIntersect())
                {
                    DeleteIntersectShape();
                }
                else
                {
                    Form.CloseWindow();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                RunAction(Options.RegionCaptureActionMiddleClick);

            }
            else if (e.Button == MouseButtons.XButton1)
            {
                RunAction(Options.RegionCaptureActionX1Click);
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                RunAction(Options.RegionCaptureActionX2Click);
            }
        }

        private void form_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (!IsCurrentShapeTypeRegion || ValidRegions.Length <= 0) return;
            Form.UpdateRegionPath();
            Form.CloseWindow();
        }

        private void form_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None) return;
            if (e.Delta > 0)
            {
                if (Options.ShowMagnifier)
                {
                    Options.MagnifierPixelCount = Math.Min(Options.MagnifierPixelCount + 2, RegionCaptureOptions.MagnifierPixelCountMaximum);
                }
                else
                {
                    Options.ShowMagnifier = true;
                }
            }
            else if (e.Delta < 0)
            {
                var magnifierPixelCount = Options.MagnifierPixelCount - 2;
                if (magnifierPixelCount < RegionCaptureOptions.MagnifierPixelCountMinimum)
                {
                    magnifierPixelCount = RegionCaptureOptions.MagnifierPixelCountMinimum;
                    Options.ShowMagnifier = false;
                }
                Options.MagnifierPixelCount = magnifierPixelCount;
            }
        }

        private void form_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    if (!IsCtrlModifier && !IsCornerMoving)
                    {
                        if (IsCreating || IsResizing)
                        {
                            IsCornerMoving = true;
                        }
                        else
                        {
                            IsCtrlModifier = true;
                        }
                    }
                    break;
                case Keys.ShiftKey:
                    IsProportionalResizing = true;
                    break;
                case Keys.Menu:
                    IsSnapResizing = true;
                    break;
                case Keys.Left:
                    isLeftPressed = true;
                    break;
                case Keys.Right:
                    isRightPressed = true;
                    break;
                case Keys.Up:
                    isUpPressed = true;
                    break;
                case Keys.Down:
                    isDownPressed = true;
                    break;
            }

            switch (e.KeyData)
            {
                case Keys.Insert:
                    if (IsCreating)
                    {
                        EndRegionSelection();
                    }
                    else
                    {
                        StartRegionSelection();
                    }
                    break;
                case Keys.Delete:
                    DeleteCurrentShape();

                    if (IsCreating)
                    {
                        EndRegionSelection();
                    }
                    break;
                case Keys.Shift | Keys.Delete:
                    DeleteAllShapes();
                    break;
            }

            var speed = e.Shift ? RegionCaptureOptions.MoveSpeedMaximum : RegionCaptureOptions.MoveSpeedMinimum;
            var x = 0;

            if (isLeftPressed)
            {
                x -= speed;
            }

            if (isRightPressed)
            {
                x += speed;
            }

            int y = 0;

            if (isUpPressed)
            {
                y -= speed;
            }

            if (isDownPressed)
            {
                y += speed;
            }

            if (x != 0 || y != 0)
            {
                BaseShape shape = CurrentShape;

                if (shape == null || IsCreating)
                {
                    Cursor.Position = Cursor.Position.Add(x, y);
                }
                else if (e.Control)
                {
                    shape.Resize(x, y, true);
                }
                else if (e.Alt)
                {
                    shape.Resize(x, y, false);
                }
                else
                {
                    shape.Move(x, y);
                }
            }
        }

        private void form_KeyUp(object sender, KeyEventArgs e)
        {
            bool wasMoving = isLeftPressed || isRightPressed || isUpPressed || isDownPressed;

            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    IsCtrlModifier = false;
                    IsCornerMoving = false;
                    break;
                case Keys.ShiftKey:
                    IsProportionalResizing = false;
                    break;
                case Keys.Menu:
                    IsSnapResizing = false;
                    break;
                case Keys.Left:
                    isLeftPressed = false;
                    break;
                case Keys.Right:
                    isRightPressed = false;
                    break;
                case Keys.Up:
                    isUpPressed = false;
                    break;
                case Keys.Down:
                    isDownPressed = false;
                    break;
            }

            if (IsCreating || IsMoving || !wasMoving) return;

            if (!(isLeftPressed || isRightPressed || isUpPressed || isDownPressed))
            {
                ShapeMoved();
            }
        }

        private void ShapeMoved()
        {
            if (IsCreating) return;
            CurrentShape?.OnMoved();
        }

        private void RunAction(RegionCaptureAction action)
        {
            switch (action)
            {
                case RegionCaptureAction.CancelCapture:
                    Form.CloseWindow();
                    break;
                case RegionCaptureAction.RemoveShapeCancelCapture:
                    if (IsShapeIntersect())
                    {
                        DeleteIntersectShape();
                    }
                    else
                    {
                        Form.CloseWindow();
                    }
                    break;
                case RegionCaptureAction.RemoveShape:
                    DeleteIntersectShape();
                    break;
                case RegionCaptureAction.SwapToolType:
                    break;
                case RegionCaptureAction.CaptureFullscreen:
                case RegionCaptureAction.CaptureActiveMonitor:
                case RegionCaptureAction.CaptureLastRegion:
                    Form.CloseWindow();
                    break;
            }
        }

        public void Update()
        {
            var shape = CurrentShape;

            shape?.OnUpdate();

            UpdateCurrentHoverShape();

            UpdateNodes();
        }

        public void StartRegionSelection()
        {
            if (IsCursorOnObject)
                return;
            InputManager.Update(Form); // If it's a touch event we don't have the correct point yet, so refresh it now
            var shape = GetIntersectShape();
            if (shape != null && shape.IsSelectable) // Select shape
            {
                DeselectCurrentShape();
                IsMoving = true;
                CurrentShape = shape;
            }
            else if (shape == null && IsCreating)
            {
                DeselectCurrentShape();
            }
            else if (!IsCreating) // Create new shape
            {
                DeselectCurrentShape();

                shape = AddShape();
                shape.OnCreating();
            }
        }

        public void EndRegionSelection()
        {
            var wasCreating = IsCreating;
            var wasMoving = IsMoving;

            IsCreating = false;
            IsMoving = false;

            var shape = CurrentShape;

            if (shape == null) return;
            if (!shape.IsValidShape)
            {
                shape.Rectangle = Rectangle.Empty;

                UpdateCurrentHoverShape();

                if (IsCurrentHoverShapeValid)
                {
                    shape.Rectangle = CurrentHoverShape.Rectangle;
                }
                else
                {
                    DeleteCurrentShape();
                    shape = null;
                }
            }

            if (shape == null) return;
            if (Options.QuickCrop && IsCurrentShapeTypeRegion)
            {
                Form.UpdateRegionPath();
                Form.CopyImage();
                Form.CloseWindow(true);
            }
            else
            {
                if (wasCreating)
                {
                    shape.OnCreated();
                    OnShapeCreated(shape);
                }
                else if (wasMoving)
                {
                    shape.OnMoved();
                }
            }
        }


        internal void DrawObjects(Graphics g)
        {
            if (IsCtrlModifier) return;
            foreach (var obj in DrawableObjects.Where(obj => obj.Visible))
            {
                obj.OnDraw(g);
            }
        }

        private BaseShape AddShape()
        {
            var shape = CreateShape();
            AddShape(shape);
            return shape;
        }

        private void AddShape(BaseShape shape)
        {
            Shapes.Add(shape);
            CurrentShape = shape;
        }

        private BaseShape CreateShape()
        {
            return CreateShape(CurrentTool);
        }

        private BaseShape CreateShape(ShapeType shapeType)
        {
            BaseShape shape;
            switch (shapeType)
            {
                default:
                case ShapeType.RegionRectangle:
                    shape = new RectangleRegionShape();
                    break;
                case ShapeType.DrawingCursor:
                    shape = new CursorDrawingShape();
                    break;
            }
            shape.Manager = this;
            shape.OnConfigLoad();
            return shape;
        }


        public PointF SnapPosition(PointF posOnClick, PointF posCurrent)
        {
            var currentSize = CaptureHelpers.CreateRectangle(posOnClick, posCurrent).Size;
            var vector = new Vector2(currentSize.Width, currentSize.Height);

            var snapSize = (from size in Options.SnapSizes
                                 let distance = MathHelpers.Distance(vector, new Vector2(size.Width, size.Height))
                                 where distance > 0 && distance < RegionCaptureOptions.SnapDistance
                                 orderby distance
                                 select size).FirstOrDefault();

            if (snapSize == null) return posCurrent;
            var posNew = CaptureHelpers.CalculateNewPosition(posOnClick, posCurrent, snapSize);

            var newRect = CaptureHelpers.CreateRectangle(posOnClick, posNew);

            return Form.ClientArea.Contains(newRect.Round()) ? posNew : posCurrent;
        }

        private void UpdateCurrentHoverShape()
        {
            CurrentHoverShape = (BaseRegionShape)CheckHover();
        }

        private BaseShape CheckHover()
        {
            if (IsCursorOnObject || IsCreating || IsMoving || IsResizing) return null;
            var shape = GetIntersectShape();

            if (shape != null && shape.IsValidShape)
            {
                return shape;
            }

            if (currentTool == ShapeType.DrawingCursor)
            {
                return null;

            }
            if (Options.IsFixedSize && IsCurrentShapeTypeRegion)
            {
                var location = Form.ScaledClientMousePosition;

                var rectangleRegionShape = CreateShape(ShapeType.RegionRectangle);
                rectangleRegionShape.Rectangle = new RectangleF(
                    new PointF(location.X - (Options.FixedSize.Width / 2),
                        location.Y - (Options.FixedSize.Height / 2)), Options.FixedSize);
                return rectangleRegionShape;
            }
            else
            {
                var window = FindSelectedWindow();

                if (window == null || window.Rectangle.IsEmpty) return null;
                var hoverArea = Form.RectangleToClient(window.Rectangle);

                var rectangleRegionShape = CreateShape(ShapeType.RegionRectangle);
                rectangleRegionShape.Rectangle = Rectangle.Intersect(Form.ClientArea, hoverArea);
                return rectangleRegionShape;
            }

        }

        public SimpleWindowInfo FindSelectedWindow()
        {
            return Windows?.FirstOrDefault(x => x.Rectangle.Contains(InputManager.MousePosition));
        }

        public Bitmap RenderOutputImage(Bitmap bmp, PointF offset)
        {
            if (bmp == null) return null;
            var bmpOutput = (Bitmap)bmp.Clone();
            bmpOutput.SetResolution(CurrentDpi.X, CurrentDpi.Y);

            if (DrawingShapes.Length <= 0) return bmpOutput;
            IsRenderingOutput = true;
            RenderOffset = offset;

            MoveAll(-offset.X, -offset.Y);

            using (var g = Graphics.FromImage(bmpOutput))
            {
                foreach (var shape in DrawingShapes)
                {
                    shape?.OnDraw(g);
                }
            }

            MoveAll(offset);

            RenderOffset = Point.Empty;
            IsRenderingOutput = false;

            return bmpOutput;
        }

        private void DeselectShape(BaseShape shape)
        {
            if (shape != CurrentShape) return;
            CurrentShape = null;
            NodesVisible = false;
        }

        private void DeselectCurrentShape()
        {
            DeselectShape(CurrentShape);
        }

        public void DeleteShape(BaseShape shape)
        {
            if (shape == null) return;
            shape.Dispose();
            Shapes.Remove(shape);
            DeselectShape(shape);

            if (shape.ShapeCategory == ShapeCategory.Drawing || shape.ShapeCategory == ShapeCategory.Effect)
            {
                OnImageModified();
            }
        }

        private void DeleteCurrentShape()
        {
            DeleteShape(CurrentShape);
        }

        private void DeleteIntersectShape()
        {
            DeleteShape(GetIntersectShape());
        }

        private void DeleteAllShapes()
        {
            if (Shapes.Count <= 0) return;
            foreach (var shape in Shapes)
            {
                shape.Dispose();
            }

            Shapes.Clear();
            DeselectCurrentShape();
            OnImageModified();
        }

        private void ResetModifiers()
        {
            IsCtrlModifier = IsCornerMoving = IsProportionalResizing = IsSnapResizing = false;
        }

        public BaseShape GetIntersectShape()
        {
            return GetIntersectShape(Form.ScaledClientMousePosition);
        }

        public BaseShape GetIntersectShape(PointF position)
        {
            //if (IsCtrlModifier) return null;
            for (var i = Shapes.Count - 1; i >= 0; i--)
            {
                var shape = Shapes[i];

                if (shape.IsSelectable && shape.Intersects(position))
                {
                    return shape;
                }
            }

            return null;
        }

        public bool IsShapeIntersect()
        {
            return GetIntersectShape() != null;
        }


        public void MoveAll(float x, float y)
        {
            if (x == 0 && y == 0) return;
            foreach (var shape in Shapes)
            {
                shape.Move(x, y);
            }
        }

        public void MoveAll(PointF offset)
        {
            MoveAll(offset.X, offset.Y);
        }

        private bool IsShapeTypeRegion(ShapeType shapeType)
        {
            return shapeType == ShapeType.RegionRectangle;
        }

        private void UpdateNodes()
        {
            var shape = CurrentShape;

            if (shape == null || !NodesVisible) return;
            if (!InputManager.IsMouseDown(MouseButtons.Left) && IsResizing)
            {
                IsResizing = false;
            }

        }

        public void OrderStepShapes()
        {
            //var i = StartingStepNumber;

            /*foreach (var shape in Shapes.OfType<StepDrawingShape>())
            {
                shape.Number = i++;
            }*/
        }

        public void AddCursor(Bitmap bmpCursor, Point position)
        {
            var shape = (CursorDrawingShape)CreateShape(ShapeType.DrawingCursor);
            shape.UpdateCursor(bmpCursor, position);
            Shapes.Add(shape);
        }

        public void DrawRegionArea(Graphics g, RectangleF rect, bool isAnimated, bool showAreaInfo = false)
        {
            Form.DrawRegionArea(g, rect, isAnimated);

            if (!showAreaInfo) return;
            var areaText = Form.GetAreaText(rect);
            Form.DrawAreaText(g, areaText, rect);
        }

        public Color GetColor(Bitmap bmp, Point pos)
        {
            if (bmp == null) return Color.Empty;
            var position = CaptureHelpers.ScreenToClient(pos);
            var offset = CaptureHelpers.ScreenToClient(Form.CanvasRectangle.Location.Round());
            position.X -= offset.X;
            position.Y -= offset.Y;

            if (position.X.IsBetween(0, bmp.Width - 1) && position.Y.IsBetween(0, bmp.Height - 1))
            {
                return bmp.GetPixel(position.X, position.Y);
            }
            return Color.Empty;
        }

        public Color GetCurrentColor(Bitmap bmp)
        {
            return GetColor(bmp, Form.ScaledClientMousePosition.Round());
        }

        public Color GetCurrentColor()
        {
            return GetCurrentColor(Form.Canvas);
        }


        public void Dispose()
        {
            DeleteAllShapes();
        }
    }
}