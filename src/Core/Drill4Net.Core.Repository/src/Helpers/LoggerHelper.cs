using System.IO;
using Drill4Net.Common;

namespace Drill4Net.Core.Repository
{
    public static class LoggerHelper
    {
        public const string LOG_FOLDER = "logs_drill";
        public static string LOG_FILENAME = "log.txt";

        /******************************************************************************************/

        /// <summary>
        /// Get the common folder to log events (for all components of system)
        /// </summary>
        /// <returns></returns>
        public static string GetCommonFilePath(string folder = LOG_FOLDER)
        {
            var dir = string.IsNullOrWhiteSpace(folder) ? LOG_FOLDER : folder;
            return Path.Combine(FileUtils.EntryDir, dir, LOG_FILENAME);
        }

        public static string GetDefaultLogDir()
        {
            return Path.Combine(FileUtils.EntryDir, LOG_FOLDER);
        }

        public static string GetDefaultLogPath()
        {
            return Path.Combine(GetDefaultLogDir(), LOG_FILENAME);
        }
    }
}
