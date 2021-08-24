using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.Agent.Messaging.Transport;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Drill4Net.Agent.Service
{
    public class AgentServer : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        private readonly AbstractRepository<MessageReceiverOptions> _rep;
        private readonly IPingReceiver _pingReceiver;
        private readonly ITargetInfoReceiver _targetReceiver;

        private readonly ConcurrentDictionary<Guid, WorkerInfo> _workers;

        private readonly string _logPrefix;

        /*****************************************************************************************************/

        public AgentServer(AbstractRepository<MessageReceiverOptions> rep, ITargetInfoReceiver targetReceiver,
            IPingReceiver pingReceiver)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _pingReceiver = pingReceiver ?? throw new ArgumentNullException(nameof(pingReceiver));
            _workers = new ConcurrentDictionary<Guid, WorkerInfo>();
            _logPrefix = TransportUtils.GetLogPrefix(rep.Subsystem, typeof(AgentServer));

            _pingReceiver.PingReceived += PingReceiver_PingReceived;

            _targetReceiver.TargetInfoReceived += TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += TargetReceiver_ErrorOccured;
        }

        /*****************************************************************************************************/

        public void Start()
        {
            var tasks = new List<Task>
            {
               Task.Run(_pingReceiver.Start),
               Task.Run(_targetReceiver.Start),
            };
            Task.WaitAll(tasks.ToArray());
        }

        public void Stop()
        {
            _targetReceiver.Stop();

            _targetReceiver.TargetInfoReceived -= TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= TargetReceiver_ErrorOccured;
        }

        private void PingReceiver_PingReceived(string targetSession, StringDictionary data)
        {

        }

        /// <summary>
        /// Receive the target information from Target.
        /// </summary>
        /// <param name="target">The target.</param>
        private void TargetReceiver_TargetInfoReceived(TargetInfo target)
        {
            if (_workers.ContainsKey(target.SessionUid))
                return;

            //start the Worker

            //TODO: to cfg
            var workerDir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Worker\net5.0\";
            var processName = Path.Combine(workerDir, "Drill4Net.Agent.Worker.exe");

            var dir = FileUtils.GetExecutionDir();
            var cfgArg = Path.Combine(dir, CoreConstants.CONFIG_SERVICE_NAME);
            var topic = TransportUtils.GetTopicBySessionId(target.SessionUid);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = processName,
                    Arguments = $"{MessagingTransportConstants.ARGUMENT_CONFIG_PATH}={cfgArg} {MessagingTransportConstants.ARGUMENT_TARGET_TOPIC}={topic}",
                    WorkingDirectory = workerDir,
                    CreateNoWindow = false, //true for real using
                    //UseShellExecute = true, //false for real using
                }
            };
            process.Start();

            var pid = process.Id;
            Console.WriteLine($"{_logPrefix}Worker was started with pid={pid} and topic={topic}");

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

            ITargetSenderRepository rep = new ServerSenderRepository(targetName, target, senderOpts);
            ITargetInfoSender sender = new TargetInfoKafkaSender(rep);
            sender.SendTargetInfo(rep.GetTargetInfo(), topic); //here exclusive topic for the Worker

            Console.WriteLine($"{_logPrefix}Target info was sent to the Worker with pid={pid} and topic={topic}");
        }

        private void TargetReceiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log

            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
