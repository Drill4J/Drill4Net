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
        private static readonly ConcurrentDictionary<int, string> _ctxToSession;
        private static readonly ConcurrentDictionary<string, int> _sessionToCtx;
        private static readonly ConcurrentDictionary<int, CoverageDispatcher> _ctxToCoverage;
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _ctxToExecData;

        private static readonly AgentReceiver _receiver;
        private static readonly AgentSender _sender;

        private static readonly StandardAgentRepository _rep;

        /*****************************************************************************/

        static StandardAgent()
        {
            PrepareLogger();
            Log.Debug("Initializing...");
            
            //test names vs. session ids
            _ctxToSession = new ConcurrentDictionary<int, string>();
            _sessionToCtx = new ConcurrentDictionary<string, int>();
            _ctxToCoverage = new ConcurrentDictionary<int, CoverageDispatcher>();

            // execution data by session ids
            _ctxToExecData = new ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>>();

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
        #region Started
        private static void SessionStarted(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            RemoveSession(sessionUid);
            AddSession(sessionUid);
        }

        internal static void AddSession(string sessionUid)
        {
            var ctxId = GetContextId();
            if (!_ctxToSession.ContainsKey(ctxId))
                return;
            _ctxToSession.TryAdd(ctxId, sessionUid);
        }
        #endregion
        #region Finished
        private static void SessionFinished(string sessionUid, long finishTime)
        {
            //TODO: send remaining data...
            
            //removing
            RemoveSession(sessionUid);
        }
         
        internal static void RemoveSession(string sessionUid)
        {
            if (!_sessionToCtx.ContainsKey(sessionUid))
                return;
            //
            var ctxId = _sessionToCtx[sessionUid];
            _ctxToSession.TryRemove(ctxId, out var _);
            _ctxToExecData.TryRemove(ctxId, out var _);
            _ctxToCoverage.TryRemove(ctxId, out var _);
        }
        #endregion
        #region Cancelled
        private static void AllSessionsCancelled()
        {
            throw new NotImplementedException();
        }

        private static void SessionCancelled(string sessionUid, long cancelTime)
        {
            throw new NotImplementedException();
        }
        #endregion
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
        
        public static CoverageDispatcher GetCoverageDispather()
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var ctxId = GetContextId();
            Debug.WriteLine($"Profiler: id={ctxId}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageDispatcher disp;
            if (_ctxToCoverage.ContainsKey(ctxId))
            {
                _ctxToCoverage.TryGetValue(ctxId, out disp);
            }
            else
            {
                var testName = $"test_{ctxId}"; //TODO: real name is...????
                disp = _rep.CreateCoverageDispatcher(testName);
                _ctxToCoverage.TryAdd(ctxId, disp);
            }
            return disp;
        }

        private static int GetContextId()
        {
            return Thread.CurrentThread.ExecutionContext.GetHashCode();
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