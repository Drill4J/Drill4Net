using System;
using System.IO;
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
using Drill4Net.BanderLog.Sinks.File;

// automatic version tagger including Git info - https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing
// For file version format exactly is digit
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch)]

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
            _logger = new TypedLogger<StandardAgentRepository>(CoreConstants.SUBSYSTEM_AGENT);
            _logger.Debug("Creating...");

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

            Communicator = GetCommunicator(Options.Admin, Options.Target, Options.Connector); //TODO: connector's log options

            //target classes' tree
            if(tree == null)
                tree = ReadInjectedTree();
            _injTypes = GetTypesByCallerVersion(tree);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(1200);
            _sendTimer.Elapsed += Timer_Elapsed;

            _logger.Debug("Created.");
        }

        private AbstractCommunicator GetCommunicator(DrillServerOptions adminOpts, TargetData targetOpts, ConnectorAuxOptions connOpts)
        {
            if (adminOpts == null)
                throw new ArgumentNullException(nameof(adminOpts));
            return new Communicator(CoreConstants.SUBSYSTEM_AGENT, adminOpts.Url, GetAdminAgentConfig(targetOpts, connOpts));
        }

        internal AdminAgentConfig GetAdminAgentConfig(TargetData targOpts, ConnectorAuxOptions connOpts)
        {
            string targVersion = targOpts.Version;
            if (string.IsNullOrWhiteSpace(targVersion))
                targVersion = GetExecutingAssemblyVersion(); //Guanito: a little inproperly (in Worker we get its version)

            // aux connector parameters
            (var logFile, Microsoft.Extensions.Logging.LogLevel logLevel) = GetConnectorLogParameters(connOpts, _logger);

            return new AdminAgentConfig(targOpts.Name, targVersion, GetAgentVersion())
            {
                ConnectorLogFilePath = logFile,
                ConnectorLogLevel = logLevel
            };
        }

        internal (string logFile, Microsoft.Extensions.Logging.LogLevel logLevel) GetConnectorLogParameters(ConnectorAuxOptions connOpts, Logger logger)
        {
            var logDir = connOpts?.LogDir;
            var logFile = connOpts?.LogFile;
            var logLevel = Microsoft.Extensions.Logging.LogLevel.Debug; //TODO: get real level from ...somewhere

            //Guanito: just first file sink is bad idea...
            //the last because the firast may be just emergency logger
            var fileSink = logger?.GetManager()?.GetSinks()?.LastOrDefault(s => s is FileSink) as FileSink; 

            //dir
            if (string.IsNullOrWhiteSpace(logDir))
            {
                if (fileSink == null)
                {
                    logDir = FileUtils.GetEntryDir();
                }
                else
                {
                    logDir = Path.GetDirectoryName(fileSink.Filepath);
                }
            }
            else
            {
                logDir = FileUtils.GetFullPath(logDir);
            }

            //file path
            if (string.IsNullOrWhiteSpace(logFile)) //no file path
                logFile = AgentConstants.CONNECTOR_LOG_FILE_NAME;

            //is it file path?
            if (logFile.Contains(":") || logFile.Contains("..") || logFile.Contains("/") || logFile.Contains("\\"))
            {
                logFile = FileUtils.GetFullPath(logFile); //maybe it is relative path
            }
            else //it is just file name
            {
                logFile = Path.Combine(logDir, logFile);
            }
            _logger.Debug($"Connector's logging: [{logLevel}] to [{logFile}]");

            return (logFile, logLevel);
        }

        internal string GetAgentVersion()
        {
            return FileUtils.GetProductVersion(typeof(StandardAgentRepository));
        }

        /// <summary>
        /// In the Server/Worker distributed environment it is the Worker's version, not Target's one
        /// </summary>
        /// <returns></returns>
        internal string GetExecutingAssemblyVersion()
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
        public void RegisterSessionStarted(StartSessionPayload info)
        {
            //it is just "echo" from Admin service about session starting -
            //but AUTO start was initialized from Agent side and should not be recreated
            if (info.TestType == AgentConstants.TEST_AUTO)
            {
                if (_sessionToCtx.TryGetValue(info.SessionId, out var _))
                    return;
            }

            //...all another types of sessions must be reinitialized
            RecreateSessionData(info);
        }

        /// <summary>
        /// Recreate the session
        /// </summary>
        /// <param name="info"></param>
        public void RecreateSessionData(StartSessionPayload info)
        {
            RemoveSession(info.SessionId);
            AddSession(info);
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
        public void SessionStopped(string uid)
        {
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

            //local session
            var reg = GetLocalRegistrator(ctx);
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
        /// for local type of session (user's MANUAL or autotest's AUTO)
        /// </summary>
        /// <returns></returns>
        public CoverageRegistrator GetLocalRegistrator(string ctx = null)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            ctx = Contexter.GetContextId(); //GUANO: IT IS WRONG !!!!
            //Debug.WriteLine($"Profiler: id={ctxId}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageRegistrator reg;
            if (_ctxToRegistrator.ContainsKey(ctx))
            {
                _ctxToRegistrator.TryGetValue(ctx, out reg);
                if (reg is { Session: null })
                    reg.Session = TryGetLocalSession();
            }
            else
            {
                //TODO: do it properly! Need right binding ctx to session!
                var session = TryGetLocalSession();
                if (session == null)
                    return null;
                reg = CreateCoverageRegistrator(session);
                _ctxToRegistrator.TryAdd(ctx, reg);
            }
            return reg;
        }

        /// <summary>
        /// Try get the local session (user's MANUAL or autotest's AUTO)
        /// </summary>
        /// <returns>The session</returns>
        private StartSessionPayload TryGetLocalSession()
        {
            return TryGetManualSession() ?? TryGetAutoSession(); //GUANO
        }

        /// <summary>
        /// Get the auto-session (test2run executing)
        /// </summary>
        /// <returns></returns>
        private StartSessionPayload TryGetAutoSession()
        {
            return _sessionToObject.Values
                .FirstOrDefault(a => a.TestType == AgentConstants.TEST_AUTO);
        }

        /// <summary>
        /// Get the first manual user session
        /// </summary>
        /// <returns></returns>
        private StartSessionPayload TryGetManualSession()
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
    }
}
