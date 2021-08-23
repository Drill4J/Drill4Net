using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Transport
{
    public static class TransportUtils
    {
        public static string GetLogPrefix(string subsystem, Type type)
        {
            return $"{subsystem}|{CommonUtils.CurrentProcessId}|{type.Name}|";
        }

        public static string GetTopicBySessionId(Guid sessionUid)
        {
            return $"{MessagingConstants.TOPIC_TARGET_INFO}_{sessionUid}";
        }

        public static List<string> GetTargetTopics(IEnumerable<string> topics)
        {
            var targTopics = topics?.Where(a => a == MessagingConstants.TOPIC_TARGET_INFO ||
                                                a.StartsWith($"{MessagingConstants.TOPIC_TARGET_INFO}_")
                                          ).ToList();
            if (!targTopics.Any())
                targTopics.Add(MessagingConstants.TOPIC_TARGET_INFO);
            return targTopics;
        }
    }
}
