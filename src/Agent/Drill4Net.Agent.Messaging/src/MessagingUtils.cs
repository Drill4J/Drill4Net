using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging
{
    public static class MessagingUtils
    {
        public static string GetTargetWorkerTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_TARGET_INFO}_{sessionUid}";
        }

        public static List<string> FilterProbeTopics(IEnumerable<string> topics)
        {
            return topics?.Where(a => a.StartsWith($"{MessagingConstants.TOPIC_PROBE_PREFIX}")).ToList();
        }

        public static List<string> FilterTargetTopics(IEnumerable<string> topics)
        {
            var targTopics = topics?.Where(a => a == MessagingConstants.TOPIC_TARGET_INFO ||
                                                a.StartsWith($"{MessagingConstants.TOPIC_TARGET_INFO}_")
                                          ).ToList();
            if (!targTopics.Any())
                targTopics.Add(MessagingConstants.TOPIC_TARGET_INFO);
            return targTopics;
        }

        public static string GetCommandTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_COMMAND_PREFIX}_{sessionUid}";
        }

        public static string GetProbeTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_PROBE_PREFIX}_{sessionUid}";
        }

        public static string GetLogPrefix(string subsystem, Type type)
        {
            return $"{subsystem}|{CommonUtils.CurrentProcessId}|{type.Name}|";
        }
    }
}
