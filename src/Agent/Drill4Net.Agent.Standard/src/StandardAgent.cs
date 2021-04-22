using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent : AbstractAgent
    {
        private static readonly ConcurrentDictionary<int, string> _execCtxToTestUids;
        private static readonly ConcurrentDictionary<string, int> _testUidToExecCtxs;
            
        private static ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _execCtxToExecData;

        private static readonly AgentReceiver _receiver;
        private static readonly AgentSender _sender;

        private static readonly ConcurrentDictionary<int, CoverageDispatcher> _coverageDisps;
        private static readonly StandardAgentRepository _rep;

        /*****************************************************************************/

        static StandardAgent()
        {
            PrepareLogger();
            Log.Debug("Initializing...");
            
            //test names vs. session ids
            _execCtxToTestUids = new ConcurrentDictionary<int, string>();
            _testUidToExecCtxs = new ConcurrentDictionary<string, int>();
            _coverageDisps = new ConcurrentDictionary<int, CoverageDispatcher>();

            // execution data by session ids
            _execCtxToExecData = new ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>>();

            try
            {
                _rep = new StandardAgentRepository();
                //
                var communicator = _rep.GetCommunicator();
                _receiver = new(communicator);
                _receiver.SessionStarted += SessionStarted;
                _receiver.SessionFinished += SessionFinished;
                _receiver.SessionCancelled += SessionCancelled;
                _receiver.AllSessionsCancelled += AllSessionsCancelled;
                //
                _sender = new(communicator);

                //1. Classes to admin side
                var entities = _rep.GetEntities();
                _sender.SendClassesDataMessage(entities);

                //2. "Initialized" message to admin side
                _sender.SendInitializedMessage();
                //
                Log.Debug("Initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error of {nameof(StandardAgent)} initializing");
            }
        }

        /*****************************************************************************/

        #region Session
        private static void SessionStarted(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            RemoveTest();
            AddTest(sessionUid);
        }
        
        private static void SessionFinished(string sessionUid, long finishTime)
        {
            //TODO: send data...
            
            //removing
            RemoveTest();
        }
        
        internal static void AddTest(string testName)
        {
            var id = GetSessionId();
            if (!_execCtxToTestUids.ContainsKey(id))
                return;
            _execCtxToTestUids.TryAdd(id, testName);
        }
        
        internal static void RemoveTest()
        {
            RemoveTest(GetSessionId());
        }
        
        internal static void RemoveTest(int sessionId)
        {
            if (!_execCtxToTestUids.ContainsKey(sessionId))
                return;
            _execCtxToTestUids.TryRemove(sessionId, out var _);
            _execCtxToExecData.TryRemove(sessionId, out var _);
        }

        private static void AllSessionsCancelled()
        {
            throw new NotImplementedException();
        }

        private static void SessionCancelled(string sessionUid, long cancelTime)
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

                GetCoverageDispather().RegisterCoverage(probeUid);
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
        
        public static CoverageDispatcher GetCoverageDispather()
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var id = GetSessionId();
            Debug.WriteLine($"Profiler: id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageDispatcher disp;
            if (_coverageDisps.ContainsKey(id))
            {
                _coverageDisps.TryGetValue(id, out disp);
            }
            else
            {
                var testName = $"test_{id}"; //TODO: real name is...????
                disp = _rep.CreateCoverageDispatcher(testName);
                _coverageDisps.TryAdd(id, disp);
            }
            return disp;
        }

        private static int GetSessionId()
        {
            return Thread.CurrentThread.ExecutionContext.GetHashCode();
        }
        
        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            //common log
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(StandardAgent)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}