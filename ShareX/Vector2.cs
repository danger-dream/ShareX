using System;
using System.Drawing;

namespace ShareX
{
    public struct Vector2
    {
        private float x, y;

        public float X
        {
            get => x;
            set => x = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Vector2 u, Vector2 v)
        {
            return u.x == v.x && u.y == v.y;
        }

        public static bool operator !=(Vector2 u, Vector2 v)
        {
            return !(u == v);
        }

        public static Vector2 operator +(Vector2 u, Vector2 v)
        {
            return new Vector2(u.x + v.x, u.y + v.y);
        }

        public static Vector2 operator -(Vector2 u, Vector2 v)
        {
            return new Vector2(u.x - v.x, u.y - v.y);
        }

        public static Vector2 operator *(Vector2 u, float a)
        {
            return new Vector2(a * u.x, a * u.y);
        }

        public static Vector2 operator /(Vector2 u, float a)
        {
            return new Vector2(u.x / a, u.y / a);
        }

        public static Vector2 operator -(Vector2 u)
        {
            return new Vector2(-u.x, -u.y);
        }

        public static explicit operator Point(Vector2 u)
        {
            return new Point((int)Math.Round(u.x), (int)Math.Round(u.y));
        }

        public static explicit operator PointF(Vector2 u)
        {
            return new PointF(u.x, u.y);
        }

        public static implicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}