using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class AgentServer : IMessageReceiver, IDisposable
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsStarted { get; private set; }

        private readonly AbstractRepository<MessageReceiverOptions> _rep;

        private readonly IPingReceiver _pingReceiver;
        private readonly ITargetInfoReceiver _targetReceiver;

        private readonly ConcurrentDictionary<Guid, StringDictionary> _pings;
        private readonly ConcurrentDictionary<Guid, WorkerInfo> _workers;

        private const long _oldPingTickDelta = 30000000; //3 sec

        private Timer _timeoutTimer;
        private bool _inPingCheck;

        private readonly string _logPrefix;

        /*****************************************************************************************************/

        public AgentServer(AbstractRepository<MessageReceiverOptions> rep, ITargetInfoReceiver targetReceiver,
            IPingReceiver pingReceiver)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _pingReceiver = pingReceiver ?? throw new ArgumentNullException(nameof(pingReceiver));
            _workers = new ConcurrentDictionary<Guid, WorkerInfo>();
            _pings = new ConcurrentDictionary<Guid, StringDictionary>();
            _logPrefix = TransportUtils.GetLogPrefix(rep.Subsystem, typeof(AgentServer));

            _pingReceiver.PingReceived += PingReceiver_PingReceived;

            _targetReceiver.TargetInfoReceived += TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += TargetReceiver_ErrorOccured;
        }

        /*****************************************************************************************************/

        public void Start()
        {
            var period = new TimeSpan(0, 0, 0, 1, 500);
            _timeoutTimer = new Timer(PingCheckCallback, null, period, period);

            var tasks = new List<Task>
            {
               Task.Run(_pingReceiver.Start),
               Task.Run(_targetReceiver.Start),
            };

            IsStarted = true;
            Task.WaitAll(tasks.ToArray());
        }

        public void Stop()
        {
            _timeoutTimer.Dispose();

            _pingReceiver.Stop();
            _pingReceiver.PingReceived -= PingReceiver_PingReceived;

            _targetReceiver.Stop();
            _targetReceiver.TargetInfoReceived -= TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= TargetReceiver_ErrorOccured;

            CheckWorkers();
            IsStarted = true;
        }

        private void PingReceiver_PingReceived(string targetSession, StringDictionary data)
        {
            if (!Guid.TryParse(targetSession, out Guid session)) //log carefully
                return;
            var ticks = long.Parse(data[MessagingConstants.PING_TIME]);
            var now = GetTime();
            if (now.Ticks - ticks > _oldPingTickDelta)
                return;
            //
            var subsystem = data[MessagingConstants.PING_SUBSYSTEM];
            //switch(subsystem) //TODO
            //{ 
            //}

            Console.WriteLine($"{subsystem} [{targetSession}]: {DateTime.FromBinary(ticks)}");

            _pings.AddOrUpdate(session, data, (key, oldValue) => data);
        }

        private void PingCheckCallback(object state)
        {
            if (_inPingCheck)
                return;
            _inPingCheck = true;

            try
            {
                CheckWorkers();
            }
            catch
            {
                throw;
            }
            finally
            {
                _inPingCheck = false;
            }
        }

        internal void CheckWorkers()
        {
            if (_workers.IsEmpty || _pings.IsEmpty)
                return;
            var now = GetTime();
            foreach (var uid in _pings.Keys)
            {
                var data = _pings[uid];
                var ticks = long.Parse(data[MessagingConstants.PING_TIME]);
                if (now.Ticks - ticks < _oldPingTickDelta)
                    continue;
                Task.Run(() => CloseWorker(uid));
            }
        }

        internal void CloseWorker(Guid uid)
        {
            if (!_workers.TryRemove(uid, out WorkerInfo worker))
                return;

            //TODO: more gracefuly with command by messaging
            var pid = worker.PID;
            var proc = Process.GetProcessById(pid);
            proc?.Kill();
        }

        internal virtual DateTime GetTime()
        {
            return DateTime.UtcNow;
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
            //for sending we use the same server options
            var recOpts = _rep.Options;
            var senderOpts = new MessageSenderOptions();
            senderOpts.Servers.AddRange(recOpts.Servers);
            senderOpts.Topics.Add(topic);

            var targetName = target.TargetName;
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

        //TODO: full pattern
        public void Dispose()
        {
            Stop();
        }
    }
}
