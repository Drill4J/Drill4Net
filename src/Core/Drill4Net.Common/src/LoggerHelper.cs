using System.IO;
using Serilog;

namespace Drill4Net.Common
{
    public class LoggerHelper
    {
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
        protected virtual string GetCommonFilePath()
        {
            return Path.Combine(FileUtils.GetExecutionDir(), "logs", "log.txt");
        }
    }
}
