using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Profiling.Tree;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

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

        private IAgentReceiver Receiver => _comm.Receiver;
        private IAgentSender Sender => _comm.Sender;

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
            var extrasData = new Dictionary<string, object> { { "PID", CommonUtils.CurrentProcessId } };
            _logger = new TypedLogger<StandardAgent>(CoreConstants.SUBSYSTEM_AGENT, extrasData);

            if (StandardAgentCCtorParameters.SkipCctor)
                return;

            Agent = new StandardAgent();
            if (Agent == null)
            {
                _logger.Fatal("Creation is failed");
                throw new Exception($"{_logPrefix}: creation is failed");
            }
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
            Repository.CancelAllSessions(); //just in case
            _scope = scope;
            Sender.SendScopeInitialized(scope, CommonUtils.GetCurrentUnixTimeMs());
        }

        /// <summary>
        /// Handler of the event for the requsteing classes data of the Target from the Admin side
        /// </summary>
        private void OnRequestClassesData()
        {
            lock (_entLocker)
            {
                _entities = Repository.GetEntities();
            }
        }

        /// <summary>
        /// Handler of the event for the toggling some plugin on the Admin side
        /// </summary>
        private void OnTogglePlugin(string plugin)
        {
            lock (_entLocker)
            {
                if (_entities == null)
                    return; //log??

                //1. Init message
                Sender.SendInitMessage(_entities.Count);

                //2. Send injected classes info to admin side
                Sender.SendClassesDataMessage(_entities);
                _entities.Clear();
                _entities = null;
            }

            //3. Send "Initialized" message to admin side
            Sender.SendInitializedMessage();
        }

        #region Session
        private void OnStartSession(StartAgentSession info)
        {
            Repository.StartSession(info);
            var load = info.Payload;
            Sender.SendSessionStartedMessage(load.SessionId, load.TestType, load.IsRealtime, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnFinishSession(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            Repository.SessionStop(info);
            Sender.SendSessionFinishedMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnFinishAllSessions()
        {
            var uids = Repository.StopAllSessions();
            Sender.SendAllSessionFinishedMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnCancelSession(CancelAgentSession info)
        {
            var uid = info.Payload.SessionId;
            Repository.CancelSession(info);
            Sender.SendSessionCancelledMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
        }

        private void OnCancelAllSessions()
        {
            var uids = Repository.CancelAllSessions();
            Sender.SendAllSessionCancelledMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
        }
        #endregion
        #endregion
        #region Register
        /// <summary>
        /// Registering probe's data from injected Target app
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data, string ctx = null)
        {
            Agent?.Register(data, ctx);
        }

        /// <summary>
        /// Registering probe's data from injected Target app
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        public override void Register(string data, string ctx = null)
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
    }
}