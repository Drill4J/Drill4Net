using System;
using System.Diagnostics;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public class CoverageServer : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly AbstractRepository<MessageReceiverOptions> _rep;
        private readonly ITargetInfoReceiver _targetReceiver;

        /******************************************************************/

        public CoverageServer(AbstractRepository<MessageReceiverOptions> rep, ITargetInfoReceiver receiver)
        {
            _targetReceiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));

            _targetReceiver.TargetInfoReceived += Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += Receiver_ErrorOccured;
        }

        /******************************************************************/

        public void Start()
        {
            _targetReceiver.Start();
        }

        public void Stop()
        {
            _targetReceiver.Stop();

            _targetReceiver.TargetInfoReceived -= Receiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= Receiver_ErrorOccured;
        }

        /// <summary>
        /// Receive the target information from Target.
        /// </summary>
        /// <param name="target">The target.</param>
        private void Receiver_TargetInfoReceived(TargetInfo target)
        {
            //start the Worker

            //TODO: to cfg
            var workerDir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Kafka.Worker\net5.0\";
            var processName = Path.Combine(workerDir, "Drill4Net.Agent.Kafka.Worker.exe");

            var dir = FileUtils.GetExecutionDir();
            var cfgArg = Path.Combine(dir, CoreConstants.CONFIG_SERVICE_NAME);
            var topic = $"worker_{Guid.NewGuid()}";
            var process = new Process
            {
                StartInfo =
                {
                    FileName = processName,
                    Arguments = $"{KafkaTransportConstants.ARGUMENT_CONFIG_PATH}={cfgArg} {KafkaTransportConstants.ARGUMENT_TARGET_TOPIC}={topic}",
                    WorkingDirectory = workerDir,
                    CreateNoWindow = false, //true for real using
                    UseShellExecute = true, //false for real using
                }
            };
            process.Start();
            var pid = process.Id;

            //send to worker the Target info by the exclusive topic
            //TODO: from header of incoming messages of Target info
            var targetName = "xyz";

            //for sending we use the same server options
            var recOpts = _rep.Options;
            var senderOpts = new MessageSenderOptions();
            senderOpts.Servers.AddRange(recOpts.Servers);
            senderOpts.Topics.AddRange(recOpts.Topics);
            senderOpts.Topics.Add(topic);

            IMessageSenderRepository rep = new ServerSenderRepository(targetName, CoreConstants.SUBSYSTEM_TRANSMITTER, target, senderOpts);
            IDataSender sender = new TargetDataSender(rep);
            sender.SendTargetInfo(rep.GetTargetInfo());
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log

            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
