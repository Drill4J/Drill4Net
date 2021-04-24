using System;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StandardAgent : AbstractAgent
    {
        private static readonly ICommunicator _comm;
        private static IReceiver Receiver => _comm.Receiver;
        private static ISender Sender => _comm.Sender;
        private static readonly StandardAgentRepository _rep;
        private static readonly ManualResetEvent _initEvent = new(false);

        /*****************************************************************************/

        static StandardAgent()
        {
            try
            {
                PrepareLogger();
                Log.Debug("Initializing...");

                _rep = new StandardAgentRepository();
                _comm = _rep.Communicator;

                //handler of events from admin side
                Receiver.StartSession += StartSession;
                Receiver.CancelSession += CancelSession;
                Receiver.CancelAllSessions += CancelAllSessions;
                Receiver.StopSession += StopSession;
                Receiver.StopAllSessions += StopAllSessions;

                //1. Send injected class info to admin side
                var entities = _rep.GetEntities();
                Sender.SendClassesDataMessage(entities);

                //2. Send "Initialized" message to admin side
                Sender.SendInitializedMessage();
                
                //3. Waiting events from admin side...

                //Test
                var testUid = Guid.NewGuid().ToString();
                SendTest_StartSession(testUid);
                SendTest_StopSession(testUid);
                SendTest_StopAllSessions();
                SendTest_CancelSession(testUid);
                SendTest_CancelAllSessions();
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

        #region Temporary tests
        private static void SendTest_StartSession(string sessionUid)
        {
            var payload = new StartSessionPayload()
            {
                IsGlobal = false,
                IsRealtime = true,
                SessionId = sessionUid,
                TestName = "TEST1",
                TestType = "AUTO",
            };
            var data = new StartAgentSession {Payload = payload};
            Sender.SendTest(AgentConstants.MESSAGE_IN_START_SESSION, data);
        }
        
        private static void SendTest_StopSession(string sessionUid)
        {
            var payload = new AgentSessionPayload()
            {
                SessionId = sessionUid,
            };
            var data = new StopAgentSession {Payload = payload};
            Sender.SendTest(AgentConstants.MESSAGE_IN_STOP_SESSION, data);
        }
        
        private static void SendTest_StopAllSessions()
        {
            Sender.SendTest(AgentConstants.MESSAGE_IN_STOP_ALL, null);
        }
        
        private static void SendTest_CancelSession(string sessionUid)
        {
            var payload = new AgentSessionPayload()
            {
                SessionId = sessionUid,
            };
            var data = new CancelAgentSession {Payload = payload};
            Sender.SendTest(AgentConstants.MESSAGE_IN_CANCEL_SESSION, data);
        }
        
        private static void SendTest_CancelAllSessions()
        {
            Sender.SendTest(AgentConstants.MESSAGE_IN_CANCEL_ALL, null);
        }
        #endregion
        #region Session
        private static void StartSession(StartAgentSession info)
        {
            var uid = info.Payload.SessionId;
            _rep.StartSession(info);
            Sender.SendSessionStartedMessage(uid, GetCurrentUnixTimeMs());
        }

        private static void StopSession(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            _rep.SessionStop(info);
            Sender.SendSessionFinishedMessage(uid, GetCurrentUnixTimeMs());
        }

        private static void CancelAllSessions()
        {
            _rep.CancelAllSessions();
            Sender.SendCancelAllSessionsMessage(GetCurrentUnixTimeMs());
        }

        private static void CancelSession(CancelAgentSession info)
        {
            var uid = info.Payload.SessionId;
            _rep.CancelSession(info);
            Sender.SendSessionCanceledMessage(uid, GetCurrentUnixTimeMs());
        }

        private static void StopAllSessions()
        {
            _rep.StopAllSessions();
            Sender.SendStopAllSessionsMessage(GetCurrentUnixTimeMs());
        }
        #endregion
        #region Register
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
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
                
                _initEvent.WaitOne(); //in fact, block will be only one time on init
                _rep.GetCoverageDispather().RegisterCoverage(probeUid);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{data}");
            }
        }

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