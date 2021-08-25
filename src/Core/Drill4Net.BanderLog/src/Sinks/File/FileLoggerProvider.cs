using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.File
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;

        /***************************************************/

        public FileLoggerProvider(string path)
        {
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
