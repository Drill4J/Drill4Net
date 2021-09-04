using System.IO;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks.File
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;

        /**********************************************************************/

        public FileLoggerProvider(string path = null)
        {

            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(FileUtils.ExecutingDir, "emergency.log");
            _path = path;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileSink(_path);
        }

        public void Dispose()
        {
            //TODO: create getting & removing sink in FileSinkCreator + its Disposing here !!!
        }
    }
}
