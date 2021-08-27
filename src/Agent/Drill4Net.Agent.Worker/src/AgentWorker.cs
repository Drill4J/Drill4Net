using System;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    public class AgentWorker : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsTargetReceived { get; private set; }

        private readonly ITargetInfoReceiver _targetReceiver;
        private readonly IProbeReceiver _probeReceiver;

        private readonly string _logPrefix;

        /*******************************************************************************/

        public AgentWorker(ITargetInfoReceiver targetReceiver, IProbeReceiver probeReceiver)
        {
            _logPrefix = MessagingUtils.GetLogPrefix(CoreConstants.SUBSYSTEM_AGENT_WORKER, typeof(AgentWorker));

            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _probeReceiver = probeReceiver ?? throw new ArgumentNullException(nameof(probeReceiver));

            _targetReceiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += Receiver_ErrorOccured;

            _probeReceiver.ProbeReceived += Receiver_ProbeReceived;
            _probeReceiver.ErrorOccured += Receiver_ErrorOccured;
        }

        /*******************************************************************************/

        public void Start()
        {
            _targetReceiver.Start();
        }

        public void Stop()
        {
            _targetReceiver.TargetInfoReceived -= Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= Receiver_ErrorOccured;
            _targetReceiver.Stop();

            _probeReceiver.ProbeReceived -= Receiver_ProbeReceived;
            _probeReceiver.ErrorOccured -= Receiver_ErrorOccured;
            _probeReceiver.Stop();
        }

        private void Receiver_TargetInfoReceived(TargetInfo target)
        {
            Console.WriteLine($"{_logPrefix}{nameof(TargetInfo)} has received");

            IsTargetReceived = true;
            _targetReceiver.Stop();

            StandardAgentCCtorParameters.SkipCctor = true;
            StandardAgent.Init(target.Options, target.Solution);
            Console.WriteLine($"{_logPrefix}{nameof(StandardAgent)} has initialized");

            Console.WriteLine($"{_logPrefix}{nameof(StandardAgent)} has started to receive probes...");
            _probeReceiver.Start();
        }

        private void Receiver_ProbeReceived(Probe probe)
        {
            //Log.Debug("Message: {Message}", message); //TODO: option from cfg (true only for RnD/debug/small projects, etc)
            StandardAgent.RegisterStatic(probe.Data, probe.Context);
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
