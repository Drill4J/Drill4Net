using System.IO;
using Serilog;

namespace Drill4Net.Common
{
    public class LoggerHelper
    {
        public const string LOG_DIR_DEFAULT = "logs";
        public static string LOG_FILENAME = "log.txt";

        /// <summary>
        /// Gets the base logger configuration for the logging into console and 
        /// local text file with the Verbose level.
        /// </summary>
        /// <returns></returns>
        public virtual LoggerConfiguration GetBaseLoggerConfiguration(string folder = LOG_DIR_DEFAULT)
        {
            var cfg = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File(GetCommonFilePath(folder));
            return cfg;
        }

        /// <summary>
        /// Get the common folder to log events (for all components of system)
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommonFilePath(string folder = LOG_DIR_DEFAULT)
        {
            var dir = string.IsNullOrWhiteSpace(folder) ? LOG_DIR_DEFAULT : folder;
            return Path.Combine(FileUtils.GetEntryDir(), dir, LOG_FILENAME);
        }
    }
}
