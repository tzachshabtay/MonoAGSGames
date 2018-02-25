using System;

namespace LastAndFurious
{
    // TODO: suggest adding some of these into the engine.
    public static class MathEx
    {
        /// <summary>
        /// Tests the given float and throws an exception if it's not a valid number.
        /// </summary>
        /// <param name="f"></param>
        public static void AssertFloat(float f)
        {
            try
            {
                if (float.IsInfinity(f) || float.IsNaN(f))
                    throw new OverflowException("Given float is not a valid number.");
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Converts angle to the range of 0-359 degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float Angle360(float degrees)
        {
            float angle = degrees % 360;
            if (angle >= 0) return angle;
            return 360 - (-angle);
        }

        // Converts angle to the range of [-Pi..+Pi], the angle must be in the range [-3*Pi, 3*Pi]
        public static float AnglePiFast(float rads)
        {
            if (rads > Math.PI)
                return (float)(rads - Math.PI * 2.0);
            else if (rads < -Math.PI)
                return (float)(rads + Math.PI * 2.0);
            return rads;
        }

        // Converts angle to the range of [0..2Pi)
        public static float Angle2Pi(float rads)
        {
            const float pi2 = (float)Math.PI * 2.0f;
            // TODO: needs more testing
            float angle;
            if (rads >= 0.0f)
                angle = rads - pi2 * (float)Math.Floor(rads / pi2);
            else
                angle = rads - pi2 * (float)Math.Ceiling(rads / pi2);
            if (angle >= 0.0f)
                return angle;
            return pi2 - (-angle);
        }

    }
}
