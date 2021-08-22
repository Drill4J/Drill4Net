using System;
using System.Diagnostics;
using System.IO;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public class CoverageServer : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly ITargetInfoReceiver _receiver;

        /******************************************************************/

        public CoverageServer(ITargetInfoReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));

            _receiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _receiver.ErrorOccured += Receiver_ErrorOccured;
        }

        /******************************************************************/

        public void Start()
        {
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
            //var dir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Kafka.Worker\net5.0\";
            //var processName = Path.Combine(dir, "Drill4Net.Agent.Kafka.Worker.exe");
            //var targetArg = TargetInfoArgumentSerializer.Serialize(target);
            //var process = new Process
            //{
            //    StartInfo =
            //    {
            //        FileName = processName,
            //        Arguments = $"{KafkaTransportConstants.ARGUMENT_TARGET_INFO}={targetArg}",
            //        WorkingDirectory = dir,
            //        //CreateNoWindow = true,
            //        //UseShellExecute = 
            //    }
            //};
            //process.Start();
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log

            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
