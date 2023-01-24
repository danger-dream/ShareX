using System;
using System.Drawing;

namespace ShareX
{
    internal abstract class ImageEditorControl
    {
        public event Action MouseEnter, MouseLeave;

        public bool Visible { get; set; }
        public bool HandleMouseInput { get; set; } = true;
        public RectangleF Rectangle { get; set; }

        private bool isCursorHover;

        public bool IsCursorHover
        {
            get => isCursorHover;
            set
            {
                if (isCursorHover == value) return;
                isCursorHover = value;
                if (isCursorHover)
                {
                    OnMouseEnter();
                }
                else
                {
                    OnMouseLeave();
                }
            }
        }

        public bool IsDragging { get; protected set; }

        public virtual void OnDraw(Graphics g)
        {
            if (IsDragging)
            {
                g.FillRectangle(Brushes.Blue, Rectangle);
            }
            else if (IsCursorHover)
            {
                g.FillRectangle(Brushes.Green, Rectangle);
            }
            else
            {
                g.FillRectangle(Brushes.Red, Rectangle);
            }
        }

        public virtual void OnMouseEnter()
        {
            MouseEnter?.Invoke();
        }

        public virtual void OnMouseLeave()
        {
            MouseLeave?.Invoke();
        }
    }
}