using Drill4Net.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drill4Net.BanderLog.Sinks.File
{
    public static class FileSinkCreator
    {
        private static Dictionary<string, FileSink> _fileSinkDictionary=new Dictionary<string, FileSink>();


        public static FileSink CreateSink(string filepath = null)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = Path.Combine(FileUtils.GetEntryDir(), FileSinkConstants.NAME_DEFAULT);
            FileSink fileSink;
            if (_fileSinkDictionary.TryGetValue(filepath, out fileSink))
                return fileSink;
            else
            {
                fileSink = new FileSink(filepath);
                _fileSinkDictionary.Add(filepath, fileSink);
                return fileSink;
            }
        }
    }
}
