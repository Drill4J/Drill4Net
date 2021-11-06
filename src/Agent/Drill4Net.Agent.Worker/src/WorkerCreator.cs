using System;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

namespace Drill4Net.Agent.Worker
{
    public class WorkerCreator
    {
        private readonly string[] _args;
        private readonly Logger _logger;

        /**************************************************************************/

        public WorkerCreator(string[] appArgs)
        {
            _args = appArgs ?? throw new ArgumentNullException(nameof(appArgs));
            _logger = new TypedLogger<WorkerCreator>(CoreConstants.SUBSYSTEM_AGENT_WORKER);
        }

        /**************************************************************************/

        public virtual IMessageReceiver CreateWorker()
        {
            //TODO: factory!
            var rep = GetRepository();

            //receivers
            ICommandReceiver cmdReceiver = new CommandKafkaReceiver(rep);
            IProbeReceiver probeReceiver = new ProbeKafkaReceiver(rep);
            ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<MessageReceiverOptions>(rep);

            //worker
            var worker = new AgentWorker(rep, targetReceiver, probeReceiver, cmdReceiver);
            return worker;
        }

        internal virtual TargetedReceiverRepository GetRepository()
        {
            var opts = GetBaseOptions(_args);
            var targetSession = GetTargetSession(_args);

            var targetTopic = MessagingUtils.GetTargetWorkerTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(targetTopic))
                opts.Topics.Add(targetTopic);

            var probeTopic = MessagingUtils.GetProbeTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(probeTopic))
                opts.Topics.Add(probeTopic);

            return new TargetedReceiverRepository(CoreConstants.SUBSYSTEM_AGENT_WORKER, targetSession, opts);
        }

        private string GetTargetSession(string[] args)
        {
            return AbstractRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_TARGET_SESSION);
        }

        internal virtual MessageReceiverOptions GetBaseOptions(string[] args)
        {
            var cfgPathArg = AbstractRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_CONFIG_PATH);
            _logger.Debug($"Config path from argumants: [{cfgPathArg}]");

            var opts = TargetedReceiverRepository.GetOptionsByPath(CoreConstants.SUBSYSTEM_AGENT_WORKER, cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            _logger.Debug($"Communicator options: [{opts}]");
            return opts;
        }
    }
}
