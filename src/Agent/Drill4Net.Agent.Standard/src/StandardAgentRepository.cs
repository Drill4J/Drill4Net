using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Transport;
using Drill4Net.Admin.Requester;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Repository for Standard Agent
    /// </summary>
    public sealed class StandardAgentRepository : AgentRepository
    {
        /// <summary>
        /// Target builds' information from Drill Admin service
        /// </summary>
        public List<BuildSummary> Builds { get; private set; }

        /// <summary>
        /// Any sesion is exists?
        /// </summary>
        public bool IsAnySession => _ctxToSessions.Count > 0;

        private ConcurrentDictionary<string, string> _ctxToSessions;
        private ConcurrentDictionary<string, List<string>> _sessionToCtxs;
        private ConcurrentDictionary<string, StartSessionPayload> _sessionToObjects;
        private ConcurrentDictionary<string, CoverageRegistrator> _ctxToRegistrators;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, ExecClassData>> _ctxToExecDatas;

        private CoverageRegistrator _globalRegistrator;
        private TreeConverter _converter;
        private IEnumerable<InjectedType> _injTypes;
        private AdminRequester _requester;
        private InjectedSolution _tree;

        private Logger _logger;
        private System.Timers.Timer _sendTimer;
        private object _sendLocker;
        private bool _inTimer;

        /**************************************************************************************/

        /// <summary>
        /// Create repository for Standard Agent by options specified by the file path
        /// </summary>
        /// <param name="cfgPath"></param>
        public StandardAgentRepository(string cfgPath = null) : base(cfgPath)
        {
            Init(null);
        }

        /// <summary>
        /// Create repository by paths for config and target's entities' tree
        /// </summary>
        /// <param name="cfgPath"></param>
        /// <param name="treePath"></param>
        public StandardAgentRepository(string cfgPath, string treePath) : base(cfgPath)
        {
            Init(ReadInjectedTree(treePath));
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
            _ctxToSessions = new ConcurrentDictionary<string, string>();
            _ctxToRegistrators = new ConcurrentDictionary<string, CoverageRegistrator>();

            //session maps
            _sessionToCtxs = new ConcurrentDictionary<string, List<string>>();
            _sessionToObjects = new ConcurrentDictionary<string, StartSessionPayload>();

            // execution data by session ids
            _ctxToExecDatas = new ConcurrentDictionary<string, ConcurrentDictionary<string, ExecClassData>>();

            _converter = new TreeConverter();
            _sendLocker = new object();

            //Target
            if (tree == null)
                tree = ReadInjectedTree();
            _tree = tree;
            if(!AgentInitParameters.LocatedInWorker)
                SetTypesByTargetVersion();

            var target = Options.Target;
            TargetName = target?.Name ?? tree.Name;
            TargetVersion = target?.Version ??
                            tree.SearchProductVersion(target?.VersionAssemblyName) ??
                            (AgentInitParameters.LocatedInWorker ? "0.0.0.0-unknown" : FileUtils.GetProductVersion(Assembly.GetCallingAssembly())); //for Agents injected directly to Target

            _logger.Info($"Target: {TargetName}, version: {TargetVersion}");

            _requester = new AdminRequester(Subsystem, Options.Admin.Url, TargetName, TargetVersion);
            RetrieveTargetBuilds().GetAwaiter().GetResult();

            //Communicator
            var targData = new TargetData
            {
                Name = TargetName,
                Version = TargetVersion,
            };
            Communicator = GetCommunicator(Options.Admin, targData, Options.Connector);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(1000);
            _sendTimer.Elapsed += Timer_Elapsed;

            _logger.Debug("Created.");
        }

        private AbstractCommunicator GetCommunicator(DrillServerOptions adminOpts, TargetData targetOpts, ConnectorAuxOptions connOpts)
        {
            if (adminOpts == null)
                throw new ArgumentNullException(nameof(adminOpts));
            //
            if (!GetAdminAddressFromEnvVar(out var adminUrl))
                adminUrl = adminOpts.Url;
            return new Communicator(CoreConstants.SUBSYSTEM_AGENT, adminUrl, GetAdminAgentConfig(targetOpts, connOpts));
        }

        /// <summary>
        /// It is used for the Docker environment - overrides the options for Drill service address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal bool GetAdminAddressFromEnvVar(out string address)
        {
            //.NET Core on macOS and Linux does not support per-machine or per-user environment variables.
            address = CommonUtils.ReadEnvironmentVar(CoreConstants.ENV_DRILL_ADMIN_ADDRESS);
            if (address == null)
            {
                _logger.Info("The environment variable for Drill service address is empty - will be used the config's value");
                return false;
            }
            _logger.Info($"Drill admin address found in the environment variables: {address}");
            return true;
        }

        internal AdminAgentConfig GetAdminAgentConfig(TargetData targOpts, ConnectorAuxOptions connOpts)
        {
            // aux connector parameters
            (var logFile, LogLevel logLevel) = GetConnectorLogParameters(connOpts, _logger);

            var version = GetAgentVersion();
            var cfg = new AdminAgentConfig(targOpts.Name, targOpts.Version, version)
            {
                ConnectorLogFilePath = logFile,
                ConnectorLogLevel = logLevel
            };
            _logger.Debug($"{nameof(AdminAgentConfig)}: {cfg}");
            return cfg;
        }

        /// <summary>
        /// Retrieve Target's builds to property <see cref="Builds"/>
        /// </summary>
        /// <returns></returns>
        public async Task RetrieveTargetBuilds()
        {
            try
            {
                Builds = await _requester.GetBuildSummaries()
                    .ConfigureAwait(false);
            }
            catch { Builds = new(); }
        }

        internal (string logFile, Microsoft.Extensions.Logging.LogLevel logLevel) GetConnectorLogParameters(ConnectorAuxOptions connOpts, Logger logger)
        {
            var logDir = connOpts?.LogDir;
            var logFile = connOpts?.LogFile;
            var logLevel = LogLevel.Debug; //TODO: get real level from ...somewhere

            //Guanito: just first file sink is bad idea...
            //the last because the firast may be just emergency logger
            FileSink fileSink = logger?.GetManager()?.GetSinks()?
                .LastOrDefault(s => s is FileSink) as FileSink;

            //dir
            if (string.IsNullOrWhiteSpace(logDir))
            {
                if (fileSink == null)
                    logDir = FileUtils.EntryDir;
                else
                    logDir = Path.GetDirectoryName(fileSink.Filepath);
            }
            else
            {
                logDir = FileUtils.GetFullPath(logDir);
            }

            //file path
            if (string.IsNullOrWhiteSpace(logFile)) //no file path
                logFile = AgentConstants.CONNECTOR_LOG_FILE_NAME;

            //is it file path?
            if (FileUtils.IsPossibleFilePath(logFile))
            {
                logFile = FileUtils.GetFullPath(logFile); //maybe it is relative path
            }
            else //it is just file name
            {
                logFile = Path.Combine(logDir, logFile);
            }
            _logger.Debug($"Connector logging: [{logLevel}] to [{logFile}]");

            return (logFile, logLevel);
        }

        internal string GetAgentVersion()
        {
            return FileUtils.GetProductVersion(typeof(StandardAgentRepository));
        }

        /// <summary>
        /// Get tool types, taking into account the current version of the target framework.
        /// </summary>
        /// <returns></returns>
        public void SetTypesByTargetVersion()
        {
            IEnumerable<InjectedType> injTypes = null;

            //check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = _tree.GetDirectories().ToList();
            _logger.Debug($"Root dirs: {rootDirs.Count}");
            Log.Flush();
            if (rootDirs.Count > 0 && _tree.GetAssemblies().Count() == 0) //maybe this is root of some monikers
            {
                var runDir = AgentInitParameters.TargetDir ?? (AgentInitParameters.LocatedInWorker ? null : FileUtils.EntryDir);
                if (string.IsNullOrWhiteSpace(runDir))
                    throw new Exception("Unknown target runtime dir");
                if (!runDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    runDir += "\\"; //Path.DirectorySeparatorChar;  TODO: depending on the target's OS
                _logger.Debug($"Target runtime dir: [{runDir}]");

                InjectedDirectory targetDir = null;

                //by directory directly
                foreach (var dir in rootDirs)
                {
                    var curDir = dir.DestinationPath;
                    if (!curDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        curDir += "\\"; //Path.DirectorySeparatorChar;  TODO: depending on the target's OS
                    if (!curDir.Equals(runDir, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug($"Dir [{curDir}] is skipped");
                        continue;
                    }
                    targetDir = dir;
                    _logger.Debug($"Target dir: {dir}");
                    break;
                }

                //by run target version
                if (targetDir == null)
                {
                    _logger.Warning("Target dir not found by its path directly");

                    //TODO: search by target version against potential entry version in each injected directory
                    //use AgentInitParameters.FrameworkVersion
                }
                injTypes = targetDir?.GetAllTypes();
            }

            if (injTypes == null)
                injTypes = _tree.GetAllTypes();

            _injTypes = injTypes.Distinct(new InjectedEntityComparer<InjectedType>());
            _logger.Debug($"Target types: {_injTypes.Count()}");
            Log.Flush();
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
                if (_sessionToCtxs.TryGetValue(info.SessionId, out var _))
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
            RemoveSessionData(info.SessionId);
            AddSessionData(info);
            StartSendCycle();
        }

        internal void AddSessionData(StartSessionPayload session)
        {
            string ctxId = null;
            if (session.TestType == AgentConstants.TEST_MANUAL) //maybe check the context parameter first?
                ctxId = session.TestName;
            if(string.IsNullOrWhiteSpace(ctxId))
                ctxId = GetContextId() ?? session.TestName ?? "no_context";

            if (_ctxToSessions.ContainsKey(ctxId)) //or recreate?!
                return;

            var sessionUid = session.SessionId;
            _sessionToObjects.TryAdd(sessionUid, session);
            _ctxToSessions.TryAdd(ctxId, sessionUid);
            _sessionToCtxs.TryAdd(sessionUid, new List<string> { ctxId });

            if (session.IsGlobal)
               _globalRegistrator = CreateCoverageRegistrator(ctxId, session);
        }
        #endregion
        #region Stop
        /// <summary>
        /// All sessions were stopped on the Admin side
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterAllSessionsStopped()
        {
            StopSendCycle();
            SendCoverages();
            var uids = _sessionToCtxs.Keys.ToList();
            ClearScopeData();
            return uids;
        }

        /// <summary>
        /// The session was stopped on the Admin side
        /// </summary>
        public void RegisterSessionStopped(string uid)
        {
            //send remaining data
            SendCoverages();

            //removing session/data
            RemoveSessionData(uid);
            StopSendCycleIfNeeded();
        }
        #endregion
        #region Cancel
        /// <summary>
        /// The session was cancelled on the Admin side
        /// </summary>
        /// <param name="info"></param>
        public void RegisterSessionCancelled(CancelAgentSession info)
        {
            RemoveSessionData(info.Payload.SessionId);
        }

        /// <summary>
        /// All sessions were cancelled on the Admin side
        /// </summary>
        public List<string> RegisterAllSessionsCancelled()
        {
            StopSendCycle();
            var uids = _sessionToCtxs.Keys.ToList();
            ClearScopeData();
            return uids;
        }
        #endregion

        internal void RemoveSessionData(string sessionUid)
        {
            if (!_sessionToCtxs.TryRemove(sessionUid, out var ctxList))
                return;
            foreach (var ctxId in ctxList.AsParallel())
            {
                _ctxToSessions.TryRemove(ctxId, out var _);
                _ctxToExecDatas.TryRemove(ctxId, out var _);
                _ctxToRegistrators.TryRemove(ctxId, out var _);
            }
            _sessionToObjects.TryRemove(sessionUid, out var _);
        }

        internal void ClearScopeData()
        {
            _ctxToSessions.Clear();
            _sessionToCtxs.Clear();
            _ctxToExecDatas.Clear();
            _ctxToRegistrators.Clear();
            _sessionToObjects.Clear();
        }
        #endregion
        #region Coverage data
        /// <summary>
        /// Register coverage from instrumented target by cross-point Uid 
        /// </summary>
        /// <param name="pointUid"></param>
        /// <param name="ctx"></param>
        /// <param name="missReason"></param>
        /// <returns></returns>
        public bool RegisterCoverage(string pointUid, string ctx, out string missReason)
        {
            //_logger.Trace($"point={pointUid}; ctx={ctx}"); // TEST !!!

            missReason = null;

            //global session
            var isGlobalReg = false;
            if (_globalRegistrator != null)
                isGlobalReg = _globalRegistrator.RegisterCoverage(pointUid, out missReason);

            //local session
            var reg = GetOrCreateLocalCoverageRegistrator(ctx);
            if (reg != null)
            {
                var res = reg.RegisterCoverage(pointUid, out missReason);
                if (!res && !_wasNoRegistraton)
                {
                    _logger.Error($"NO POINT REGISTRATION: [{pointUid}] -> [{ctx}]. This is signaled only at the first appearance. Reason: [{missReason}]");
                    _wasNoRegistraton = true;

                    //first 5 potentially alien points
                    var points = string.Join(", ", reg.PointToTypes.Keys.Take(5).ToList());
                    _logger.Debug($"First 5 existed points: {points}");
                }
                return res;
            }
            else
            {
                if (_globalRegistrator == null && _wasNoRegistraton)
                {
                    _wasNoRegistraton = true;
                    _logger.Error($"NO POINT REGISTRATORS: [{pointUid}] -> [{ctx}]. This is signaled only at the first appearance.");
                }
                return isGlobalReg;
            }
        }
        private bool _wasNoRegistraton;

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

                //send coverage
                var regs = _ctxToRegistrators.Values.Where(a => a.AffectedProbeCount > 0).AsParallel();
                foreach (var reg in regs)
                {
                    SendCoverageData(reg);
                }

                //clean old regs
                var timeout = new TimeSpan(0, 5, 0);
                var oldRegs = _ctxToRegistrators.Values.Where(a => a.AffectedProbeCount == 0 && (DateTime.Now - a.SentTime) > timeout).AsParallel();
                foreach (var reg in oldRegs)
                    _ctxToRegistrators.TryRemove(reg.Context, out _);
            }
        }

        internal void SendCoverage(string sessionUid)
        {
            var regs = GetCoverageRegistrators(sessionUid);
            if (regs == null)
                return; //??
            foreach(var reg in regs.AsParallel())
                SendCoverageData(reg);
        }

        private void SendCoverageData(CoverageRegistrator reg)
        {
            var sessionUid = reg?.Session?.SessionId;
            if (sessionUid == null)
                return;
            var execClasses = reg.AffectedTypes.ToList();

            const int maxCount = 65535;
            switch (execClasses.Count)
            {
                case 0:
                    return;
                case > maxCount: //Drill side's restriction
                    //TODO: implement sending in cycle by chunk by 65535 probes (find out more from the Drill engineers)
                    break;
                default:
                    Communicator.Sender.SendCoverageData(sessionUid, execClasses);
                    break;
            }

            var session = reg.Session;
            if (session is { IsRealtime: true })
                Communicator.Sender.SendSessionChangedMessage(sessionUid, reg.AffectedProbeCount);
            reg.ClearAffectedData();
            reg.SentTime = DateTime.Now;
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
            if(_ctxToRegistrators.Count == 0)
                _sendTimer.Enabled = false;
        }

        internal List<CoverageRegistrator> GetCoverageRegistrators(string sessionUid)
        {
            if (!_sessionToCtxs.TryGetValue(sessionUid, out var ctxList))
                return null;
            var regs = new List<CoverageRegistrator>();
            foreach (var ctxId in ctxList)
            {
                if (!_ctxToRegistrators.TryGetValue(ctxId, out var reg))
                    return null;
                regs.Add(reg);
            }
            return regs;
        }

        /// <summary>
        /// Get the coverage registrator by current context if exists and otherwise create it
        /// for local type of session (user's MANUAL or autotest's AUTO)
        /// </summary>
        /// <returns></returns>
        public CoverageRegistrator GetOrCreateLocalCoverageRegistrator(string ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx))
                ctx = GetContextId(); //it is only for local Agent injected directly in Target's OS process

            CoverageRegistrator reg;
            if (_ctxToRegistrators.ContainsKey(ctx))
            {
                _ctxToRegistrators.TryGetValue(ctx, out reg);
                if (reg is { Session: null })
                    reg.Session = TryGetLocalSession();
            }
            else //create
            {
                //TODO: do it properly! Need right binding ctx to session?
                var session = TryGetLocalSession();
                if (session == null)
                {
                    _logger.Warning($"No session for context [{ctx}]");
                    return null;
                }
                reg = CreateCoverageRegistrator(ctx, session);
                _ctxToRegistrators.TryAdd(ctx, reg);
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
            return _sessionToObjects.Values
                .FirstOrDefault(a => a.TestType == AgentConstants.TEST_AUTO);
        }

        /// <summary>
        /// Get the first manual user session
        /// </summary>
        /// <returns></returns>
        private StartSessionPayload TryGetManualSession()
        {
            return _sessionToObjects.Values
                .FirstOrDefault(a => a.TestType == AgentConstants.TEST_MANUAL &&
                                    //manual session in global mode not needed itself
                                    (_globalRegistrator == null || _globalRegistrator.Session != a)); 
        }

        internal CoverageRegistrator CreateCoverageRegistrator(string context, StartSessionPayload session)
        {
            var reg = _converter.CreateCoverageRegistrator(context, session, _injTypes);
            _logger.Debug($"{nameof(CoverageRegistrator)} is created: {reg}");
            return reg;
        }
        #endregion
    }
}
