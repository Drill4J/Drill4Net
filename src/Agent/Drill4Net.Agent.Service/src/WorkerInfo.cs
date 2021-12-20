using System;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    /// <summary>
    /// Info about target processing worker
    /// </summary>
    public class WorkerInfo
    {
        /// <summary>
        /// Metainfo about target app
        /// </summary>
        public TargetInfo Target { get; }

        /// <summary>
        /// PID of Worker's process
        /// </summary>
        public int PID { get; }

        /// <summary>
        /// Topic in Kafka for extended metainfo about Targer app
        /// </summary>
        public string TargetInfoTopic { get; }

        /// <summary>
        /// Topic in Kafka with probes of executin crosspoints
        /// </summary>
        public string ProbeTopic { get; }

        /// <summary>
        /// Topic in Kafka for commands to Worker
        /// </summary>
        public string CommandToWorkerTopic { get; }

        /// <summary>
        /// Topic in Kafka for commands to Tramsmitter module in Target's memory (system process)
        /// </summary>
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
