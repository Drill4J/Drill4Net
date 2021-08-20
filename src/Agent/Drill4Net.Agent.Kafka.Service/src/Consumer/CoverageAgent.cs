using System;
using System.Linq;
using Serilog;
using Drill4Net.Agent.Kafka.Common;
using System.Collections.Concurrent;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Kafka.Service
{
    public class CoverageAgent
    {
        private readonly IProbeReceiver _receiver;
        private readonly ConcurrentDictionary<string, AbstractAgent> _agents;

        /***************************************************************************/

        public CoverageAgent(IProbeReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            receiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            receiver.ProbeReceived += Receiver_ProbeReceived;
            receiver.ErrorOccured += Receiver_ErrorOccured;

            _agents = new ConcurrentDictionary<string, AbstractAgent>();
            StandardAgent.Init();
        }

        /***************************************************************************/

        public void Start()
        {
            _receiver.Start();
        }

        public void Stop()
        {
            _receiver.Stop();
        }

        private void Receiver_TargetInfoReceived(TargetInfo target)
        {

        }

        private void Receiver_ProbeReceived(Probe probe)
        {
            // Log.Debug("Message: {Message}", message); //TODO: option from cfg (true only for RnD/debug/small projects, etc)
            StandardAgent.RegisterStatic(message);
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
