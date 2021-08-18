using System;
using System.Collections.Generic;
using System.Text;

namespace Drill4Net.BanderLog.Sinks.File
{
      static class FileSinkConstants
    {
        internal const string NAME_DEFAULT = "log.txt";
        internal const int LINES_TO_FLUSH_COUNTER=5000;
        internal const double  MAX_TIME_GAP_FOR_FLUSH = 1;
    }
}
