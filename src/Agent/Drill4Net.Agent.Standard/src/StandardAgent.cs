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
    /// Standard Agent (Profiler0) for Drill Admin side
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StandardAgent : AbstractAgent
    {
        private static readonly ICommunicator _comm;
        private static IReceiver Receiver => _comm.Receiver;
        private static ISender Sender => _comm.Sender;
        private static readonly StandardAgentRepository _rep;
        private static readonly ManualResetEvent _initEvent = new(false);
        private static List<AstEntity> _entities;
        private static InitActiveScope _scope;
        private static readonly object _entLocker = new();

        /*****************************************************************************/

        static StandardAgent()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

                PrepareLogger();
                Log.Debug("Initializing...");

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

                //...and now we will wait events from admin side and
                //probe's data from instrumented code on RegisterStatic

                //local tests
                // var testUid = Guid.NewGuid().ToString();
                // SendTest_StartSession(testUid);
                // SendTest_StopSession(testUid);
                // SendTest_StopAllSessions();
                // SendTest_CancelSession(testUid);
                // SendTest_CancelAllSessions();
                //
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

        /// <summary>
        /// It just run the ctor with the main init procedure.
        /// This function mainly used for debugging. It's not necessary
        /// in a real system because the ctor will be arised due Register call.
        /// </summary>
        public static void Init() { }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            File.AppendAllText(Path.Combine(FileUtils.GetExecutionDir(), "first_chance_error.log"), e.Exception.ToString());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var info = $"{args.Name}: {args.RequestingAssembly.FullName}";
            File.AppendAllText(Path.Combine(FileUtils.GetExecutionDir(), "resolve_failed.log"), info);
            return null;
        }

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

        private static void OnInitScopeData(InitActiveScope scope)
        {
            _rep.CancelAllSessions(); //just in case
            _scope = scope;
            Sender.SendScopeInitialized(scope, GetCurrentUnixTimeMs());
        }

        private static void OnRequestClassesData()
        {
            lock (_entLocker)
            {
                _entities = _rep.GetEntities();
            }
        }
        
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
                _initEvent.WaitOne(); //in fact, the blocking will be only one time on init

                #region Checks
                if (_rep == null || !_rep.IsAnySession)
                    return;
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log.Error("Data is empty");
                    return;
                }
                //
                var ar = data.Split('^');
                if (ar.Length < 4)
                {
                    Log.Error($"Bad format of input: {data}");
                    return;
                }
                #endregion

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
                Log.Error(ex, $"{data}");
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

        private static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            //common log
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(StandardAgent)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}