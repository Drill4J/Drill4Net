using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Transport;
using Drill4Net.Agent.Abstract.Transfer;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
  ThisAssembly.Git.SemVer.Major + "." +
  ThisAssembly.Git.SemVer.Minor + "." +
  ThisAssembly.Git.SemVer.Patch + "-" +
  ThisAssembly.Git.Branch + "+" +
  ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Repository for Standard Agent
    /// </summary>
    public sealed class StandardAgentRepository : AbstractCommunicatorRepository
    {
        /// <summary>
        /// Any sesion is exists?
        /// </summary>
        public bool IsAnySession => _sessionToCtx.Count > 0;

        private ConcurrentDictionary<string, string> _ctxToSession;
        private ConcurrentDictionary<string, string> _sessionToCtx;
        private ConcurrentDictionary<string, StartSessionPayload> _sessionToObject;
        private ConcurrentDictionary<string, CoverageRegistrator> _ctxToRegistrator;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, ExecClassData>> _ctxToExecData;

        private CoverageRegistrator _globalRegistrator;
        private TreeConverter _converter;
        private IEnumerable<InjectedType> _injTypes;

        private Logger _logger;
        private System.Timers.Timer _sendTimer;
        private object _sendLocker;
        private bool _inTimer;

        /**************************************************************************************/

        /// <summary>
        /// Create repository for Standard Agent by options specified by the file path
        /// </summary>
        /// <param name="cfgPath"></param>
        public StandardAgentRepository(string cfgPath = null): base(cfgPath)
        {
            Init(null);
        }

        /// <summary>
        /// Create repository for Standard Agent specified the known options
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="tree"></param>
        public StandardAgentRepository(AgentOptions opts, InjectedSolution tree) : base(opts)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));
            Init(tree);
        }

        /**************************************************************************************/

        #region Init
        private void Init(InjectedSolution tree = null)
        {
            _logger = new TypedLogger<StandardAgent>(CoreConstants.SUBSYSTEM_AGENT);

            //ctx maps
            _ctxToSession = new ConcurrentDictionary<string, string>();
            _ctxToRegistrator = new ConcurrentDictionary<string, CoverageRegistrator>();

            //session maps
            _sessionToCtx = new ConcurrentDictionary<string, string>();
            _sessionToObject = new ConcurrentDictionary<string, StartSessionPayload>();

            // execution data by session ids
            _ctxToExecData = new ConcurrentDictionary<string, ConcurrentDictionary<string, ExecClassData>>();

            _converter = new TreeConverter();
            _sendLocker = new object();

            Communicator = GetCommunicator(Options.Admin, Options.Target);

            //target classes' tree
            if(tree == null)
                tree = ReadInjectedTree();
            _injTypes = GetTypesByCallerVersion(tree);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(1200);
            _sendTimer.Elapsed += Timer_Elapsed;
        }

        private AbstractCommunicator GetCommunicator(DrillServerOptions adminOpts, TargetData targetOpts)
        {
            if (adminOpts == null)
                throw new ArgumentNullException(nameof(adminOpts));
            return new Communicator(CoreConstants.SUBSYSTEM_AGENT, adminOpts.Url, GetAgentPartConfig(targetOpts));
        }

        internal AgentPartConfig GetAgentPartConfig(TargetData targOpts)
        {
            string targVersion = targOpts.Version;
            if (string.IsNullOrWhiteSpace(targVersion))
                targVersion = GetRealTargetVersion();
            return new AgentPartConfig(targOpts.Name, targVersion, GetAgentVersion());
        }

        internal string GetAgentVersion()
        {
            return FileUtils.GetProductVersion(typeof(StandardAgentRepository));
        }

        internal string GetRealTargetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }

        /// <summary>
        /// Get tool types, taking into account the current version of the target framework.
        /// </summary>
        /// <param name="tree">The tree data of injected assemblies.</param>
        /// <returns></returns>
        internal IEnumerable<InjectedType> GetTypesByCallerVersion(InjectedSolution tree)
        {
            IEnumerable<InjectedType> injTypes = null;

            // check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = tree.GetDirectories().ToList();
            if (rootDirs.Count > 1)
            {
                //TODO: refactor (optimize)!
                //investigate the versionable copies of target
                var asmNameByDirs = (from dir in rootDirs
                                     select dir.GetAssemblies()
                                               .Select(a => a.Name)
                                               .Where(a => a.EndsWith(".dll"))
                                               .ToList())
                                     .ToList();
                if (asmNameByDirs[0].Count > 0)
                {
                    var multi = true;
                    for (var i = 1; i < asmNameByDirs.Count; i++)
                    {
                        var prev = asmNameByDirs[i - 1];
                        var cur = asmNameByDirs[i];
                        //is the ALMOST same structure? It's not a strict match to compare NetFx and NetCore assembly sets
                        if (Math.Abs(prev.Count - cur.Count) < 2 && prev.Intersect(cur).Any())
                            continue;
                        multi = false;
                        break;
                    }
                    if (multi)
                    {
                        //here many copies of target for different runtimes
                        //we need only one
                        var execVer = CommonUtils.GetEntryTargetVersioning();
                        InjectedDirectory targetDir = null;
                        foreach (var dir in rootDirs)
                        {
                            var asms = dir.GetAssemblies().ToList();
                            if (asms[0].Version.Version != execVer.Version)
                                continue;
                            targetDir = dir;
                            break;
                        }
                        injTypes = targetDir?.GetAllTypes();
                    }
                }
            }

            if(injTypes == null)
                injTypes = tree.GetAllTypes();

            return injTypes.Distinct(new InjectedEntityComparer<InjectedType>());
        }

        /// <summary>
        /// Get list of <see cref="AstEntity"/> for list of registered <see cref="InjectedType"/>
        /// </summary>
        /// <returns>List of DTO entities for injected types</returns>
        public List<AstEntity> GetEntities()
        {
            return _converter.ToAstEntities(_injTypes);
        }
        #endregion
        #region Session
        #region Started        
        /// <summary>
        /// Session started on the Admin side.
        /// </summary>
        /// <param name="info">The information.</param>
        public void SessionStarted(StartAgentSession info)
        {
            var load = info.Payload;
            RemoveSession(load.SessionId);
            AddSession(load);
            StartSendCycle();
        }

        internal void AddSession(StartSessionPayload session)
        {
            var ctxId = Contexter.GetContextId(); //GUANO: it's WRONG!!! HOW DO I GET THE REAL CONTEXT FROM A TARGET?!!
            if (_ctxToSession.ContainsKey(ctxId)) //or recreate?!
                return;

            var sessionUid = session.SessionId;
            _sessionToObject.TryAdd(sessionUid, session);
            _ctxToSession.TryAdd(ctxId, sessionUid);
            _sessionToCtx.TryAdd(sessionUid, ctxId);

            if (session.IsGlobal)
                _globalRegistrator = CreateCoverageRegistrator(session);
        }
        #endregion
        #region Stop
        /// <summary>
        /// All sessions were stopped on the Admin side
        /// </summary>
        /// <returns></returns>
        public List<string> AllSessionsStopped()
        {
            StopSendCycle();
            SendCoverages();
            var uids = _sessionToCtx.Keys.ToList();
            ClearScopeData();
            return uids;
        }

        /// <summary>
        /// The session was stopped on the Admin side
        /// </summary>
        public void SessionStopped(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            
            //send remaining data
            SendCoverages();

            //removing session/data
            RemoveSession(uid);
            StopSendCycleIfNeeded();
        }
        #endregion
        #region Cancel
        /// <summary>
        /// The session was cancelled on the Admin side
        /// </summary>
        /// <param name="info"></param>
        public void SessionCancelled(CancelAgentSession info)
        {
            RemoveSession(info.Payload.SessionId);
        }

        /// <summary>
        /// All sessions were cancelled on the Admin side
        /// </summary>
        /// <param name="info"></param>
        public List<string> AllSessionsCancelled()
        {
            StopSendCycle();
            var uids = _sessionToCtx.Keys.ToList();
            ClearScopeData();
            return uids;
        }
        #endregion

        internal void RemoveSession(string sessionUid)
        {
            if (!_sessionToCtx.TryRemove(sessionUid, out var ctxId))
                return;
            _ctxToSession.TryRemove(ctxId, out var _);
            _ctxToExecData.TryRemove(ctxId, out var _);
            _ctxToRegistrator.TryRemove(ctxId, out var _);
            _sessionToObject.TryRemove(sessionUid, out var _);
        }

        internal void ClearScopeData()
        {
            _ctxToSession.Clear();
            _sessionToCtx.Clear();
            _ctxToExecData.Clear();
            _ctxToRegistrator.Clear();
            _sessionToObject.Clear();
        }
        #endregion
        #region Coverage data
        /// <summary>
        /// Register coverage from instrumented target by cross-point Uid 
        /// </summary>
        /// <param name="pointUid"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool RegisterCoverage(string pointUid, string ctx = null)
        {
            //global session
            var isGlobalReg = false;
            if (_globalRegistrator != null)
                isGlobalReg = _globalRegistrator.RegisterCoverage(pointUid); //always register

            //user session
            var reg = GetUserRegistrator(ctx);
            if (reg != null)
                return reg.RegisterCoverage(pointUid);
            else
                return isGlobalReg;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_inTimer)
                return;
            _inTimer = true;
            try
            {
                SendCoverages();
            }
            catch { } //TODO: log?
            finally
            {
                _inTimer = false;
            }
        }

        internal void SendCoverages()
        {
            lock (_sendLocker)
            {
                if (_globalRegistrator != null)
                    SendCoverageData(_globalRegistrator);

                foreach (var ctxId in _ctxToSession.Keys)
                {
                    if (!_ctxToRegistrator.TryGetValue(ctxId, out var reg))
                        continue; // reg = GetUserRegistrator(); //?? hmmm...
                    SendCoverageData(reg);
                }
            }
        }

        private void SendCoverageData(CoverageRegistrator reg)
        {
            var sessionUid = reg?.Session?.SessionId;
            if (sessionUid == null)
                return;
            var execClasses = reg.AffectedTypes.ToList();

            switch (execClasses.Count)
            {
                case 0:
                    return;
                case > 65535: //Drill side's restriction
                    //TODO: implement sending in cycle by chunk by 65535 probes (find out more from the engineers)
                    break;
                default:
                    Communicator.Sender.SendCoverageData(sessionUid, execClasses);
                    break;
            }

            var session = reg.Session;
            if (session is { IsRealtime: true })
                Communicator.Sender.SendSessionChangedMessage(sessionUid, reg.AffectedProbeCount);
            reg.ClearAffectedData();
        }

        private void StartSendCycle()
        {
            _sendTimer.Enabled = true;
        }

        private void StopSendCycle()
        {
            _sendTimer.Enabled = false;
        }

        private void StopSendCycleIfNeeded()
        {
            if(_ctxToRegistrator.Count == 0)
                _sendTimer.Enabled = false;
        }

        /// <summary>
        /// Get the coverage registrator by current context if exists and otherwise create it
        /// </summary>
        /// <returns></returns>
        public CoverageRegistrator GetUserRegistrator(string ctx = null)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            ctx = Contexter.GetContextId();
            //Debug.WriteLine($"Profiler: id={ctxId}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageRegistrator reg;
            if (_ctxToRegistrator.ContainsKey(ctx))
            {
                _ctxToRegistrator.TryGetValue(ctx, out reg);
                if(reg is {Session: null})
                    reg.Session = GetManualUserSession();
            }
            else
            {
                //TODO: do it properly! Need right binding ctx to session!
                var session = GetManualUserSession();
                if (session == null)
                    return null;
                reg = CreateCoverageRegistrator(session);
                _ctxToRegistrator.TryAdd(ctx, reg);
            }
            return reg;
        }

        private StartSessionPayload GetManualUserSession()
        {
            return _sessionToObject.Values
                .FirstOrDefault(a => a.TestType == AgentConstants.TEST_MANUAL &&
                                    (_globalRegistrator == null || _globalRegistrator.Session != a));
        }

        internal CoverageRegistrator CreateCoverageRegistrator(StartSessionPayload session)
        {
            return _converter.CreateCoverageRegistrator(session, _injTypes);
        }
        #endregion
        #region Commands
        /// <summary>
        /// Do some command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public void ExecCommand(int command, string data)
        {
            _logger.Info($"Command: {command} -> {data}");
            switch (command)
            {
                case 2: StartSession(data); break;
                case 3: StopSession(data); break;
            }
        }

        /// <summary>
        /// Automatic command from Agent to Admin side to start the session (for autotests)
        /// </summary>
        /// <param name="name"></param>
        internal void StartSession(string name)
        {

        }

        /// <summary>
        /// Automatic command from Agent to Admin side to stop the session (for autotests)
        /// </summary>
        /// <param name="name"></param>
        internal void StopSession(string name)
        {

        }
        #endregion
    }
}
