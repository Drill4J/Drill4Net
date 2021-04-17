using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard
{
    public class StandardProfiler : AbstractAgent
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        private static readonly Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static readonly Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;

        /*****************************************************************************/

        static StandardProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            PrepareLogger();
            Log.Debug("Initializing...");

            try
            {
                //rep
                var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_STD_NAME);
                var rep = new InjectorRepository(cfg_path);

                //tree info
                var tree = rep.ReadInjectedTree();
                _parentMap = tree.CalcParentMap();
                _pointMap = tree.CalcPointMap(_parentMap);

                Log.Debug("Initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error of {nameof(StandardProfiler)} initializing");
            }
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
            if (_pointMap == null)
                return null;
            if (!_pointMap.ContainsKey(probeUid))
                return null;
            if (_pointMap[probeUid] is not InjectedMethod method)
                return null;
            return method.BusinessMethod;
        }
        
        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            //common log
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(StandardProfiler)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}