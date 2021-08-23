using System;
using Drill4Net.Common;
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

        public virtual IMessageReceiver CreateWorker()
        {
            var rep = GetRepository();
            IProbeReceiver probeReceiver = new ProbeReceiver(rep);
            ITargetInfoReceiver targetReceiver = new TargetInfoReceiver(rep);
            var worker = new ProbeWorker(targetReceiver, probeReceiver);
            return worker;
        }

        internal virtual AbstractRepository<MessageReceiverOptions> GetRepository()
        {
            var opts = GetBaseOptions(_args);
            var targetTopic = GetTargetTopic(_args);
            if(!string.IsNullOrWhiteSpace(targetTopic))
                opts.Topics.Add(targetTopic);
            return new KafkaReceiverRepository(CoreConstants.SUBSYSTEM_PROBE_WORKER, opts);
        }

        private string GetTargetTopic(string[] args)
        {
            return KafkaReceiverRepository.GetArgument(args, KafkaTransportConstants.ARGUMENT_TARGET_TOPIC);
        }

        internal virtual MessageReceiverOptions GetBaseOptions(string[] args)
        {
            var cfgPathArg = KafkaReceiverRepository.GetArgument(args, KafkaTransportConstants.ARGUMENT_CONFIG_PATH);
            var opts = KafkaReceiverRepository.GetOptionsByPath(cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            return opts;
        }
    }
}
