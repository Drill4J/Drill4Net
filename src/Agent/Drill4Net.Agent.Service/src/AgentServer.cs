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
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class AgentServer : IMessageReceiver, IDisposable
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsStarted { get; private set; }

        private readonly AbstractAgentServerRepository _rep;

        private readonly IPingReceiver _pingReceiver;
        private readonly ITargetInfoReceiver _targetReceiver;

        private readonly ConcurrentDictionary<Guid, StringDictionary> _pings;
        private readonly ConcurrentDictionary<Guid, WorkerInfo> _workers;

        private const long _oldPingTickDelta = 50000000; //3 sec

        private Timer _timeoutTimer;
        private bool _inPingCheck;
        private readonly string _cfgPath;
        private readonly string _logPrefix;
        private readonly string _workerDir;
        private readonly string _processName;
        private bool _disposed;

        /*****************************************************************************************************/

        public AgentServer(AbstractAgentServerRepository rep, ITargetInfoReceiver targetReceiver,
            IPingReceiver pingReceiver)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _targetReceiver = targetReceiver ?? throw new ArgumentNullException(nameof(targetReceiver));
            _pingReceiver = pingReceiver ?? throw new ArgumentNullException(nameof(pingReceiver));
            _workers = new ConcurrentDictionary<Guid, WorkerInfo>();
            _pings = new ConcurrentDictionary<Guid, StringDictionary>();
            _logPrefix = MessagingUtils.GetLogPrefix(rep.Subsystem, typeof(AgentServer));

            _processName = FileUtils.GetFullPath(_rep.Options.WorkerPath, FileUtils.GetExecutionDir());
            _workerDir = Path.GetDirectoryName(_processName);

            //for using by workers
            var dir = FileUtils.GetExecutionDir();
            _cfgPath = Path.Combine(dir, CoreConstants.CONFIG_SERVICE_NAME);

            //events
            _pingReceiver.PingReceived += PingReceiver_PingReceived;

            _targetReceiver.TargetInfoReceived += TargetReceiver_TargetInfoReceived;
            _targetReceiver.ErrorOccured += TargetReceiver_ErrorOccured;
        }

        ~AgentServer()
        {
            Dispose(false);
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
            foreach (var uid in _pings.Keys.AsParallel())
            {
                var data = _pings[uid];
                var ticks = long.Parse(data[MessagingConstants.PING_TIME]);
                if (now.Ticks - ticks < _oldPingTickDelta)
                    continue;
                //
                Console.WriteLine($"{_logPrefix}Closing worker: {uid} -> {data[MessagingConstants.PING_TARGET_NAME]}");
                if (!_workers.TryGetValue(uid, out WorkerInfo worker))
                    continue;
                Task.Run(() => CloseWorker(uid));
                Task.Run(() => DeleteTopic(worker.TargetInfoTopic));
                Task.Run(() => DeleteTopic(worker.ProbeTopic));
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

        internal void DeleteTopic(string topic)
        {
            var admin = _rep.GetTransportAdmin();
            admin.DeleteTopics(_rep.Options.Servers, new List<string> { topic });
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
            var trgTopic = MessagingUtils.GetTargetWorkerTopic(sessionUid.ToString());
            var probeTopic = MessagingUtils.GetProbeTopic(sessionUid.ToString());
            var pid = StartAgentWorkerProcess(target.SessionUid);
            Console.WriteLine($"{_logPrefix}Worker was started with pid={pid} -> {trgTopic} : {probeTopic}");

            //add local worker info
            var worker = new WorkerInfo(target, trgTopic, probeTopic, pid);
            if (!_workers.TryAdd(target.SessionUid, worker))
                return;

            //send to worker the info
            SendTargetInfoToAgentWorker(trgTopic, target);
            Console.WriteLine($"{_logPrefix}Target info was sent to the Worker with pid={pid} and topic={trgTopic}");
        }

        internal int StartAgentWorkerProcess(Guid targetSession)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = _processName,
                    Arguments = $"{MessagingTransportConstants.ARGUMENT_CONFIG_PATH}={_cfgPath} {MessagingTransportConstants.ARGUMENT_TARGET_SESSION}={targetSession}",
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
