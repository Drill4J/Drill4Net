using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Agent.Transport;
using Serilog;
using System.IO;

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
        public bool IsAnySession => _sessionToCtx.Any();

        private readonly ConcurrentDictionary<int, string> _ctxToSession;
        private readonly ConcurrentDictionary<string, int> _sessionToCtx;
        private readonly ConcurrentDictionary<string, StartSessionPayload> _sessionToObject;
        private readonly ConcurrentDictionary<int, CoverageRegistrator> _ctxToRegistrator;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _ctxToExecData;

        private CoverageRegistrator _globalRegistrator;
        private readonly TreeConverter _converter;
        private readonly IEnumerable<InjectedType> _injTypes;

        private readonly System.Timers.Timer _sendTimer;
        private readonly object _sendLocker;
        private bool _inTimer;

        /**************************************************************************************/

        /// <summary>
        /// Create repository for Standard Agent
        /// </summary>
        public StandardAgentRepository(string cfgPath = null): base(cfgPath)
        {
            //ctx maps
            _ctxToSession = new ConcurrentDictionary<int, string>();
            _ctxToRegistrator = new ConcurrentDictionary<int, CoverageRegistrator>();
            
            //session maps
            _sessionToCtx = new ConcurrentDictionary<string, int>();
            _sessionToObject = new ConcurrentDictionary<string, StartSessionPayload>();
            
            // execution data by session ids
            _ctxToExecData = new ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>>();

            _converter = new TreeConverter();
            _sendLocker = new object();

            Communicator = GetCommunicator(Options.Admin, Options.Target);

            //target classes' tree
            var tree = ReadInjectedTree();
            _injTypes = GetTypesByCallerVersion(tree);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(2000);
            _sendTimer.Elapsed += Timer_Elapsed;
        }

        /**************************************************************************************/

        #region Init
        private AbstractCommunicator GetCommunicator(AdminOptions adminOpts, TargetOptions targetOpts)
        {
            if (adminOpts == null)
                throw new ArgumentNullException(nameof(adminOpts));
            return new Communicator(adminOpts.Url, GetAgentPartConfig(targetOpts));
        }

        internal AgentPartConfig GetAgentPartConfig(TargetOptions targOpts)
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
                        injTypes = targetDir?.GetAssemblies().SelectMany(a => a.GetAllTypes());
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
        #region Start
        public void StartSession(StartAgentSession info)
        {
            var load = info.Payload;
            RemoveSession(load.SessionId);
            AddSession(load);
            StartSendCycle();
        }

        internal void AddSession(StartSessionPayload session)
        {
            var ctxId = GetContextId();
            if (_ctxToSession.ContainsKey(ctxId))
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
        public List<string> StopAllSessions()
        {
            StopSendCycle();
            SendCoverages();
            var uids = _sessionToCtx.Keys.ToList();
            ClearScopeData();
            return uids;
        }
        
        public void SessionStop(StopAgentSession info)
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
        public void CancelSession(CancelAgentSession info)
        {
            RemoveSession(info.Payload.SessionId);
        }

        public List<string> CancelAllSessions()
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
        /// <returns></returns>
        public bool RegisterCoverage(string pointUid)
        {
            //global session
            var isGlobalReg = false;
            if (_globalRegistrator != null)
                isGlobalReg = _globalRegistrator.RegisterCoverage(pointUid);

            //user session
            var reg = GetUserRegistrator();
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
                    SendCoverageDataFor(_globalRegistrator);

                foreach (var ctxId in _ctxToSession.Keys)
                {
                    if (!_ctxToRegistrator.TryGetValue(ctxId, out var reg))
                        reg = GetUserRegistrator(); //?? hmmm...
                    SendCoverageDataFor(reg);
                }
            }
        }

        private void SendCoverageDataFor(CoverageRegistrator reg)
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
                    //TODO: implement in cycle by chunk
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
        public CoverageRegistrator GetUserRegistrator()
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var ctxId = GetContextId();
            Debug.WriteLine($"Profiler: id={ctxId}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageRegistrator reg;
            if (_ctxToRegistrator.ContainsKey(ctxId))
            {
                _ctxToRegistrator.TryGetValue(ctxId, out reg);
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
                _ctxToRegistrator.TryAdd(ctxId, reg);
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

        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <returns></returns>
        internal int GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode() ?? 0;
        }
    }
}
