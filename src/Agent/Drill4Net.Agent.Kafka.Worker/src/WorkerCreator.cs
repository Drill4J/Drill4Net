using System;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public class WorkerCreator
    {
        private readonly string[] _args;

        /**************************************************************************/

        public WorkerCreator(string[] appArgs)
        {
            _args = appArgs ?? throw new ArgumentNullException(nameof(appArgs));
        }

        /**************************************************************************/

        internal virtual AbstractRepository<CommunicatorOptions> GetRepository()
        {
            return new KafkaConsumerRepository();
        }

        internal virtual TargetInfo GetTarget(string[] args)
        {
            var targetArg = AbstractRepository<CommunicatorOptions>.GetArgument(args, KafkaTransportConstants.ARGUMENT_TARGET_INFO);
            var target = TargetInfoArgumentSerializer.Deserialize(targetArg);
            if (targetArg == null)
                throw new Exception("Target info doesn't deserialize");
            return target;
        }

        public virtual IProbeReceiver CreateWorker()
        {
            var rep = GetRepository();
            IKafkaWorkerReceiver consumer = new KafkaWorkerReceiver(rep);
            var target = GetTarget(_args);
            var agent = new CoverageWorker(target, consumer);
            return agent;
        }
    }
}
