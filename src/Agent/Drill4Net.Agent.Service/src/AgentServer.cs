using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    /// <summary>
    /// Agent server which processes target info and its probes' data
    /// by special agent workers.
    /// </summary>
    public class AgentServer : IMessageReceiver, IDisposable
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsStarted { get; private set; }

        private readonly AbstractAgentServerRepository _rep;

        private readonly IPingReceiver _pingReceiver;
        private readonly ITargetInfoReceiver _targetReceiver;

        private readonly ConcurrentDictionary<Guid, StringDictionary> _pings;
        private readonly ConcurrentDictionary<Guid, WorkerInfo> _workers;

        private const long _oldPingTickDelta = 7 * 10_000_000; //n sec

        private readonly AbstractTransportAdmin _admin;
        private readonly Logger _logger;

        private AgentServerDebugOptions _debugOpts;
        private bool _isDebug;

        private Timer _timeoutTimer;
        private bool _inPingCheck;
        private readonly string _cfgPath;
        private readonly string _workerDir;
        private readonly string _processName;
        private bool _disposed;

        /*****************************************************************************************************/

        /// <summary>
        /// Create the Agent server which processes target info and its probes' data
        /// by special agent workers.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="targetReceiver"></param>
        /// <param name="pingReceiver"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AgentServer(AbstractAgentServerRepository rep, ITargetInfoReceiver targetReceiver,
            IPingReceiver pingReceiver)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<AgentServer>(rep.Subsystem);
            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _pingReceiver = pingReceiver ?? throw new ArgumentNullException(nameof(pingReceiver));
            _pings = new ConcurrentDictionary<Guid, StringDictionary>();
            _workers = new ConcurrentDictionary<Guid, WorkerInfo>();
            _admin = _rep.GetTransportAdmin();

            _debugOpts = _rep?.Options?.Debug;
            _isDebug = _debugOpts?.Disabled == false;

            _processName = FileUtils.GetFullPath(_rep.Options.WorkerPath, FileUtils.ExecutingDir);
            _workerDir = Path.GetDirectoryName(_processName);

            //for using by workers
            _cfgPath = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_SERVICE_NAME);

            //events
            _pingReceiver.PingReceived += PingReceiver_PingReceived;

            _targetReceiver.TargetInfoReceived += TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += TargetReceiver_ErrorOccured;

            _logger.Debug("Created.");
        }

        ~AgentServer()
        {
            Dispose(false);
        }

        /*****************************************************************************************************/

        public void Start()
        {
            if (_disposed)
                throw new Exception($"Object of {nameof(AgentServer)} is disposed");
            //
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
            _timeoutTimer?.Dispose();

            _pingReceiver.Stop();
            _pingReceiver.PingReceived -= PingReceiver_PingReceived;

            _targetReceiver.Stop();
            _targetReceiver.TargetInfoReceived -= TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured -= TargetReceiver_ErrorOccured;

            CheckWorkers();
            IsStarted = false;
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

            Console.WriteLine($"{subsystem}|{targetSession}|{DateTime.FromBinary(ticks)}");

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
            catch(Exception ex)
            {
                _logger.Error("Error for the closing the workers", ex);
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
            foreach (var uid in _pings.Keys.AsParallel())
                CheckWorker(uid);
        }

        private void CheckWorker(Guid uid)
        {
            var now = GetTime();
            if (!_pings.TryGetValue(uid, out StringDictionary data))
                return;
            var ticks = long.Parse(data[MessagingConstants.PING_TIME]);
            if (now.Ticks - ticks < _oldPingTickDelta)
                return;
            if (!_workers.TryGetValue(uid, out WorkerInfo worker))
                return;
            //
            _logger.Info($"Closing worker: {uid} -> {data[MessagingConstants.PING_TARGET_NAME]}");
            Task.Run(() => CloseWorker(uid));
            Task.Run(() => DeleteTopic(worker.TargetInfoTopic));
            Task.Run(() => DeleteTopic(worker.ProbeTopic));
            Task.Run(() => DeleteTopic(worker.CommandTopic));
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

        internal void DeleteTopic(string topic)
        {
            _admin.DeleteTopics(_rep.Options.Servers, new List<string> { topic });
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
            //Target session
            var sessionUid = target.SessionUid;
            if (_workers.ContainsKey(sessionUid))
                return;

            //topics
            string trgTopic = MessagingUtils.GetTargetWorkerTopic(sessionUid);
            var probeTopic = MessagingUtils.GetProbeTopic(sessionUid);
            var cmdTopic = MessagingUtils.GetCommandToWorkerTopic(sessionUid);

            int pid = -1;
            var needStartWorker = !_isDebug || _debugOpts?.DontStartWorker != true;
            if (needStartWorker)
            {
                pid = StartAgentWorkerProcess(target.SessionUid);
                _logger.Info($"Worker was started with pid={pid} -> {trgTopic} : {probeTopic}");

                //add local worker info
                var worker = new WorkerInfo(target, trgTopic, probeTopic, cmdTopic, pid);
                if (!_workers.TryAdd(target.SessionUid, worker))
                    return;
            }
            else
            {
                _logger.Info("Worker was not started due to Agent Server's debug options");
            }

            //send to worker the target's info
            SendTargetInfoToAgentWorker(trgTopic, target);
            _logger.Debug($"Target info was sent to the topic={trgTopic} for the Worker with pid={pid}");
        }

        internal int StartAgentWorkerProcess(Guid targetSession)
        {
            var args = $"-{MessagingTransportConstants.ARGUMENT_CONFIG_PATH}={_cfgPath} -{MessagingTransportConstants.ARGUMENT_TARGET_SESSION}={targetSession}";
            _logger.Debug($"Agent Worker's argument: [{args}]");

            var process = new Process
            {
                StartInfo =
                {
                    FileName = _processName,
                    Arguments = args,
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
            var senderOpts = new MessagerOptions() { Sender = new() };
            senderOpts.Servers.AddRange(_rep.Options.Servers); //for sending we use the same server options
            senderOpts.Sender.Topics.Add(topic);

            ITargetedInfoSenderRepository trgRep = new TargetedInfoSenderRepository(target, senderOpts);
            using ITargetInfoSender sender = new TargetInfoKafkaSender(trgRep);
            sender.SendTargetInfo(trgRep.GetTargetInfo(), topic); //here is exclusive topic for the Worker
        }

        private void TargetReceiver_ErrorOccured(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            //TODO: log
            ErrorOccured?.Invoke(source, isFatal, isLocal, message);
        }
        #endregion

        internal virtual DateTime GetTime()
        {
            return DateTime.UtcNow;
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Stop();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //.....

                // Note disposing has been done.
                _disposed = true;
            }
        }
        #endregion
    }
}
