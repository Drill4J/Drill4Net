using Drill4Net.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drill4Net.BanderLog.Sinks.File
{
    public static class FileSinkBuilder
    {
        private static Dictionary<string, FileSink> fileSinkDictionary=new Dictionary<string, FileSink>();
        private const string NAME_DEFAULT = "log.txt";

        public static FileSink CreateSink(string filepath = null)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = Path.Combine(FileUtils.GetEntryDir(), NAME_DEFAULT);
            FileSink fileSink;
            if (fileSinkDictionary.TryGetValue(filepath, out fileSink))
                return fileSink;
            else
            {
                fileSink = new FileSink(filepath);
                fileSinkDictionary.Add(filepath, fileSink);
                return fileSink;
            }
        }

    }
}
