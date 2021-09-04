using System.IO;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks.File
{
    public static class FileSinkCreator
    {
        private static readonly Dictionary<string, FileSink> _fileSinkDictionary =
            new Dictionary<string, FileSink>();

        /********************************************************************/

        public static FileSink CreateSink(string filepath = null)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = Path.Combine(FileUtils.EntryDir, FileSinkConstants.NAME_DEFAULT);

            if (_fileSinkDictionary.TryGetValue(filepath, out FileSink fileSink))
            {
                return fileSink;
            }
            else
            {
                fileSink = new FileSink(filepath);
                _fileSinkDictionary.Add(filepath, fileSink);
                return fileSink;
            }
        }
    }
}
