using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog.Sinks;

namespace Drill4Net.BanderLog
{
    public interface ILogManager: ILogSink
    {
        void AddSink(ILogger sink);
        IList<ILogSink> GetSinks();

    }
}