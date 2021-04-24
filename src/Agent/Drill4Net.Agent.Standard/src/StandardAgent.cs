using System;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

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
                Sender.SendTest("TYPE", "MESS");
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

        #region Session
        private static void StartSession(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            _rep.SessionStarted(sessionUid, testType, isRealTime, startTime);
        }

        private static void StopSession(string sessionUid, long finishTime)
        {
            _rep.SessionStop(sessionUid, finishTime);
            Sender.SendSessionFinishedMessage(sessionUid, GetCurrentUnixTimeMs());
        }

        private static void CancelAllSessions()
        {
            throw new NotImplementedException();
        }

        private static void CancelSession(string sessionUid, long cancelTime)
        {
            throw new NotImplementedException();
        }

        private static void StopAllSessions(long finishtime)
        {
            throw new NotImplementedException();
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
                
                //in fact, block will be only one time on init
                _initEvent.WaitOne();
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