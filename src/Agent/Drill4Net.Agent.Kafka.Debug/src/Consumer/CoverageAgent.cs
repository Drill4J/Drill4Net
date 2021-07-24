using System;
using System.Linq;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class CoverageAgent
    {
        public event ReceivedMessageHandler MessageReceived;
        public event ErrorOccuredHandler ErrorOccured;

        private readonly IProbeReceiver _receiver;

        /***************************************************************************/

        public CoverageAgent(IProbeReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            receiver.MessageReceived += Receiver_MessageReceived;
            receiver.ErrorOccured += Receiver_ErrorOccured;
        }

        /***************************************************************************/

        public void Start()
        {
            _receiver.Start();
        }

        private void Receiver_MessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
