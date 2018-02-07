using System;
using AGS.API;

namespace LastAndFurious
{
    /// <summary>
    /// Two-dimensional vector algebra, implemented as an extension to Vector2 class.
    /// </summary>
    public static class Vectors
    {
        /// <summary>
        /// Calculates an angle between this vector and X axis (1; 0).
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>Angle in radians.</returns>
        public static float Angle(this Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }
    }
}
