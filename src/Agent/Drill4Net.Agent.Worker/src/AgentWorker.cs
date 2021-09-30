using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Worker
{
    public class AgentWorker : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsTargetReceived { get; private set; }

        public bool IsStarted { get; private set; }

        private readonly AgentWorkerRepository _rep;
        private readonly ITargetInfoReceiver _targetReceiver;
        private readonly IProbeReceiver _probeReceiver;
        private readonly ICommandReceiver _cmdReceiver;

        private readonly Logger _logger;

        /********************************************************************************************/

        public AgentWorker(AgentWorkerRepository rep, ITargetInfoReceiver targetReceiver, 
            IProbeReceiver probeReceiver, ICommandReceiver cmdReceiver)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));

            var extrasData = new Dictionary<string, object> { { "TargetSession", _rep.TargetSession } };
            _logger = new TypedLogger<AgentWorker>(_rep.Subsystem, extrasData);

            _logger.Debug($"Worker is initializing for target session: {_rep.TargetSession}");

            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _probeReceiver = probeReceiver ?? throw new ArgumentNullException(nameof(probeReceiver));
            _cmdReceiver = cmdReceiver ?? throw new ArgumentNullException(nameof(cmdReceiver));

            _targetReceiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += Receiver_ErrorOccured;

            _probeReceiver.ProbeReceived += Receiver_ProbeReceived;
            _probeReceiver.ErrorOccured += Receiver_ErrorOccured;

            _cmdReceiver.CommandReceived += Receiver_CommandReceived;
            _cmdReceiver.ErrorOccured += Receiver_ErrorOccured;

            _logger.Debug($"{nameof(TargetInfo)} is created");
        }

        /********************************************************************************************/

        public void Start()
        {
            if (IsStarted)
                return;
            IsStarted = true;
            _logger.Debug("Worker is starting");

            Task.Run(_cmdReceiver.Start);
            _targetReceiver.Start();
        }

        public void Stop()
        {
            if (!IsStarted)
                return;
            IsStarted = false;
            _logger.Debug("Worker stopping");

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

        private void Receiver_CommandReceived(Command probe)
        {
            _logger.Info(probe.ToString());
        }

        private void Receiver_TargetInfoReceived(TargetInfo target)
        {
            _logger.Info($"{nameof(TargetInfo)} is received");

            IsTargetReceived = true;
            _targetReceiver.Stop();

            StandardAgentCCtorParameters.SkipCctor = true;
            StandardAgent.Init(target.Options, target.Solution);
            _logger.Debug($"{nameof(StandardAgent)} is initialized");

            _logger.Info($"{nameof(AgentWorker)} starts receiving probes...");
            _probeReceiver.Start();
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