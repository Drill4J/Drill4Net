using System;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    public class WorkerInfo
    {
        public TargetInfo Target { get; }
        public int PID { get; }
        public string Topic { get; }

        /******************************************************/

        public WorkerInfo(TargetInfo target, string topic, int pID)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            PID = pID;
        }
    }
}
