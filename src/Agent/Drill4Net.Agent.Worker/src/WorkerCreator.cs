using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;
using Drill4Net.Core.Repository;

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
            IProbeReceiver probeReceiver = new ProbeKafkaReceiver(rep);
            ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<MessageReceiverOptions>(rep);
            var worker = new AgentWorker(targetReceiver, probeReceiver);
            return worker;
        }

        internal virtual AbstractRepository<MessageReceiverOptions> GetRepository()
        {
            var opts = GetBaseOptions(_args);
            var targetSession = GetTargetSession(_args);
            Console.WriteLine($"Worker session = {targetSession}");

            var targetTopic = MessagingUtils.GetTargetWorkerTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(targetTopic))
                opts.Topics.Add(targetTopic);

            var probeTopic = MessagingUtils.GetProbeTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(probeTopic))
                opts.Topics.Add(probeTopic);

            return new AgentWorkerRepository(CoreConstants.SUBSYSTEM_AGENT_WORKER, opts);
        }

        private string GetTargetSession(string[] args)
        {
            return AgentWorkerRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_TARGET_SESSION);
        }

        internal virtual MessageReceiverOptions GetBaseOptions(string[] args)
        {
            var cfgPathArg = AgentWorkerRepository.GetArgument(args, MessagingTransportConstants.ARGUMENT_CONFIG_PATH);
            var opts = AgentWorkerRepository.GetOptionsByPath(cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            return opts;
        }
    }
}
