using System;

namespace ShareX
{
    public static class NumberExtensions
    {
        public static T Clamp<T>(this T num, T min, T max) where T : IComparable<T>
        {
            return MathHelpers.Clamp(num, min, max);
        }

        public static bool IsBetween<T>(this T num, T min, T max) where T : IComparable<T>
        {
            return MathHelpers.IsBetween(num, min, max);
        }
    }
}