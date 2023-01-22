using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX
{
    public abstract class BaseShape : IDisposable
    {
        public abstract ShapeCategory ShapeCategory { get; }

        public abstract ShapeType ShapeType { get; }

        private RectangleF rectangle;

        public RectangleF Rectangle
        {
            get => rectangle;
            set
            {
                rectangle = value;
                startPosition = rectangle.Location;
                endPosition = new PointF(rectangle.X + rectangle.Width - 1, rectangle.Y + rectangle.Height - 1);
            }
        }

        private PointF startPosition;

        public PointF StartPosition
        {
            get => startPosition;
            private set
            {
                startPosition = value;
                rectangle = CaptureHelpers.CreateRectangle(startPosition, endPosition);
            }
        }

        private PointF endPosition;

        public PointF EndPosition
        {
            get => endPosition;
            private set
            {
                endPosition = value;
                rectangle = CaptureHelpers.CreateRectangle(startPosition, endPosition);
            }
        }

        public SizeF InitialSize { get; set; }

        public virtual bool IsValidShape => !Rectangle.IsEmpty && Rectangle.Width >= Options.MinimumSize && Rectangle.Height >= Options.MinimumSize;

        public virtual bool IsSelectable => Manager.CurrentTool == ShapeType;

        public bool ForceProportionalResizing { get; protected set; }

        internal ShapeManager Manager { get; set; }

        protected RegionCaptureOptions Options => Manager.Options;
        protected AnnotationOptions AnnotationOptions => Manager.Options.AnnotationOptions;

        public virtual bool Intersects(PointF position)
        {
            return Rectangle.Contains(position);
        }

        public void AddShapePath(GraphicsPath gp, int sizeOffset = 0)
        {
            RectangleF rect = Rectangle;

            if (sizeOffset != 0)
            {
                rect = rect.SizeOffset(sizeOffset);
            }

            OnShapePathRequested(gp, rect);
        }

        public virtual void Move(float x, float y)
        {
            StartPosition = StartPosition.Add(x, y);
            EndPosition = EndPosition.Add(x, y);
        }

        public virtual void Resize(int x, int y, bool fromBottomRight)
        {
            Rectangle = fromBottomRight ? Rectangle.SizeOffset(x, y) : Rectangle.LocationOffset(x, y).SizeOffset(-x, -y);
        }

        public virtual BaseShape Duplicate()
        {
            ShapeManager manager = Manager;
            Manager = null;
            BaseShape shape = this.Copy();
            Manager = manager;
            shape.Manager = manager;
            return shape;
        }

        public virtual void OnCreating()
        {
            var pos = Manager.Form.ScaledClientMousePosition;

            if (Options.IsFixedSize && ShapeCategory == ShapeCategory.Region)
            {
                Manager.IsMoving = true;
                Rectangle = new RectangleF(new PointF(pos.X - (Options.FixedSize.Width / 2), pos.Y - (Options.FixedSize.Height / 2)), Options.FixedSize);
                OnCreated();
            }
            else
            {
                Manager.IsCreating = true;
                Rectangle = new RectangleF(pos.X, pos.Y, 1, 1);
            }
        }

        public virtual void OnCreated()
        {
            InitialSize = Rectangle.Size;

            if (ShapeCategory == ShapeCategory.Drawing || ShapeCategory == ShapeCategory.Effect)
            {
                Manager.OnImageModified();
            }
        }

        public virtual void OnMoved()
        {
        }


        public virtual void OnUpdate()
        {
            if (Manager.IsCreating)
            {
                var pos = Manager.Form.ScaledClientMousePosition;

                if (Manager.IsCornerMoving && !Manager.IsPanning)
                {
                    StartPosition = StartPosition.Add(Manager.Form.ScaledClientMouseVelocity);
                }
                if (Manager.IsProportionalResizing || ForceProportionalResizing)
                {
                    pos = CaptureHelpers.SnapPositionToDegree(StartPosition, pos, 90, 45).Round();
                }
                else if (Manager.IsSnapResizing)
                {
                    pos = Manager.SnapPosition(StartPosition, pos);
                }

                EndPosition = pos;
            }
            else if (Manager.IsMoving && !Manager.IsPanning)
            {
                Move(Manager.Form.ScaledClientMouseVelocity.X, Manager.Form.ScaledClientMouseVelocity.Y);
            }
        }

        public virtual void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddRectangle(rect);
        }

        public virtual void OnConfigLoad()
        {
        }

        public virtual void OnConfigSave()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}