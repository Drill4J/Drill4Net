using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Standard Agent (Profiler) for the Drill Admin side
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StandardAgent : AbstractAgent
    {
        private static IReceiver Receiver => _comm.Receiver;
        private static ISender Sender => _comm.Sender;

        /// <summary>
        /// Repository for Agent
        /// </summary>
        public static StandardAgentRepository Repository { get; }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public static string EmergencyLogDir { get; }

        private static readonly ICommunicator _comm;

        private static readonly ManualResetEvent _initEvent = new(false);
        private static List<AstEntity> _entities;
        private static InitActiveScope _scope;
        private static readonly AssemblyResolver _resolver;
        private static readonly object _entLocker = new();

        /*****************************************************************************/

        static StandardAgent()
        {
            try
            {
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
                AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
                _resolver = new AssemblyResolver();

                EmergencyLogDir = FileUtils.GetEmergencyDir();
                AbstractRepository<AgentOptions>.PrepareInitLogger(FileUtils.LOG_FOLDER_EMERGENCY);

                Log.Debug("Initializing...");

                //TEST assembly resolving!!!
                //var ver = "Microsoft.Data.SqlClient.resources, Version=2.0.20168.4, Culture=en-US, PublicKeyToken=23ec7fc2d6eaa4a5";
                //var ver = "System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

                //var ver = "System.Private.Xml.resources, Version=4.0.2.0, Culture=en-US, PublicKeyToken=cc7b13ffcd2ddd51";
                //var reqPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Private.Xml.dll";

                //var ver = "Drill4Net.Target.Common.VB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                //var reqPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected\Drill4Net.Target.Net50.App\net5.0\";
                //var asm = _resolver.Resolve(ver, reqPath);

                //var asm = _resolver.ResolveResource(@"d:\Projects\IHS-bdd.Injected\de-DE\Microsoft.Data.Tools.Schema.Sql.resources.dll", "Microsoft.Data.Tools.Schema.Sql.Deployment.DeploymentResources.en-US.resources");

                Repository = new StandardAgentRepository();
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

                _comm.Connect();

                //...and now we will wait the events from the admin side and the
                //probe's data from the instrumented code on the RegisterStatic

                #region local tests
                // var testUid = Guid.NewGuid().ToString();
                // SendTest_StartSession(testUid);
                // SendTest_StopSession(testUid);
                // SendTest_StopAllSessions();
                // SendTest_CancelSession(testUid);
                // SendTest_CancelAllSessions();
                #endregion

                Log.Debug("Initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error of {nameof(StandardAgent)} initializing");
            }
            finally
            {
                _initEvent.Set();
            }
        }

        /*****************************************************************************/

        #region Init
        /// <summary>
        /// It just run the ctor with the main init procedure.
        /// This function mainly used for debugging. It's not necessary
        /// in a real system because the ctor will be arised due Register call.
        /// </summary>
        public static void Init() { }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "first_chance_error.log"),
                new string[] { $"{CommonUtils.GetPreciseTime()}: {e.Exception}" });
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            Log.Debug("Need resolve assembly: [{Name}]", name);
            var asm = _resolver.Resolve(name, args.RequestingAssembly.Location);
            if (asm != null)
                return asm;
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request assembly from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "resolve_failed.log"), new string[] { info });
            Log.Debug("Assembly [{Name}] didn't resolve", name);
            return args.RequestingAssembly; //null
        }

        private static Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            var asm = _resolver.ResolveResource(args.RequestingAssembly.Location, name);
            if (asm != null)
                return asm;
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request resource from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "resolve_resource_failed.log"), new string[] { info });
            return null;
        }

        private static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request type from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "resolve_type_failed.log"), new string[] { info });
            return null;
        }
        #endregion
        #region Events
        /// <summary>
        /// Handler of the event for the creating new test scope on the Admin side
        /// </summary>
        private static void OnInitScopeData(InitActiveScope scope)
        {
            Repository.CancelAllSessions(); //just in case
            _scope = scope;
            Sender.SendScopeInitialized(scope, CommonUtils.GetCurrentUnixTimeMs());
        }

        /// <summary>
        /// Handler of the event for the requsteing classes data of the Target from the Admin side
        /// </summary>
        private static void OnRequestClassesData()
        {
            lock (_entLocker)
            {
                _entities = Repository.GetEntities();
            }
        }

        /// <summary>
        /// Handler of the event for the toggling some plugin on the Admin side
        /// </summary>
        private static void OnTogglePlugin(string plugin)
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
        private static void OnStartSession(StartAgentSession info)
        {
            Repository.StartSession(info);
            var load = info.Payload;
            Sender.SendSessionStartedMessage(load.SessionId, load.TestType, load.IsRealtime, CommonUtils.GetCurrentUnixTimeMs());
        }

        private static void OnFinishSession(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            Repository.SessionStop(info);
            Sender.SendSessionFinishedMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
        }

        private static void OnFinishAllSessions()
        {
            var uids = Repository.StopAllSessions();
            Sender.SendAllSessionFinishedMessage(uids, CommonUtils.GetCurrentUnixTimeMs());
        }

        private static void OnCancelSession(CancelAgentSession info)
        {
            var uid = info.Payload.SessionId;
            Repository.CancelSession(info);
            Sender.SendSessionCancelledMessage(uid, CommonUtils.GetCurrentUnixTimeMs());
        }

        private static void OnCancelAllSessions()
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
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data)
        {
            try
            {
                _initEvent.WaitOne(); //in fact, the blocking will be only one time on the init

                #region Checks
                if (Repository?.IsAnySession != true)
                    return;
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log.Error("Data is empty");
                    return;
                }
                #endregion

                var ar = data.Split('^'); //data can contains some additional info in the debug mode
                var probeUid = ar[0];
                //var asmName = ar[1];
                //var funcName = ar[2];
                //var probe = ar[3];         

                var res = Repository.RegisterCoverage(probeUid);
                if (!res) //for tests
                { }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Data}", data);
            }
        }

        /// <summary>
        /// Registering probe's data from injected Target app
        /// </summary>
        /// <param name="data"></param>
        public override void Register(string data)
        {
            RegisterStatic(data);
        }
        #endregion
        #region Temporary tests
        // private static void SendTest_StartSession(string sessionUid)
        // {
        //     var payload = new StartSessionPayload()
        //     {
        //         IsGlobal = false,
        //         IsRealtime = true,
        //         SessionId = sessionUid,
        //         TestName = "TEST1",
        //         TestType = "AUTO",
        //     };
        //     var data = new StartAgentSession {Payload = payload};
        //     Sender.SendTest(data);
        // }
        //
        // private static void SendTest_StopSession(string sessionUid)
        // {
        //     var payload = new AgentSessionPayload()
        //     {
        //         SessionId = sessionUid,
        //     };
        //     var data = new StopAgentSession {Payload = payload};
        //     Sender.SendTest(data);
        // }
        //
        // private static void SendTest_StopAllSessions()
        // {
        //     Sender.SendTest(new StopAllAgentSessions());
        // }
        //
        // private static void SendTest_CancelSession(string sessionUid)
        // {
        //     var payload = new AgentSessionPayload()
        //     {
        //         SessionId = sessionUid,
        //     };
        //     var data = new CancelAgentSession {Payload = payload};
        //     Sender.SendTest(data);
        // }
        //
        // private static void SendTest_CancelAllSessions()
        // {
        //     Sender.SendTest(new CancelAllAgentSessions());
        // }
        #endregion
    }
}