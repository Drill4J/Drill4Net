using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    /// <summary>
    /// Worker for processing target probes' data, converting them and sending to the Drill admin side
    /// by <see cref="StandardAgent"/>, sending commands to the Transmitter in the target.
    /// </summary>
    public class AgentWorker : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsTargetReceived { get; private set; }

        public bool IsStarted { get; private set; }

        public bool IsAgentInitialized { get; private set; }

        private bool _isAgentInitStarted;
        private readonly TargetedReceiverRepository _rep;
        private readonly ITargetInfoReceiver _targetReceiver;
        private readonly IProbeReceiver _probeReceiver;
        private readonly ICommandReceiver _cmdReceiver;
        public ICommandSender _cmdSender;

        private readonly IEnumerable<string> _cmdToTransTopics;
        private readonly Logger _logger;

        /********************************************************************************************/

        public AgentWorker(TargetedReceiverRepository rep, ITargetInfoReceiver targetReceiver,
            IProbeReceiver probeReceiver, ICommandReceiver cmdReceiver, ICommandSender cmdSender)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));

            var extrasData = new Dictionary<string, object> { { "TargetSession", _rep.TargetSession } };
            _logger = new TypedLogger<AgentWorker>(_rep.Subsystem, extrasData);

            _logger.Debug($"Worker is initializing for target session: {_rep.TargetSession}");

            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _probeReceiver = probeReceiver ?? throw new ArgumentNullException(nameof(probeReceiver));
            _cmdReceiver = cmdReceiver ?? throw new ArgumentNullException(nameof(cmdReceiver));

            _cmdSender = cmdSender ?? throw new ArgumentNullException(nameof(cmdSender));

            _targetReceiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += Receiver_ErrorOccured;

            _probeReceiver.ProbeReceived += Receiver_ProbeReceived;
            _probeReceiver.ErrorOccured += Receiver_ErrorOccured;

            _cmdReceiver.CommandReceived += Receiver_CommandReceived;
            _cmdReceiver.ErrorOccured += Receiver_ErrorOccured;

            _cmdToTransTopics = GetCommandToTransmitterTopics();
            _logger.Debug($"Topics for commands to Transmitter: {string.Join(", ", _cmdToTransTopics)}");

            _logger.Debug($"{nameof(AgentWorker)} is created");
        }

        /********************************************************************************************/

        public void Start()
        {
            if (IsStarted)
                return;
            IsStarted = true;
            _logger.Info("Worker is starting");

            _targetReceiver.Start();
        }

        public void Stop()
        {
            if (!IsStarted)
                return;
            IsStarted = false;
            _logger.Info("Worker stopping");

            _targetReceiver.TargetInfoReceived -= Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= Receiver_ErrorOccured;
            _targetReceiver.Stop();

            _probeReceiver.ProbeReceived -= Receiver_ProbeReceived;
            _probeReceiver.ErrorOccured -= Receiver_ErrorOccured;
            _probeReceiver.Stop();

            _cmdReceiver.CommandReceived -= Receiver_CommandReceived;
            _cmdReceiver.ErrorOccured -= Receiver_ErrorOccured;
            _cmdReceiver.Stop();
        }

        private void Receiver_CommandReceived(Command command)
        {
            _logger.Info(command.ToString());

            if (!_isAgentInitStarted)
            {
                _logger.Warning($"Command [{command.Type}] is received, but the Agent is not initialized");
                return;
            }

            //TODO: non-Agent commands, so to call it only for the Agent's commands
            StandardAgent.DoCommand(command.Type, command.Data);
        }

        private void Receiver_TargetInfoReceived(TargetInfo info)
        {
            _logger.Info($"{nameof(TargetInfo)} is received");

            IsTargetReceived = true;
            _targetReceiver.Stop();

            InitAgent(info);

            _logger.Info($"{nameof(AgentWorker)} starts receiving the commands...");
            Task.Run(_cmdReceiver.Start);

            _logger.Info($"{nameof(AgentWorker)} starts receiving the probes...");
            _probeReceiver.Start();
        }

        private void InitAgent(TargetInfo info)
        {
            if (_isAgentInitStarted)
                return;

            AgentInitParameters.SkipCreatingSingleton = true;
            AgentInitParameters.LocatedInWorker = true;
            AgentInitParameters.TargetDir = info.TargetDir;
            AgentInitParameters.TargetVersion = info.TargetVersion;

            StandardAgent.Init(info.Options, info.Tree);
            StandardAgent.Agent.Initialized += AgentInitialized;

            _isAgentInitStarted = true;
            _logger.Debug($"{nameof(StandardAgent)} is primary initialized");
        }

        private void AgentInitialized()
        {
            IsAgentInitialized = true;
            _logger.Info("Sending the command to Transmitter to continue executing");

            //Send message "Can start to execute the probes" to the Target
            _cmdSender.SendCommand((int)AgentCommandType.TRANSMITTER_CAN_CONTINUE, null, _cmdToTransTopics);
        }

        private IEnumerable<string> GetCommandToTransmitterTopics()
        {
            return MessagingUtils.FilterCommandTopics(_rep.Options.Sender.Topics);
        }

        private void Receiver_ProbeReceived(Probe probe)
        {
            if (probe == null)
                return; //??

            //_logger.Debug("Message: {Message}", message); //TODO: option from cfg (true only for RnD/debug/small projects, etc)
            StandardAgent.RegisterWithContextStatic(probe.Data, probe.Context);
        }

        private void Receiver_ErrorOccured(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(source, isFatal, isLocal, message);
        }
    }
}