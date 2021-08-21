using System;
using System.Linq;
using System.Collections.Concurrent;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public class CoverageWorker : IProbeReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly IKafkaWorkerReceiver _receiver;
        private readonly ConcurrentDictionary<string, AbstractAgent> _agents;

        /***************************************************************************/

        public CoverageWorker(IKafkaWorkerReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));



            _agents = new ConcurrentDictionary<string, AbstractAgent>();

        }

        /***************************************************************************/

        public void Start()
        {
            _receiver.ProbeReceived += Receiver_ProbeReceived;
            _receiver.ErrorOccured += Receiver_ErrorOccured;

            StandardAgent.Init();
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
            // Log.Debug("Message: {Message}", message); //TODO: option from cfg (true only for RnD/debug/small projects, etc)
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
