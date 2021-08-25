using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

namespace Drill4Net.Agent.Worker
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
            ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver(rep);
            var worker = new AgentWorker(targetReceiver, probeReceiver);
            return worker;
        }

        internal virtual AbstractRepository<MessageReceiverOptions> GetRepository()
        {
            var opts = GetBaseOptions(_args);
            var targetTopic = GetTargetTopic(_args);
            if(!string.IsNullOrWhiteSpace(targetTopic))
                opts.Topics.Add(targetTopic);
            return new MessageReceiverRepository(CoreConstants.SUBSYSTEM_AGENT_WORKER, opts);
        }

        private string GetTargetTopic(string[] args)
        {
            return MessageReceiverRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_TARGET_TOPIC);
        }

        internal virtual MessageReceiverOptions GetBaseOptions(string[] args)
        {
            var cfgPathArg = MessageReceiverRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_CONFIG_PATH);
            var opts = MessageReceiverRepository.GetOptionsByPath(cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            return opts;
        }
    }
}
