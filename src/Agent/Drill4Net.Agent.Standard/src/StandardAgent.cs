﻿using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Profiling.Tree;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.Admin.Requester;

/*** INFO
 automatic version tagger including Git info - https://github.com/devlooped/GitInfo
 semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
 the most common format is v0.0 (or just 0.0 is enough)
 to change semVer it is nesseccary to create appropriate tag and push it to remote repository
 patches'(commits) count starts with 0 again after new tag pushing
 For file version format exactly is digit
***/
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
    /// Standard Agent (Profiler) for the Drill Admin side
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class StandardAgent : AbstractAgent
    {
        /// <summary>
        /// Agent as singleton
        /// </summary>
        public static StandardAgent Agent { get; private set; }

        private IAgentReceiver Receiver => _comm?.Receiver;

        //in fact, it is the Main sender. Others are additional ones - as plugins
        private IAgentCoveragerSender CoverageSender => _comm?.Sender;

        /// <summary>
        /// Repository for Agent
        /// </summary>
        public StandardAgentRepository Repository { get; }

        /// <summary>
        /// Is the Agent initialized?
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public string EmergencyLogDir { get; }

        private readonly ICommunicator _comm;
        private readonly ManualResetEvent _initEvent = new(false);
        private static List<AstEntity> _entities;
        private static InitActiveScope _scope;

        private static bool _isFastInitializing;
        private readonly AdminRequester _requester;

        private static readonly Logger _logger;
        private static FileSink _probeLogger;
        private readonly bool _writeProbesToFile;
        private readonly AssemblyResolver _resolver;
        private static readonly object _entitiesLocker = new();
        private static readonly string _logPrefix;

        /*****************************************************************************/

        static StandardAgent() //it's needed for invocation from Target
        {
            var extras = new Dictionary<string, object> { { "PID", CommonUtils.CurrentProcessId } };
            _logger = new TypedLogger<StandardAgent>(CoreConstants.SUBSYSTEM_AGENT, extras);
            _logPrefix = nameof(StandardAgent);

            if (StandardAgentCCtorParameters.SkipCctor)
                return;

            Agent = new StandardAgent();
            if (Agent == null)
            {
                _logger.Fatal("Creation is failed");
                throw new Exception($"{_logPrefix}: creation is failed");
            }
            Agent.Initialized += Agent_Initialized;

            //recreate the logger with external context
            _logger = new TypedLogger<StandardAgent>($"{Agent.Repository.Subsystem}/{CoreConstants.SUBSYSTEM_AGENT}", extras);
        }

        private StandardAgent(): this(null, null) { }

        private StandardAgent(AgentOptions opts, InjectedSolution tree)
        {
            try
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
                AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
                _resolver = new AssemblyResolver();

                EmergencyLogDir = FileUtils.EmergencyDir;
                AbstractRepository.PrepareEmergencyLogger(FileUtils.LOG_FOLDER_EMERGENCY);

                _logger.Debug($"{_logPrefix} is initializing...");

                #region TEST assembly resolving
                //var ver = "Microsoft.Data.SqlClient.resources, Version=2.0.20168.4, Culture=en-US, PublicKeyToken=23ec7fc2d6eaa4a5";
                //var ver = "System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

                //var ver = "System.Private.Xml.resources, Version=4.0.2.0, Culture=en-US, PublicKeyToken=cc7b13ffcd2ddd51";
                //var reqPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Private.Xml.dll";

                //var ver = "Drill4Net.Target.Common.VB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                //var reqPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected\Drill4Net.Target.Net50.App\net5.0\";
                //var asm = _resolver.Resolve(ver, reqPath);

                //var asm = _resolver.ResolveResource(@"d:\Projects\IHS-bdd.Injected\de-DE\Microsoft.Data.Tools.Schema.Sql.resources.dll", "Microsoft.Data.Tools.Schema.Sql.Deployment.DeploymentResources.en-US.resources");
                #endregion

                Repository = opts == null || tree == null ? new StandardAgentRepository() : new StandardAgentRepository(opts, tree);
                if (opts == null)
                    opts = Repository.Options;
                _comm = Repository.Communicator;

                //it needed to be done here  before connect to Admin by Connector with websocket
                _requester = new AdminRequester(opts.Admin.Url, Repository.TargetName, Repository.TargetVersion);
                _isFastInitializing = GetIsFastInitilizing();
                _logger.Debug($"Initializing is fast: {_isFastInitializing}");

                //events from admin side
                Receiver.InitScopeData += OnInitScopeData;
                Receiver.PluginLoaded += PluginLoaded;
                Receiver.TogglePlugin += OnTogglePlugin;
                Receiver.RequestClassesData += OnRequestClassesData;
                Receiver.StartSession += OnStartSession;
                Receiver.CancelSession += OnCancelSession;
                Receiver.CancelAllSessions += OnCancelAllSessions;
                Receiver.StopSession += OnFinishSession;
                Receiver.StopAllSessions += OnFinishAllSessions;

                _comm.Connect(); //connect to Drill Admin side

                //debug
                _writeProbesToFile = Repository.Options.Debug is { Disabled: false, WriteProbes: true };
                if (_writeProbesToFile)
                {
                    var probeLogfile = Path.Combine(FileUtils.GetCommonLogDirectory(FileUtils.EntryDir), "probes.log");
                    _logger.Debug($"Probes writing to [{probeLogfile}]");
                    if (File.Exists(probeLogfile))
                        File.Delete(probeLogfile);
                    _probeLogger = new FileSink(probeLogfile);
                }

                //...and now we will wait the events from the admin side and the
                //probe's data from the instrumented code on the RegisterStatic

                _logger.Debug($"{_logPrefix} is primarly initialized.");
            }
            catch (Exception ex)
            {
                _logger.Fatal($"{_logPrefix}: error of initializing", ex);
            }
        }

        /*****************************************************************************/

        #region Init
        /// <summary>
        /// Init of the static Agent singleton.
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="tree"></param>
        /// <param name="initHandler"></param>
        public static void Init(AgentOptions opts, InjectedSolution tree, AgentInitializedHandler initHandler)
        {
            Agent = new StandardAgent(opts, tree);
            if (Agent == null)
                throw new Exception($"{_logPrefix}: creation is failed");
            Agent.Initialized += Agent_Initialized; //internal
            Agent.Initialized += initHandler; //external
        }

        private bool GetIsFastInitilizing()
        {
            var summaryInfo = _requester.GetBuildSummaries();
            if (summaryInfo == null || summaryInfo.Count == 0)
                return false;
            var exactly = summaryInfo.Find(a => a.BuildVersion == Repository.TargetVersion);
            if (exactly?.Summary != null) //such version already exists
                return true;
            //var last = summaryInfo.OrderByDescending(a => a.DetectedAt).First();
            //if(last.Summary == null)
            //    return false;
            return false;
        }

        //TODO: parameter with plugin name and checkikn if this plugin... test2code (guanito)
        private void PluginLoaded()
        {
            if (_isFastInitializing)
                Agent_Initialized();
        }

        private static void Agent_Initialized()
        {
            if (IsInitialized)
                return;
            //
            IsInitialized = true;
            Agent.RaiseInitilizedEvent();
            Agent.ReleaseProbeProcessing();

            _logger.Debug($"{nameof(StandardAgent)} is fully initialized.");
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            CommonUtils.LogFirstChanceException(EmergencyLogDir, _logPrefix, e.Exception);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveAssembly(EmergencyLogDir, _logPrefix, args, _resolver, null); //TODO: use BanderLog!
        }

        private Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveResource(EmergencyLogDir, _logPrefix, args, _resolver, null); //TODO: use BanderLog!
        }

        private Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveType(EmergencyLogDir, _logPrefix, args, null); //TODO: use BanderLog!
        }
        #endregion
        #region Events from Admin side
        /// <summary>
        /// Handler of the event for the creating new test scope on the Admin side
        /// </summary>
        private void OnInitScopeData(InitActiveScope scope)
        {
            try
            {
                Repository.RegisterAllSessionsCancelled(); //just in case
                _scope = scope;
                CoverageSender.SendScopeInitialized(scope, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Fatal("Scope init failed", ex);
            }
        }

        /// <summary>
        /// Handler of the event for the requsteing classes data of the Target from the Admin side
        /// </summary>
        private void OnRequestClassesData()
        {
            try
            {
                lock (_entitiesLocker)
                {
                    _entities = Repository.GetEntities();
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("Getting classes data failed", ex);
            }
        }

        /// <summary>
        /// Handler of the event for the toggling some plugin on the Admin side
        /// </summary>
        private void OnTogglePlugin(string plugin)
        {
            try
            {
                lock (_entitiesLocker)
                {
                    if (_entities == null)
                        return;

                    //1. Init message
                    CoverageSender.SendInitMessage(_entities.Count);

                    //2. Send injected classes info to admin side
                    CoverageSender.SendClassesDataMessage(_entities);
                    _entities.Clear();
                    _entities = null;
                }

                //3. Send "Initialized" message to admin side
                CoverageSender.SendInitializedMessage();

                //4. Send message "Can start to execute the probes" to the Target from Worker
                RaiseInitilizedEvent();
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Toggle plugin {plugin} is failed", ex);
            }
        }

        #region Session
        private void OnStartSession(StartAgentSession info)
        {
            try
            {
                RegisterStartedSession(info.Payload);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Session start for [{info}] is failed", ex);
            }
        }

        private void RegisterStartedSession(StartSessionPayload load)
        {
            Repository.RegisterSessionStarted(load);
            CoverageSender.SendSessionStartedMessage(load.SessionId, load.TestType, load.IsRealtime, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnFinishSession(StopAgentSession info)
        {
            try
            {
                RegisterFinishedSession(info.Payload.SessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Session finish for [{info}] is failed", ex);
            }
        }

        private void RegisterFinishedSession(string uid)
        {
            Repository.RegisterSessionStopped(uid);
            CoverageSender.SendSessionFinishedMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnFinishAllSessions()
        {
            try
            {
                var uids = Repository.RegisterAllSessionsStopped();
                CoverageSender.SendAllSessionFinishedMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error("Finishing for all sessions is is failed", ex);
            }
        }

        private void OnCancelSession(CancelAgentSession info)
        {
            try
            {
                var uid = info.Payload.SessionId;
                Repository.RegisterSessionCancelled(info);
                CoverageSender.SendSessionCancelledMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error($"Session cancel for [{info}] is failed", ex);
            }
        }

        private void OnCancelAllSessions()
        {
            try
            {
                var uids = Repository.RegisterAllSessionsCancelled();
                CoverageSender.SendAllSessionCancelledMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error("Cancelling for all sessions is failed", ex);
            }
        }
        #endregion
        #endregion
        #region Register probes from Target
        #region Static register
        /// <summary>
        ///  Registers the probe data from the injected Target app
        /// </summary>
        /// <param name="data"></param>
        public static void RegisterStatic(string data) //don't combine this signatute with next one
        {
            Agent?.Register(data);
        }

        /// <summary>
        /// Registering probe's data from injected Target app
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterWithContextStatic(string data, string ctx)
        {
            Agent?.RegisterWithContext(data, ctx);
        }
        #endregion
        #region Object's register
        /// <summary>
        /// Registering probe's data from injected Target app
        /// directly in its sys process. The context of probe is retrieved here
        /// (in this sys process).
        /// </summary>
        /// <param name="data"></param>
        public override void Register(string data)
        {
            var ctx = Repository?.GetContextId(); //it is only for local Agent injected directly in Target's sys process
            RegisterWithContext(data, ctx);
        }

        /// <summary>
        /// Registering probe's data from injected Target app
        /// with known its context
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        public override void RegisterWithContext(string data, string ctx)
        {
            try
            {
                _initEvent.WaitOne(); //in fact, the blocking will be only one time on the init

                #region Checks
                if (Repository?.IsAnySession != true)
                    return;
                if (string.IsNullOrWhiteSpace(data))
                {
                    _logger.Error("Data is empty");
                    return;
                }
                #endregion

                var ar = data.Split('^'); //data can contains some additional info in the debug mode
                var probeUid = ar[0];
                //var asmName = ar[1];
                //var funcName = ar[2];
                //var probe = ar[3];         

                if (_writeProbesToFile)
                    _probeLogger?.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{ctx} -> {probeUid}");

                var res = Repository.RegisterCoverage(probeUid, ctx);
                if (!res) //for debug
                { }
            }
            catch (Exception ex)
            {
                _logger.Error($"{data}", ex);
            }
        }
        #endregion
        #endregion
        #region Commands: autotests, etc

        //each agent can serve only one target, so there is only one autotest session
        private StartSessionPayload _curAutoSession;

        /// <summary>
        /// Do some command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public static void DoCommand(int command, string data)
        {
            //DON'T refactor parameters to the Command type, because
            //some injections wait exactly current parameters
            Agent.ExecCommand(command, data);
            Log.Flush();
        }

        /// <summary>
        /// Do some command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public void ExecCommand(int command, string data)
        {
            var type = (AgentCommandType)command;
            _logger.Debug($"Command received: [{type}] -> [{data}]");

            Repository.RegisterCommand(command, data);

            TestCaseContext testCaseCtx;
            switch (type)
            {
                case AgentCommandType.ASSEMBLY_TESTS_START: StartAutoSession(data); break;
                case AgentCommandType.ASSEMBLY_TESTS_STOP: StopAutoSession(); break;

                #region BDD features, test groups...
                //now, in fact, these are group of tests for one "test method" with many different cases
                //case AgentCommandType.TEST_START:
                //    break;
                //case AgentCommandType.TEST_STOP:
                //    break;
                #endregion

                case AgentCommandType.TEST_CASE_START:
                    testCaseCtx = Repository.GetTestCaseContext(data);
                    RegisterTestInfoStart(testCaseCtx);
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    testCaseCtx = Repository.GetTestCaseContext(data);
                    RegisterTestInfoFinish(testCaseCtx);
                    break;
                default:
                    _logger.Warning($"Skipping command: [{type}] -> [{data}]");
                    break;
            }
        }

        #region Manage sessions on Agent side
        /// <summary>
        /// Automatic command from Agent to Admin side to start the session (for autotests)
        /// </summary>
        /// <param name="metadata">Some info about session. It can be empty</param>
        internal void StartAutoSession(string metadata)
        {
            var session = GetAutoSessionName(metadata);
            _logger.Info($"Admin side session is starting: [{session}]");

            _curAutoSession = new StartSessionPayload
            {
                SessionId = session,
                TestName = null,
                TestType = AgentConstants.TEST_AUTO,
                IsGlobal = false,
                IsRealtime = true
            };
            RegisterStartedSession(_curAutoSession);

            CoverageSender.SendStartSessionCommand(session); //actually starting the session
            _logger.Info($"Admin side session is started: [{session}]");
        }

        /// <summary>
        /// Automatic command from Agent to Admin side to stop the session (for autotests)
        /// </summary>
        /// <param name="metadata"></param>
        internal void StopAutoSession()
        {
            var session = _curAutoSession.SessionId;
            _logger.Info($"Agent have to stop the session: [{session}]");

            SendRemainedCoverage();

            //DON'T REMOVE THESE COMMENTED LINES: the scope will be removed as entity from Drill Admin... soon...

            //actually force stopping the session from Connector DLL API
            //CoverageSender.SendStopSessionCommand(session);

            //TODO: check if it received in AgentReceiver (more proper do it there)
            //send message to admin side about finishing
            //RegisterFinishedSession(session); //uncomment this if without SendStopSessionCommand (and comment the next line)
            //CoverageSender.SendSessionFinishedMessage(session, CommonUtils.GetCurrentUnixTimeMs()); //name as uid (still)

            //we need to finish test scope + force finish the session
            CoverageSender.SendFinishScopeAction();

            _curAutoSession = null;

            ReleaseProbeProcessing(); //excess probes aren't need

            _logger.Info($"Admin side session is stopped: [{session}]");
        }

        internal string GetAutoSessionName(string metadata)
        {
            //in fact, this metadata is just test run's name, and actually may be empty
            if (string.IsNullOrWhiteSpace(metadata))
                return Guid.NewGuid().ToString();
            const int limLength = 64;
            if (metadata.Length > limLength)
                metadata = metadata.Substring(0, limLength);
            return NormalizeSessionName(metadata);
        }

        internal string NormalizeSessionName(string session)
        {
            if (string.IsNullOrWhiteSpace(session))
                return Guid.NewGuid().ToString();
            //
            if (session.Contains(" "))
            {
                var ar = session.Split(' ');
                for (int i = 0; i < ar.Length; i++)
                {
                    string word = ar[i];
                    if (string.IsNullOrWhiteSpace(word))
                        continue;
                    char[] a = word.ToLower().ToCharArray();
                    a[0] = char.ToUpper(a[0]);
                    ar[i] = new string(a);
                }
                session = string.Join(null, ar).Replace(" ", null);
            }
            session = HttpUtility.UrlEncode(session);
            return session;
        }
        #endregion

        internal void RegisterTestInfoStart(TestCaseContext testCtx)
        {
            BlockProbeProcessing();

            //each agent can serve only one target, so there is only one autotest session
            if (_curAutoSession != null)
                Repository.RecreateSessionData(_curAutoSession, testCtx.CaseName); //because we need recreate the Coverager at all in the current session - guanito

            CoverageSender.RegisterTestCaseStart(testCtx);
            ReleaseProbeProcessing();
        }

        internal void RegisterTestInfoFinish(TestCaseContext testCtx)
        {
            BlockProbeProcessing();
            SendRemainedCoverage();
            CoverageSender.RegisterTestCaseFinish(testCtx);
        }

        private void SendRemainedCoverage()
        {
            Task.Delay(1500); // this is inefficient: TODO control by probe's context (=test) by dict
            if (_curAutoSession != null)
                Repository.SendCoverage(_curAutoSession.SessionId);
        }
        #endregion

        private void BlockProbeProcessing()
        {
            _initEvent.Reset();
        }

        private void ReleaseProbeProcessing()
        {
            _initEvent.Set();
        }
    }
}