using System;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    public class WorkerInfo
    {
        public TargetInfo Target { get; }
        public int PID { get; }
        public string TargetInfoTopic { get; }
        public string ProbeTopic { get; }

        /******************************************************/

        public WorkerInfo(TargetInfo targtInfotarget, string targetTopic, string probeTopic, int pID)
        {
            Target = targtInfotarget ?? throw new ArgumentNullException(nameof(targtInfotarget));
            TargetInfoTopic = targetTopic ?? throw new ArgumentNullException(nameof(targetTopic));
            ProbeTopic = probeTopic ?? throw new ArgumentNullException(nameof(probeTopic));
            PID = pID;
        }
    }
}
