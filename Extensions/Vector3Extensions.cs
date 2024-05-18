using Rage;
using Rage.Native;

namespace SimpleCTRL.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Vector3"/> class.
    /// </summary>
    internal static class Vector3Extensions
    {
        #region Distance Methods
        /// <summary>
        /// Calculates the distance between two vectors in 3D space.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        public static float DistanceTo(this Vector3 v1, Vector3 v2)
        {
            return DistanceTo(v1, v2, useZ: true);
        }

        /// <summary>
        /// Calculates the distance between two vectors in 2D space (ignoring the Z-axis).
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        /// <returns>The distance between the two vectors (2D).</returns>
        public static float DistanceTo2D(this Vector3 v1, Vector3 v2)
        {
            return DistanceTo(v1, v2, useZ: false);
        }

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="point1">The first vector.</param>
        /// <param name="point2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        public static float DistanceToSquared(this Vector3 point1, Vector3 point2)
        {
            float dx = point1.X - point2.X;
            float dy = point1.Y - point2.Y;
            float dz = point1.Z - point2.Z;

            return dx * dx + dy * dy + dz * dz;
        }
        #endregion

        #region Private Methods
        private static float DistanceTo(Vector3 v1, Vector3 v2, bool useZ)
        {
            return NativeFunction.CallByHash<float>(0xF1B760881820C952, v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, useZ);
        }
        #endregion

        #region GetDistance Method
        /// <summary>
        /// Calculates the distance between two vectors in 3D space.
        /// </summary>
        /// <param name="position1">The first position.</param>
        /// <param name="position2">The second position.</param>
        /// <returns>The distance between the two positions.</returns>
        internal static float GetDistance(Vector3 position1, Vector3 position2)
        {
            return DistanceTo(position1, position2, useZ: true);
        }
        #endregion
    }
}
