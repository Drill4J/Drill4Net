using System;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Transport
{
    public abstract class TransportAdmin
    {
        public static string GetLogPrefix(string subsystem, Type type)
        {
            return $"{subsystem}|{CommonUtils.CurrentProcessId}|{type.Name}|";
        }

        public abstract void DeleteTopics(IEnumerable<string> servers, IEnumerable<string> topicNameList);
    }
}
