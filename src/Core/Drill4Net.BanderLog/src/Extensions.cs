using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public static class Extensions
    {
        public static int GetKey(this ILogger logger)
        {
            return logger.GetHashCode();
        }
    }
}
