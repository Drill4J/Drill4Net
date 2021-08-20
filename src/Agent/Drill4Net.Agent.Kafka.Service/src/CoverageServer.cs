using System;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Service
{
    public class CoverageServer
    {
        private readonly IKafkaServerReceiver _receiver;

        /******************************************************************/

        public CoverageServer(IKafkaServerReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            receiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            receiver.ErrorOccured += Receiver_ErrorOccured;
        }

        /******************************************************************/

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
            throw new NotImplementedException();
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            throw new NotImplementedException();
        }
    }
}
