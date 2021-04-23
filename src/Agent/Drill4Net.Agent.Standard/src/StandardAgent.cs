using System;
using System.IO;
using System.Linq;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent : AbstractAgent
    {
        private static readonly ICommunicator _comm;
        private static IReceiver Receiver => _comm.Receiver;
        private static ISender Sender => _comm.Sender;
        private static readonly StandardAgentRepository _rep;
        private static object _initLock;

        /*****************************************************************************/

        static StandardAgent()
        {
            _initLock = new object();
            lock (_initLock)
            {
                PrepareLogger();
                Log.Debug("Initializing...");

                try
                {
                    _rep = new StandardAgentRepository();
                    _comm = _rep.Communicator;

                    Receiver.SessionStarted += SessionStarted;
                    Receiver.SessionStop += SessionStop;
                    Receiver.SessionCancelled += SessionCancelled;
                    Receiver.AllSessionsCancelled += AllSessionsCancelled;
                    Receiver.SessionChanged += SessionChanged;

                    //1. Classes to admin side
                    var entities = _rep.GetEntities();
                    Sender.SendClassesDataMessage(entities);

                    //2. "Initialized" message to admin side
                    Sender.SendInitializedMessage();
                    //
                    Log.Debug("Initialized.");
                    //
                    //test
                    Task.Run(() => Sender.Send("aaa", "bbb"));
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"Error of {nameof(StandardAgent)} initializing");
                }
            }
        }

        /*****************************************************************************/

        #region Session
        private static void SessionStarted(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            _rep.SessionStarted(sessionUid, testType, isRealTime, startTime);
        }

        private static void SessionStop(string sessionUid, long finishTime)
        {
            _rep.SessionStop(sessionUid, finishTime);
            Sender.SendSessionFinishedMessage(sessionUid, GetCurrentUnixTimeMs());
        }

        private static void AllSessionsCancelled()
        {
            throw new NotImplementedException();
        }

        private static void SessionCancelled(string sessionUid, long cancelTime)
        {
            throw new NotImplementedException();
        }

        private static void SessionChanged(string sessionUid, int probeCnt)
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
                lock (_initLock)
                {
                    _rep.GetCoverageDispather().RegisterCoverage(probeUid);
                }
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

        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            //common log
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(StandardAgent)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}