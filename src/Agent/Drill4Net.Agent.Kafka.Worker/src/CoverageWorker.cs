using System;
using System.Linq;
using Serilog;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public class CoverageWorker : IProbeReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly TargetInfo _target;
        private readonly IKafkaWorkerReceiver _receiver;

        /*******************************************************************************/

        public CoverageWorker(TargetInfo target, IKafkaWorkerReceiver receiver)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));

            _receiver.ProbeReceived += Receiver_ProbeReceived;
            _receiver.ErrorOccured += Receiver_ErrorOccured;

            StandardAgent.Init(target.Options, target.Solution);
        }

        /*******************************************************************************/

        public void Start()
        {
            _receiver.Start();
        }

        public void Stop()
        {
            _receiver.Stop();

            _receiver.ProbeReceived -= Receiver_ProbeReceived;
            _receiver.ErrorOccured -= Receiver_ErrorOccured;
        }

        private void Receiver_ProbeReceived(Probe probe)
        {
            //Log.Debug("Message: {Message}", message); //TODO: option from cfg (true only for RnD/debug/small projects, etc)
            StandardAgent.RegisterStatic(probe.Data, probe.Context);
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            var mess = $"Local: {isLocal} -> {message}";
            if (isFatal)
                Log.Fatal(mess);
            else
                Log.Error(mess);
        }
    }
}
