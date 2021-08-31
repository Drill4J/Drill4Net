using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog
{
    public class TypedLogger<T> : Logger, ILogger<T> where T : class
    {
        public TypedLogger(string subsystem = null, Dictionary<string, object> extras = null) : base(typeof(T).Name, subsystem, extras)
        {
        }
    }
}