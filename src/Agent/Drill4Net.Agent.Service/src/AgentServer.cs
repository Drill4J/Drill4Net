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
        private readonly string _cfgPath;
        private readonly string _logPrefix;
        private readonly string _workerDir;
        private readonly string _processName;

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

            //TODO: to cfg
            _workerDir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Worker\net5.0\";
            _processName = Path.Combine(_workerDir, "Drill4Net.Agent.Worker.exe");

            var dir = FileUtils.GetExecutionDir();
            _cfgPath = Path.Combine(dir, CoreConstants.CONFIG_SERVICE_NAME);

            //events
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
            if (now.Ticks - ticks > _oldPingTickDelta) //too old data
                return;
            //
            var subsystem = data[MessagingConstants.PING_SUBSYSTEM];
            //switch(subsystem) //TODO
            //{ 
            //}

            Console.WriteLine($"{subsystem} [{targetSession}]: {DateTime.FromBinary(ticks)}");

            //update data
            _pings.AddOrUpdate(session, data, (key, oldValue) => data);
        }

        #region CheckWorkers
        /// <summary>
        /// Pings the 'check callback' for the unnecessary workers,
        /// whose targets are no longer being worked out.
        /// </summary>
        /// <param name="state">The state.</param>
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

        /// <summary>
        /// Checks the unnecessary workers, whose targets are no longer being worked out.
        /// </summary>
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

        /// <summary>
        /// Closing already unnecessary workers, whose targets are no longer being worked out.
        /// </summary>
        /// <param name="uid">The uid.</param>
        internal void CloseWorker(Guid uid)
        {
            if (!_workers.TryRemove(uid, out WorkerInfo worker))
                return;
            _pings.TryRemove(uid, out _);

            //TODO: more gracefully with command by messaging
            try
            {
                var proc = Process.GetProcessById(worker.PID);
                proc?.Kill();
            }
            catch { }
        }
        #endregion
        #region Targets
        /// <summary>
        /// Receive the target information from Target.
        /// </summary>
        /// <param name="target">The target.</param>
        private void TargetReceiver_TargetInfoReceived(TargetInfo target)
        {
            RunAgentWorker(target);
        }

        /// <summary>
        /// Runs the agent worker.
        /// </summary>
        /// <param name="target">The target info.</param>
        internal void RunAgentWorker(TargetInfo target)
        {
            var sessionUid = target.SessionUid;
            if (_workers.ContainsKey(sessionUid))
                return;

            //start the Worker
            var topic = TransportUtils.GetTargetWorkerTopic(sessionUid);
            var pid = StartAgentWorkerProcess(topic);
            Console.WriteLine($"{_logPrefix}Worker was started with pid={pid} and topic={topic}");

            //add local worker info
            var worker = new WorkerInfo(target, pid);
            if (!_workers.TryAdd(target.SessionUid, worker))
                return;

            //send to worker the info
            SendTargetInfoToAgentWorker(topic, target);
            Console.WriteLine($"{_logPrefix}Target info was sent to the Worker with pid={pid} and topic={topic}");
        }

        internal int StartAgentWorkerProcess(string topic)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = _processName,
                    Arguments = $"{MessagingTransportConstants.ARGUMENT_CONFIG_PATH}={_cfgPath} {MessagingTransportConstants.ARGUMENT_TARGET_TOPIC}={topic}",
                    WorkingDirectory = _workerDir,
                    CreateNoWindow = false, //true for real using
                    //UseShellExecute = true, //false for real using
                }
            };
            process.Start();

            return process.Id;
        }

        internal void SendTargetInfoToAgentWorker(string topic, TargetInfo target)
        {
            //send to worker the Target info by the exclusive topic     
            var senderOpts = new MessageSenderOptions();
            senderOpts.Servers.AddRange(_rep.Options.Servers); //for sending we use the same server options
            senderOpts.Topics.Add(topic);

            ITargetSenderRepository trgRep = new TargetedSenderRepository(target, senderOpts);
            using ITargetInfoSender sender = new TargetInfoKafkaSender(trgRep);
            sender.SendTargetInfo(trgRep.GetTargetInfo(), topic); //here exclusive topic for the Worker
        }

        private void TargetReceiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            //TODO: log
            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
        #endregion

        internal virtual DateTime GetTime()
        {
            return DateTime.UtcNow;
        }

        //TODO: full pattern
        public void Dispose()
        {
            Stop();
        }
    }
}
