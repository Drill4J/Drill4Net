using System;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    public class WorkerInfo
    {
        public TargetInfo Target { get; }
        public int PID { get; }

        /******************************************************/

        public WorkerInfo(TargetInfo target, int pID)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            PID = pID;
        }
    }
}
