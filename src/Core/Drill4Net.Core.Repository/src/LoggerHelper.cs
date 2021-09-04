using System.IO;
using Drill4Net.Common;

namespace Drill4Net.Core.Repository
{
    public static class LoggerHelper
    {
        public const string LOG_DIR_DEFAULT = "logs";
        public static string LOG_FILENAME = "log.txt";

        /******************************************************************************************/

        /// <summary>
        /// Get the common folder to log events (for all components of system)
        /// </summary>
        /// <returns></returns>
        public static string GetCommonFilePath(string folder = LOG_DIR_DEFAULT)
        {
            var dir = string.IsNullOrWhiteSpace(folder) ? LOG_DIR_DEFAULT : folder;
            return Path.Combine(FileUtils.EntryDir, dir, LOG_FILENAME);
        }
    }
}
