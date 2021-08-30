using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    /// <summary>
    /// Extensions for BanderLog's system
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Retrieving the key of the logger sink extending excactly Miscosoft ILogger interface
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static int GetKey(this ILogger logger)
        {
            return logger.GetHashCode();
        }
    }
}
