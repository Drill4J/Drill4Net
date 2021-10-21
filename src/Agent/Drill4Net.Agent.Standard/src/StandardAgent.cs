﻿using System;
using System.Web;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Profiling.Tree;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

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

        //in fact, the Main sender. Others are additional ones - as plugins
        private IAgentCoveragerSender CoverageSender => _comm?.Sender;

        /// <summary>
        /// Repository for Agent
        /// </summary>
        public StandardAgentRepository Repository { get; }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public string EmergencyLogDir { get; }

        private readonly ICommunicator _comm;
        private static readonly ManualResetEvent _initEvent = new(false);
        private static List<AstEntity> _entities;
        private static InitActiveScope _scope;
        private static readonly Logger _logger;
        private readonly AssemblyResolver _resolver;
        private static readonly object _entLocker = new();
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

                //TEST assembly resolving!!!
                //var ver = "Microsoft.Data.SqlClient.resources, Version=2.0.20168.4, Culture=en-US, PublicKeyToken=23ec7fc2d6eaa4a5";
                //var ver = "System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

                //var ver = "System.Private.Xml.resources, Version=4.0.2.0, Culture=en-US, PublicKeyToken=cc7b13ffcd2ddd51";
                //var reqPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Private.Xml.dll";

                //var ver = "Drill4Net.Target.Common.VB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                //var reqPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected\Drill4Net.Target.Net50.App\net5.0\";
                //var asm = _resolver.Resolve(ver, reqPath);

                //var asm = _resolver.ResolveResource(@"d:\Projects\IHS-bdd.Injected\de-DE\Microsoft.Data.Tools.Schema.Sql.resources.dll", "Microsoft.Data.Tools.Schema.Sql.Deployment.DeploymentResources.en-US.resources");

                Repository = opts == null || tree == null ? new StandardAgentRepository() : new StandardAgentRepository(opts, tree);
                _comm = Repository.Communicator;

                //events from admin side
                Receiver.InitScopeData += OnInitScopeData;
                Receiver.TogglePlugin += OnTogglePlugin;
                Receiver.RequestClassesData += OnRequestClassesData;
                Receiver.StartSession += OnStartSession;
                Receiver.CancelSession += OnCancelSession;
                Receiver.CancelAllSessions += OnCancelAllSessions;
                Receiver.StopSession += OnFinishSession;
                Receiver.StopAllSessions += OnFinishAllSessions;

                _comm.Connect(); //connect to Drill Admin side

                //...and now we will wait the events from the admin side and the
                //probe's data from the instrumented code on the RegisterStatic

                _logger.Debug($"{_logPrefix} is initialized.");
            }
            catch (Exception ex)
            {
                _logger.Fatal($"{_logPrefix}: error of initializing", ex);
            }
            finally
            {
                _initEvent.Set();
            }
        }

        /*****************************************************************************/

        #region Init
        /// <summary>
        /// Init of the static Agent singleton.
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="tree"></param>
        public static void Init(AgentOptions opts, InjectedSolution tree)
        {
            Agent = new StandardAgent(opts, tree);
            if (Agent == null)
                throw new Exception($"{_logPrefix}: creation is failed");
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
        #region Events
        /// <summary>
        /// Handler of the event for the creating new test scope on the Admin side
        /// </summary>
        private void OnInitScopeData(InitActiveScope scope)
        {
            try
            {
                Repository.AllSessionsCancelled(); //just in case
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
                lock (_entLocker)
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
                lock (_entLocker)
                {
                    if (_entities == null)
                        return; //log??

                    //1. Init message
                    CoverageSender.SendInitMessage(_entities.Count);

                    //2. Send injected classes info to admin side
                    CoverageSender.SendClassesDataMessage(_entities);
                    _entities.Clear();
                    _entities = null;
                }

                //3. Send "Initialized" message to admin side
                CoverageSender.SendInitializedMessage();
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Togle plugin {plugin} failed", ex);
            }
        }

        #region Session
        private void OnStartSession(StartAgentSession info)
        {
            try
            {
                Repository.SessionStarted(info);
                var load = info.Payload;
                CoverageSender.SendSessionStartedMessage(load.SessionId, load.TestType, load.IsRealtime, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Session start for [{info}] failed", ex);
            }
        }

        private void OnFinishSession(StopAgentSession info)
        {
            try
            {
                var uid = info.Payload.SessionId;
                Repository.SessionStopped(info);
                CoverageSender.SendSessionFinishedMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error($"Session finish for [{info}] failed", ex);
            }
        }

        private void OnFinishAllSessions()
        {
            try
            {
                var uids = Repository.AllSessionsStopped();
                CoverageSender.SendAllSessionFinishedMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error("Finishing for all sessions is failed", ex);
            }
        }

        private void OnCancelSession(CancelAgentSession info)
        {
            try
            {
                var uid = info.Payload.SessionId;
                Repository.SessionCancelled(info);
                CoverageSender.SendSessionCancelledMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error($"Session cancel for [{info}] failed", ex);
            }
        }

        private void OnCancelAllSessions()
        {
            try
            {
                var uids = Repository.AllSessionsCancelled();
                CoverageSender.SendAllSessionCancelledMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
            }
            catch (Exception ex)
            {
                _logger.Error("Cancelling for all sessions is failed", ex);
            }
        }
        #endregion
        #endregion
        #region Register
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
        /// </summary>
        /// <param name="data"></param>
        public override void Register(string data)
        {
            RegisterWithContext(data, null);
        }

        /// <summary>
        /// Registering probe's data from injected Target app
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

                var res = Repository.RegisterCoverage(probeUid, ctx);
                if (!res) //for tests
                { }
            }
            catch (Exception ex)
            {
                _logger.Error($"{data}", ex);
            }
        }
        #endregion
        #endregion
        #region Commands
        /// <summary>
        /// Do some command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public static void DoCommand(int command, string data)
        {
            //DON'T refactor parameters to Command type, because
            //some injections wait exactly current parameters
            Agent.ExecCommand(command, data);
        }

        /// <summary>
        /// Do some command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public void ExecCommand(int command, string data)
        {
            var comTypes = Enum.GetValues(typeof(AgentCommandType)).Cast<int>().ToList();
            if (!comTypes.Contains(command))
            {
                _logger.Error($"Unknown command: [{command}] -> [{data}]");
                return;
            }
            //
            var type = (AgentCommandType)command;
            _logger.Debug($"Command: [{type}] -> [{data}]");
            TestCaseContext testCaseCtx = null;

            switch (type)
            {
                case AgentCommandType.ASSEMBLY_TESTS_START: StartSession(data); break;
                case AgentCommandType.ASSEMBLY_TESTS_STOP: StopSession(data); break;

                //now, in fact, these are group of tests for one "test method" with many different cases
                //case AgentCommandType.TEST_START:
                //    break;
                //case AgentCommandType.TEST_STOP:
                //    break;

                case AgentCommandType.TEST_CASE_START:
                    testCaseCtx = GetTestCaseContext(data);
                    SendTest2RunInfo(testCaseCtx);
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    testCaseCtx = GetTestCaseContext(data);
                    //....
                    break;
                default:
                    break;
            }
        }

        internal TestCaseContext GetTestCaseContext(string str)
        {
            return JsonConvert.DeserializeObject<TestCaseContext>(str);
        }

        #region Manage sessions on Agent side
        /// <summary>
        /// Automatic command from Agent to Admin side to start the session (for autotests)
        /// </summary>
        /// <param name="metadata">Some info about session. It can be empty</param>
        internal void StartSession(string metadata)
        {
            var session = GetSessionName(metadata);
            _logger.Info($"Starting admin side session: [{session}]");
            CoverageSender.SendStartSessionCommand(session);
        }

        /// <summary>
        /// Automatic command from Agent to Admin side to stop the session (for autotests)
        /// </summary>
        /// <param name="metadata"></param>
        internal void StopSession(string metadata)
        {
            var session = GetSessionName(metadata);
            _logger.Info($"Stopping admin side session: [{session}]");
            CoverageSender.SendStopSessionCommand(session);
        }

        internal string GetSessionName(string metadata)
        {
            var guidName = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(metadata))
                return guidName;
            if (metadata.Length > 32)
                metadata = metadata.Substring(0, 32);
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
            session = HttpUtility.HtmlEncode(session);
            return session;
        }
        #endregion

        internal void SendTest2RunInfo(TestCaseContext testCtx)
        {
            CoverageSender.SendTestRunStart(testCtx);
        }
        #endregion
    }
}