using System;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

namespace Drill4Net.Agent.Worker
{
    public class WorkerCreator
    {
        private readonly CliDescriptor _cliDescriptor;
        private readonly Logger _logger;

        /**************************************************************************/

        public WorkerCreator(string[] appArgs)
        {
            if (appArgs == null)
                throw new ArgumentNullException(nameof(appArgs));
            _cliDescriptor = new CliDescriptor(appArgs, false);
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
            ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<MessagerOptions>(rep, false);

            //senders
            ICommandSender cmdSender = CreateCommandSender(rep);

            //worker
            return new AgentWorker(rep, targetReceiver, probeReceiver, cmdReceiver, cmdSender);
        }

        internal virtual TargetedReceiverRepository GetRepository()
        {
            #region Get options
            var (cfgPath, opts) = GetBaseOptions();
            var targetSession = _cliDescriptor.GetParameter(MessagingTransportConstants.ARGUMENT_TARGET_SESSION);
            var targetName = _cliDescriptor.GetParameter(MessagingTransportConstants.ARGUMENT_TARGET_NAME);
            var targetVersion = _cliDescriptor.GetParameter(MessagingTransportConstants.ARGUMENT_TARGET_VERSION);
            _logger.Info($"Parameters: session={targetSession};name={targetName};version={targetVersion}");

            if (opts.Sender == null)
                opts.Sender = new();
            if (opts.Sender.Topics == null)
                opts.Sender.Topics = new();
            if (opts.Receiver == null)
                throw new Exception("Receiver is empty");
            #endregion

            //some topics are located together in the Topics property of options //

            //Receivers
            var targetTopic = MessagingUtils.GetTargetWorkerTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(targetTopic))
                opts.Receiver.Topics.Add(targetTopic);

            var cmdForWorkerTopic = MessagingUtils.GetCommandToWorkerTopic(targetSession); //get the topic for the commands to this Worker
            if (!string.IsNullOrWhiteSpace(cmdForWorkerTopic))
                opts.Receiver.Topics.Add(cmdForWorkerTopic);

            var probeTopic = MessagingUtils.GetProbeTopic(targetSession);
            if (!string.IsNullOrWhiteSpace(probeTopic))
                opts.Receiver.Topics.Add(probeTopic);

            //Senders
            var cmdForTransTopic = MessagingUtils.GetCommandToTransmitterTopic(targetSession); //get the topic for the commands to the Transmitter
            if (!string.IsNullOrWhiteSpace(cmdForTransTopic))
                opts.Sender.Topics.Add(cmdForTransTopic);

            return new TargetedReceiverRepository(CoreConstants.SUBSYSTEM_AGENT_WORKER, targetSession, targetName, targetVersion, opts, cfgPath);
        }

        private ICommandSender CreateCommandSender(ITargetedRepository rep)
        {
            _logger.Debug("Creating command sender...");
            IMessagerRepository targRep = new TargetedSenderRepository(rep.Subsystem, rep.TargetSession, rep.TargetName,
                rep.TargetVersion, rep.ConfigPath); //need read own config
            if (targRep.MessagerOptions.Receiver == null)
                throw new Exception("Receiver is empty");

            var topic = MessagingUtils.GetCommandToTransmitterTopic(rep.TargetSession);
            _logger.Debug($"Command sender topic is {topic}");
            (targRep.MessagerOptions.Receiver.Topics ??= new()).Add(topic);

            _logger.Debug("Command sender is created.");
            Log.Flush();

            return new CommandKafkaSender(targRep);
        }

        internal virtual (string cfgPath, MessagerOptions opts) GetBaseOptions()
        {
            var cfgPathArg = _cliDescriptor.GetParameter(MessagingTransportConstants.ARGUMENT_CONFIG_PATH);
            _logger.Debug($"Config path from arguments: [{cfgPathArg}]");
            if (cfgPathArg == null)
                throw new InvalidOperationException("The path to the config file is not obtained from the command line arguments.");

            var opts = TargetedReceiverRepository.GetOptionsByPath(CoreConstants.SUBSYSTEM_AGENT_WORKER, cfgPathArg);
            if (opts == null)
                throw new Exception("Communicator options hasn't retrieved");
            _logger.Debug($"Communicator options: [{opts}]");
            return (cfgPathArg, opts);
        }
    }
}
