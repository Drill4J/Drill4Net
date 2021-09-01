using System.Collections.Generic;

namespace Drill4Net.BanderLog
{
    public interface ILoggerData
    {
        string Category { get; }
        Dictionary<string, object> Extras { get; }
        string ExtrasString { get; }
        string Subsystem { get; }
    }
}
