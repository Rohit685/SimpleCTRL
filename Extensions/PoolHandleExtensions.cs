using Rage;

namespace SimpleCTRL.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="PoolHandle"/> class.
    /// </summary>
    internal static class PoolHandleExtensions
    {
        /// <summary>
        /// Converts a <see cref="PoolHandle"/> to an integer.
        /// </summary>
        /// <param name="poolHandle">The <see cref="PoolHandle"/> to convert.</param>
        /// <returns>The integer value of the <see cref="PoolHandle"/>.</returns>
        public static int ToInt32(this PoolHandle poolHandle)
        {
            return (int)poolHandle.Value;
        }
    }
}
