namespace SimpleCTRL.Extensions
{
    /// <summary>
    /// Provides extension methods for the float type.
    /// </summary>
    internal static class FloatExtensions
    {
        /// <summary>
        /// Returns a safe float value, ensuring it is neither infinity nor NaN.
        /// </summary>
        /// <param name="f">The float value to check.</param>
        /// <param name="_default">The default value to return if the input is infinity or NaN.</param>
        /// <returns>The original float value if it is a valid number; otherwise, the specified default value.</returns>
        public static float SafeFloat(float f, float _default)
        {
            if (float.IsInfinity(f) || float.IsNaN(f))
            {
                f = _default;
            }
            return f;
        }
    }
}
