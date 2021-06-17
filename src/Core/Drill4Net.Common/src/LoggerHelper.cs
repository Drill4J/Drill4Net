using System.IO;
using Serilog;

namespace Drill4Net.Common
{
    public class LoggerHelper
    {
        public static string LOG_FILENAME = "log.txt";

        /// <summary>
        /// Gets the base logger configuration for the logging into console and 
        /// local text file with the Verbose level.
        /// </summary>
        /// <returns></returns>
        public virtual LoggerConfiguration GetBaseLoggerConfiguration()
        {
            var cfg = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File(GetCommonFilePath());
            return cfg;
        }

        /// <summary>
        /// Get the common folder to log events (for all components of system)
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommonFilePath()
        {
            return Path.Combine(FileUtils.GetExecutionDir(), "logs", LOG_FILENAME);
        }
    }
}
