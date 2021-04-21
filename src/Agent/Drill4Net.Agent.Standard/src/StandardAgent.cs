using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Transport;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent : AbstractAgent
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        private static readonly Dictionary<string, InjectedMethod> _pointToMethods;

        private static readonly ConcurrentDictionary<int, string> _execCtxToTestUids;
        private static readonly ConcurrentDictionary<string, int> _testUidToExecCtxs;
            
        private static ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _execCtxToExecData;

        private static readonly AgentReceiver _receiver;
        private static readonly AgentSender _sender;

        /*****************************************************************************/

        static StandardAgent()
        {
            PrepareLogger();
            Log.Debug("Initializing...");
            
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            
            //test names vs. session ids
            _execCtxToTestUids = new ConcurrentDictionary<int, string>();
            _testUidToExecCtxs = new ConcurrentDictionary<string, int>();
            
            // execution data by session ids
            _execCtxToExecData = new ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>>();

            try
            {
                var rep = new StandardAgentRepository();
                //
                var communicator = rep.GetCommunicator();
                _receiver = new(communicator);
                _receiver.SessionStarted += SessionStarted;
                _receiver.SessionFinished += SessionFinished;
                _receiver.SessionCancelled += SessionCancelled;
                _receiver.AllSessionsCancelled += AllSessionsCancelled;
                //
                _sender = new(communicator);

                //1. Classes to admin side
                _pointToMethods = rep.MapPointToMethods();
                var entities = rep.GetEntities();
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
                var asmName = ar[1];
                //var funcName = ar[2];
                var probe = ar[3];

                var businessMethod = GetBusinessMethodName(probeUid);
                if(businessMethod != null)
                    AddPoint(asmName, businessMethod, $"{probeUid}:{probe}"); //in the prod profiler only uid needed
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
        
        internal static void AddPoint(string asmName, string funcSig, string point)
        {
            var points = GetPoints(asmName, funcSig);
            points.Add(point);
        }
        
        public static List<string> GetPoints(string asmName, string funcSig, bool withPointRemoving = false)
        {
            var byFunctions = GetFunctions(!withPointRemoving);
            List<string> points;
            var funcPath = $"{asmName};{funcSig}";
            if (byFunctions.ContainsKey(funcPath))
            {
                points = byFunctions[funcPath];
            }
            else
            {
                points = new List<string>();
                byFunctions.Add(funcPath, points);
            }
            //
            if (withPointRemoving)
                byFunctions.Remove(funcPath);
            return points;
        }
        
        public static Dictionary<string, List<string>> GetFunctions(bool createNotExistedBranch)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var id = GetSessionId();
            Debug.WriteLine($"Profiler({createNotExistedBranch}): id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(id))
            {
                _clientPoints.TryGetValue(id, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if(createNotExistedBranch)
                    _clientPoints.TryAdd(id, byFunctions);
            }
            return byFunctions;
        }

        private static int GetSessionId()
        {
            return Thread.CurrentThread.ExecutionContext.GetHashCode();
        }

        internal static string GetBusinessMethodName(string probeUid)
        {
            if (_pointToMethods == null || !_pointToMethods.ContainsKey(probeUid))
                return null;
            return _pointToMethods[probeUid].BusinessMethod;
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