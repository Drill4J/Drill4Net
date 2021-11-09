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
        public string CommandToWorkerTopic { get; }
        public string CommandToTransmitterTopic { get; }

        /*************************************************************************************/

        public WorkerInfo(TargetInfo targetInfotarget, int pID, string targetTopic,
            string probeTopic, string cmdToWorkerTopic, string cmdToTransTopic)
        {
            Target = targetInfotarget ?? throw new ArgumentNullException(nameof(targetInfotarget));
            TargetInfoTopic = targetTopic ?? throw new ArgumentNullException(nameof(targetTopic));
            ProbeTopic = probeTopic ?? throw new ArgumentNullException(nameof(probeTopic));
            CommandToWorkerTopic = cmdToWorkerTopic ?? throw new ArgumentNullException(nameof(cmdToWorkerTopic));
            CommandToTransmitterTopic = cmdToTransTopic ?? throw new ArgumentNullException(nameof(cmdToTransTopic));
            PID = pID;
        }
    }
}
