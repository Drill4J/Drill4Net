using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Core.Repository;

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
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public static string EmergencyLogDir { get; }

        private static readonly ICommunicator _comm;
        private static readonly StandardAgentRepository _rep;
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
                BaseRepository.PrepareInitLogger(FileUtils.LOG_FOLDER_EMERGENCY);

                Log.Debug("Initializing...");

                //TEST!!!
                //var ver = "Microsoft.Data.SqlClient.resources, Version=2.0.20168.4, Culture=en-US, PublicKeyToken=23ec7fc2d6eaa4a5";
                //var ver = "System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
                //var asm = _resolver.Resolve(ver);

                //var asm = _resolver.ResolveResource(@"d:\Projects\IHS-bdd.Injected\de-DE\Microsoft.Data.Tools.Schema.Sql.resources.dll", "Microsoft.Data.Tools.Schema.Sql.Deployment.DeploymentResources.en-US.resources");

                _rep = new StandardAgentRepository();
                _comm = _rep.Communicator;

                //events from admin side
                Receiver.InitScopeData += OnInitScopeData;
                Receiver.TogglePlugin += OnTogglePlugin;
                Receiver.RequestClassesData += OnRequestClassesData;
                Receiver.StartSession += OnStartSession;
                Receiver.CancelSession += OnCancelSession;
                Receiver.CancelAllSessions += OnCancelAllSessions;
                Receiver.StopSession += OnStopSession;
                Receiver.StopAllSessions += OnStopAllSessions;

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
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "resolve_failed.log"), new string[] { info });
            Log.Debug("Assembly [{Name}] didn't resolve ", name);
            return null;
        }

        private static Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            var asm = _resolver.ResolveResource(args.RequestingAssembly.Location, name);
            if (asm != null)
                return asm;
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(EmergencyLogDir, "resolve_resource_failed.log"), new string[] { info });
            return null;
        }

        private static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            var info = $"{CommonUtils.GetPreciseTime()}: {name} -> request from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
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
            _rep.CancelAllSessions(); //just in case
            _scope = scope;
            Sender.SendScopeInitialized(scope, GetCurrentUnixTimeMs());
        }

        /// <summary>
        /// Handler of the event for the requsteing classes data of the Target from the Admin side
        /// </summary>
        private static void OnRequestClassesData()
        {
            lock (_entLocker)
            {
                _entities = _rep.GetEntities();
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
            _rep.StartSession(info);
            var load = info.Payload;
            Sender.SendSessionStartedMessage(load.SessionId, load.TestType, load.IsRealtime, GetCurrentUnixTimeMs());
        }

        private static void OnStopSession(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            _rep.SessionStop(info);
            Sender.SendSessionFinishedMessage(uid, GetCurrentUnixTimeMs());
        }

        private static void OnCancelAllSessions()
        {
            var uids = _rep.CancelAllSessions();
            Sender.SendAllSessionCancelledMessage(uids, GetCurrentUnixTimeMs());
        }

        private static void OnCancelSession(CancelAgentSession info)
        {
            var uid = info.Payload.SessionId;
            _rep.CancelSession(info);
            Sender.SendSessionCancelledMessage(uid, GetCurrentUnixTimeMs());
        }

        private static void OnStopAllSessions()
        {
            var uids = _rep.StopAllSessions();
            Sender.SendAllSessionFinishedMessage(uids, GetCurrentUnixTimeMs());
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
                if (_rep?.IsAnySession != true)
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

                var res = _rep.RegisterCoverage(probeUid);
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