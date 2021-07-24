using System;
using System.Linq;
using Serilog;
using Drill4Net.Agent.Standard;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class CoverageAgent
    {
        private readonly IProbeReceiver _receiver;

        /***************************************************************************/

        public CoverageAgent(IProbeReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            receiver.MessageReceived += Receiver_MessageReceived;
            receiver.ErrorOccured += Receiver_ErrorOccured;

            StandardAgent.Init();
        }

        /***************************************************************************/

        public void Start()
        {
            _receiver.Start();
        }

        private void Receiver_MessageReceived(string message)
        {
            Log.Debug("Message: {Message}", message);
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
