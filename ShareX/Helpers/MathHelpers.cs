using System;
using System.Drawing;

namespace ShareX
{
    public static class MathHelpers
    {
        public const float DegreePi = 0.01745329f; // Math.PI / 180.0

        public static T Clamp<T>(T num, T min, T max) where T : IComparable<T>
        {
            if (num.CompareTo(min) <= 0) return min;
            if (num.CompareTo(max) >= 0) return max;
            return num;
        }

        public static bool IsBetween<T>(T num, T min, T max) where T : IComparable<T>
        {
            return num.CompareTo(min) >= 0 && num.CompareTo(max) <= 0;
        }

        public static float DegreeToRadian(float degree)
        {
            return degree * DegreePi;
        }

        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));
        }

        public static Vector2 RadianToVector2(float radian, float length)
        {
            return RadianToVector2(radian) * length;
        }

        public static float LookAtRadian(PointF pos1, PointF pos2)
        {
            return (float)Math.Atan2(pos2.Y - pos1.Y, pos2.X - pos1.X);
        }

        public static float Distance(Vector2 pos1, Vector2 pos2)
        {
            return (float)Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2));
        }

        public static float Distance(PointF pos1, PointF pos2)
        {
            return (float)Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2));
        }
    }
}