using System;
using System.Linq;
using Serilog;
using Drill4Net.Agent.Standard;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class CoverageAgent
    {
        private readonly IProbeReceiver _receiver;

        /***************************************************************************/

        public CoverageAgent(IProbeReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            receiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            receiver.MessageReceived += Receiver_MessageReceived;
            receiver.ErrorOccured += Receiver_ErrorOccured;

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

        private void Receiver_MessageReceived(string message)
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
