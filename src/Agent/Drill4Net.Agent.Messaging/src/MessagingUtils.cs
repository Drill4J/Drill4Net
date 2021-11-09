using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging
{
    public static class MessagingUtils
    {
        public static IEnumerable<string> FilterProbeTopics(IEnumerable<string> topics)
        {
            if (topics == null)
                return new List<string>();
            return topics.Where(a => a.StartsWith($"{MessagingConstants.TOPIC_PROBE_PREFIX}")).ToList();
        }

        public static IEnumerable<string> FilterCommandTopics(IEnumerable<string> topics)
        {
            if(topics == null)
                return new List<string>();
            return topics.Where(a => a.StartsWith($"{MessagingConstants.TOPIC_COMMAND_PREFIX}")).ToList();
        }

        public static IEnumerable<string> FilterTargetTopics(IEnumerable<string> topics, bool isServer)
        {
            if (topics == null)
                return new List<string>();
            var targTopics = topics.Where(a => a == MessagingConstants.TOPIC_TARGET_INFO ||
                                                a.StartsWith($"{MessagingConstants.TOPIC_TARGET_INFO}_")
                                          ).ToList();
            if (targTopics.Count == 0 && isServer)
                targTopics.Add(MessagingConstants.TOPIC_TARGET_INFO);
            return targTopics;
        }

        #region GetTargetWorkerTopic
        public static string GetTargetWorkerTopic(Guid sessionUid)
        {
            return GetTargetWorkerTopic(sessionUid.ToString());
        }

        public static string GetTargetWorkerTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_TARGET_INFO}_{sessionUid}";
        }
        #endregion
        #region GetCommandTopic
        public static string GetCommandToWorkerTopic(Guid sessionUid)
        {
            return GetCommandToWorkerTopic(sessionUid.ToString());
        }

        public static string GetCommandToWorkerTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_COMMAND_WORKER_PREFIX}_{sessionUid}";
        }

        public static string GetCommandToTransmitterTopic(Guid sessionUid)
        {
            return GetCommandToTransmitterTopic(sessionUid.ToString());
        }

        public static string GetCommandToTransmitterTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_COMMAND_TRANSMITTER_PREFIX}_{sessionUid}";
        }
        #endregion
        #region GetProbeTopic
        public static string GetProbeTopic(Guid sessionUid)
        {
            return GetProbeTopic(sessionUid.ToString());
        }

        public static string GetProbeTopic(string sessionUid)
        {
            return $"{MessagingConstants.TOPIC_PROBE_PREFIX}_{sessionUid}";
        }
        #endregion

        public static string GetLogPrefix(string subsystem, Type type)
        {
            return $"{subsystem}|{CommonUtils.CurrentProcessId}|{type.Name}|";
        }
    }
}
