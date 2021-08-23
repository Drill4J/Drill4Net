using System;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transport
{
    public static class TransportUtils
    {
        public static string GetLogPrefix(string subsystem, Type type)
        {
            return $"{subsystem}|{CommonUtils.CurrentProcessId}|{type.Name}|";
        }
    }
}
