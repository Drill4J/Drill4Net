using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public class ProbeServer : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly AbstractRepository<MessageReceiverOptions> _rep;
        private readonly ITargetInfoReceiver _targetReceiver;

        private readonly ConcurrentDictionary<Guid, WorkerInfo> _workers;

        private readonly string _logPrefix;

        /******************************************************************/

        public ProbeServer(AbstractRepository<MessageReceiverOptions> rep, ITargetInfoReceiver receiver)
        {
            _targetReceiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _workers = new ConcurrentDictionary<Guid, WorkerInfo>();

            _logPrefix = TransportUtils.GetLogPrefix(rep.Subsystem, typeof(ProbeServer));

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
            if (_workers.ContainsKey(target.SessionUid))
                return;

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
                    //UseShellExecute = true, //false for real using
                }
            };
            process.Start();
            var pid = process.Id;

            Console.WriteLine($"{_logPrefix}Has started the Worker with pid={pid} and topic={topic}");

            //worker info
            var worker = new WorkerInfo(target, pid);
            _workers.TryAdd(target.SessionUid, worker);

            //send to worker the Target info by the exclusive topic
            //TODO: from header of incoming messages of Target info
            var targetName = "xyz";

            //for sending we use the same server options
            var recOpts = _rep.Options;
            var senderOpts = new MessageSenderOptions();
            senderOpts.Servers.AddRange(recOpts.Servers);
            senderOpts.Topics.Add(topic);

            IMessageSenderRepository rep = new ServerSenderRepository(targetName, target, senderOpts);
            IDataSender sender = new TargetDataSender(rep);
            sender.SendTargetInfo(rep.GetTargetInfo());

            Console.WriteLine($"{_logPrefix}Has sent Target info to the Worker with pid={pid} and topic={topic}");
        }

        private void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log

            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
