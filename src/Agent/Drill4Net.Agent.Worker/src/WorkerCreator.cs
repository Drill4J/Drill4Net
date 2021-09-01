using System;
using Drill4Net.Common;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Messaging;
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
            //TODO: factory!
            var rep = GetRepository();
            IProbeReceiver probeReceiver = new ProbeKafkaReceiver(rep);
            ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<MessageReceiverOptions>(rep);
            var worker = new AgentWorker(rep, targetReceiver, probeReceiver);
            return worker;
        }

        internal virtual AgentWorkerRepository GetRepository()
        {
            var opts = GetBaseOptions(_args);
            var targetSession = GetTargetSession(_args);

            var targetTopic = MessagingUtils.GetTargetWorkerTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(targetTopic))
                opts.Topics.Add(targetTopic);

            var probeTopic = MessagingUtils.GetProbeTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(probeTopic))
                opts.Topics.Add(probeTopic);

            return new AgentWorkerRepository(CoreConstants.SUBSYSTEM_AGENT_WORKER, targetSession, opts);
        }

        private string GetTargetSession(string[] args)
        {
            return AbstractRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_TARGET_SESSION);
        }

        internal virtual MessageReceiverOptions GetBaseOptions(string[] args)
        {
            var cfgPathArg = AbstractRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_CONFIG_PATH);
            var opts = AgentWorkerRepository.GetOptionsByPath(cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            return opts;
        }
    }
}
