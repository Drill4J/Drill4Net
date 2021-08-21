using System;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public class CoverageServer : IProbeReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly IKafkaServerReceiver _receiver;

        /******************************************************************/

        public CoverageServer(IKafkaServerReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        /******************************************************************/

        public void Start()
        {
            _receiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _receiver.ErrorOccured += Receiver_ErrorOccured;

            _receiver.Start();
        }

        public void Stop()
        {
            _receiver.Stop();

            _receiver.TargetInfoReceived -= Receiver_TargetInfoReceived;
            _receiver.ErrorOccured -= Receiver_ErrorOccured;
        }

        private void Receiver_TargetInfoReceived(TargetInfo target)
        {
            throw new NotImplementedException();
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log

            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
