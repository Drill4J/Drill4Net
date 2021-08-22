using System;
using System.Linq;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public class CoverageWorker : IProbeReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;
        public event ProbeReceivedHandler ProbeReceived;

        public bool IsTargetReceived { get; private set; }

        private readonly ITargetInfoReceiver _targetReceiver;
        private readonly IProbeReceiver _probeReceiver;

        /*******************************************************************************/

        public CoverageWorker(ITargetInfoReceiver targetReceiver, IProbeReceiver probeReceiver)
        {
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
            Console.WriteLine($"{nameof(TargetInfo)} has received");

            IsTargetReceived = true;
            _targetReceiver.Stop();

            StandardAgent.Init(target.Options, target.Solution);
            Console.WriteLine($"{nameof(StandardAgent)} has initialized");

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
