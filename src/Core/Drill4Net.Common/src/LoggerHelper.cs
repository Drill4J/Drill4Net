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

        protected virtual string GetCommonFilePath()
        {
            return Path.Combine(FileUtils.GetExecutionDir(), "logs", "log.txt");
        }
    }
}
