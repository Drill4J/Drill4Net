using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework.Internal;
#if NETFRAMEWORK
using System.Runtime.Remoting.Messaging;
#endif
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;
using System.Threading;

namespace Drill4Net.Agent.Testing
{
    public class TesterProfiler : AbstractAgent
    {
        private static ConcurrentDictionary<string, Dictionary<string, List<string>>> _clientPoints;
        private static Dictionary<string, InjectedMethod> _pointToMethods;

        /*****************************************************************************/

        static TesterProfiler()
        {
            PrepareLogger();
            Log.Debug("Initializing...");

            try
            {
                var domain = AppDomain.CurrentDomain;
                _pointToMethods = domain.GetData("pointToMethods") as Dictionary<string, InjectedMethod>;
                if (_pointToMethods == null)
                {
                    //rep
                    var callingDir = FileUtils.GetCallingDir();
                    var cfg_path = Path.Combine(callingDir, CoreConstants.CONFIG_TESTS_NAME);
                    var rep = new TesterRepository(cfg_path);

                    //tree info
                    var targetsDir = rep.GetTargetsDir(callingDir);
                    var treePath = Path.Combine(targetsDir, CoreConstants.TREE_FILE_NAME);
                    var tree = rep.ReadInjectedTree(treePath);
                    _pointToMethods = tree.MapPointToMethods();
                    domain.SetData("pointToMethods", _pointToMethods);

                    _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
                    domain.SetData("clientPoints", _clientPoints);

                    Log.Debug("Initialized.");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error of {nameof(TesterProfiler)} initializing");
            }
        }

        /*****************************************************************************/

        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
                var logCtx = GetLogicalContext();
                if (logCtx == null)
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
                var asmName = ar[1];
                //var funcName = ar[2];
                var probe = ar[3];

                var businessMethod = GetBusinessMethodName(probeUid);
                if (businessMethod != null)
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

        internal static TestExecutionContext GetLogicalContext()
        {
#if NETFRAMEWORK
            var ret = CallContext.LogicalGetData("NUnit.Framework.TestExecutionContext") as TestExecutionContext;
            return ret;
#else
            return new TestExecutionContext(); // { CurrentTest = new Test { FullName = "NONE" } };
#endif
        }

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

        public static List<string> GetPointsIgnoringContext(string funcSig)
        {
            var all = new List<string>();
            foreach (var funcs in _clientPoints.Values)
            {
                if (!funcs.ContainsKey(funcSig))
                    continue;
                all.AddRange(funcs[funcSig]);
                funcs.Remove(funcSig);
            }
            return all;
        }

        public static Dictionary<string, List<string>> GetFunctions(bool createNotExistedBranch)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var id = Thread.CurrentThread.ExecutionContext.GetHashCode();
            //Debug.WriteLine($"Profiler({createNotExistedBranch}): id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            string method = id.ToString();
#if NETFRAMEWORK
            var logCtx = GetLogicalContext();
            method = logCtx?.CurrentTest?.FullName ?? "unknown";
#endif

            if (_clientPoints == null)
            {
                if (AppDomain.CurrentDomain.GetData("clientPoints") is ConcurrentDictionary<string, Dictionary<string, List<string>>> clientPoints)
                    _clientPoints = clientPoints;
            }
            if (_clientPoints == null)
                _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
            //
            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(method))
            {
                _clientPoints.TryGetValue(method, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if (createNotExistedBranch)
                    _clientPoints.TryAdd(method, byFunctions);
            }
            return byFunctions;
        }

        internal static string GetBusinessMethodName(string probeUid)
        {
            if (_pointToMethods == null)
            {
                if (AppDomain.CurrentDomain.GetData("pointToMethods") is Dictionary<string, InjectedMethod> pointToMethods)
                    _pointToMethods = pointToMethods;
            }
            if (_pointToMethods == null || !_pointToMethods.ContainsKey(probeUid))
                return null;
            return _pointToMethods[probeUid].BusinessMethod;
        }

        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            var file = Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\..\..\..\"), $"{nameof(TesterProfiler)}.log");
            cfg.WriteTo.File(file);
            Log.Logger = cfg.CreateLogger();
        }
    }
}
