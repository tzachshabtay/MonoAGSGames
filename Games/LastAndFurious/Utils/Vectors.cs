using System;
using AGS.API;

namespace LastAndFurious
{
    /// <summary>
    /// Helper struct, depicts a 2D region defined by the two points.
    /// </summary>
    /// TODO: find a substitution in the engine's API.
    public struct Vecregion
    {
        public Vector2 P1;
        public Vector2 P2;

        public Vecregion(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public bool IsInside(Vector2 p)
        {
            return p.X >= P1.X && p.X <= P2.X && p.Y >= P1.Y && p.Y <= P2.Y;
        }
    }

    /// <summary>
    /// Two-dimensional vector algebra, implemented as an extension to Vector2 struct.
    /// </summary>
    /// TODO: search for ways to optimize the calculations.
    /// 
    public static class Vectors
    {
        /// <summary>
        /// Tests given vector and throws an exception if any of its components is not a valid number.
        /// </summary>
        /// <param name="v"></param>
        public static void Assert(Vector2 v)
        {
            MathEx.AssertFloat(v.X);
            MathEx.AssertFloat(v.Y);
        }

        public static Vector2 AddScaled(Vector2 v, Vector2 other, float scale)
        {
            return new Vector2(v.X + other.X * scale, v.Y + other.Y * scale);
        }

        /// <summary>
        /// Calculates an angle between this vector and X axis (1; 0).
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>Angle in radians.</returns>
        public static float Angle(this Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }

        /// <summary>
        /// Calculates distance between two points.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance(Vector2 a, Vector2 b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static bool IsZero(this Vector2 v)
        {
            return MathUtils.FloatEquals(v.X, 0) && MathUtils.FloatEquals(v.Y, 0);
        }

        public static Vector2 MaxValues(Vector2 v, Vector2 other)
        {
            return new Vector2(Math.Max(v.X, other.X), v.Y = Math.Max(v.Y, other.Y));
        }

        public static Vector2 MinValues(Vector2 v, Vector2 other)
        {
            return new Vector2(Math.Min(v.X, other.X), Math.Min(v.Y, other.Y));
        }

        public static Vector2 Negate(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 SafeNormalize(Vector2 v)
        {
            float len = v.Length;
            if (MathUtils.FloatEquals(len, 0.0F))
                return v;
            float n = 1.0F / len;
            return new Vector2(v.X * n, v.Y * n);
        }

        /// <summary>
        /// Rotate vector by the given angle in radians.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rads"></param>
        public static Vector2 Rotate(Vector2 v, float rads)
        {
            float x = (float)(v.X * Math.Cos(rads) - v.Y * Math.Sin(rads));
            float y = (float)(v.X * Math.Sin(rads) + v.Y * Math.Cos(rads));
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns an angle between two vectors in radians, in the range of -PI to PI.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleBetween(Vector2 a, Vector2 b)
        {
            // The result of atan's difference will be in the range of -2Pi to +2Pi
            float angle = (float)(Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X));
            return MathEx.AnglePiFast(angle);
        }

        /// <summary>
        /// Projection of vector a on vector b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Projection(Vector2 a, Vector2 b)
        {
            if (b.IsZero())
                return 0.0F;
            return (a.X * b.X + a.Y * b.Y) / b.Length;
        }
    }
}
