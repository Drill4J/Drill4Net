using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Core;

namespace Drill4Net.Agent.Testing
{
    public class TestingProfiler : AbstractAgent
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        private static readonly InjectedSolution _tree;
        private static readonly Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static readonly Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;

        /*****************************************************************************/

        static TestingProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            PrepareLogger();

            //rep
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);
            var rep = new InjectorRepository(cfg_path);
            var opts = rep.Options;

            //tree info
            var targetDir = opts.Destination.Directory;
            var treePath = rep.GenerateTreeFilePath(targetDir);
            _tree = rep.ReadInjectedTree(treePath);
            _parentMap = _tree.CalcParentMap();
            _pointMap = _tree.CalcPointMap(_parentMap);
        }

        /*****************************************************************************/

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
                if (ar.Length < 5)
                {
                    Log.Error($"Bad format of input: {data}");
                    return;
                }
                #endregion

                //var realmethodName = ar[0];
                var asmName = ar[1];
                var funcName = ar[2];
                var probeUid = ar[3];
                var probe = ar[4];

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

        internal static string GetBusinessMethodName(string probeUid)
        {
            if (!_pointMap.ContainsKey(probeUid))
                return null;
            if (_pointMap[probeUid] is not CrossPoint point)
                return null;
            if (_parentMap[point] is not InjectedMethod method)
                return null;
            return method.BusinessMethod;
        }

        #region Logger
        public static void PrepareLogger()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.File(GetLogPath())
               .CreateLogger();
        }

        public static string GetLogPath()
        {
            return Path.Combine(FileUtils.GetExecutionDir(), "logs", "log.txt");
        }
        #endregion
    }
}
