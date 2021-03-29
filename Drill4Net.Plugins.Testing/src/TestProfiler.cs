﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Drill4Net.Injector.Core;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.Testing
{
    public class TestProfiler : AbsractPlugin
    {
        public static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        public static readonly ConcurrentDictionary<int, string> _lastFuncByCtx;
        private static readonly ConcurrentDictionary<MethodInfo, string> _parentByInfo;
        private static readonly ConcurrentDictionary<string, MethodInfo> _infoBySig; // <string, byte> ?
        private static readonly ConcurrentDictionary<string, string> _displaySigs;

        private const string DISPLAY_CLASS = "c__DisplayClass";

        private static readonly InjectedSolution _tree;
        private static readonly Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            _parentByInfo = new ConcurrentDictionary<MethodInfo, string>();
            _infoBySig = new ConcurrentDictionary<string, MethodInfo>();
            _lastFuncByCtx = new ConcurrentDictionary<int, string>();
            _displaySigs = new ConcurrentDictionary<string, string>();

            //rep
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);
            var rep = new InjectorRepository(cfg_path);
            var opts = rep.Options;

            //tree info
            var targetDir = opts.Destination.Directory;
            var treeHintPath = rep.GetTreeFileHintPath(targetDir);
            var treePath = File.ReadAllText(treeHintPath);
            _tree = rep.ReadInjectedTree(treePath);
            _parentMap = _tree.CalcParentMap();
            _pointMap = _tree.CalcPointMap(_parentMap);
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log("Data is empty");
                    return;
                }
                //
                var ar = data.Split('^');
                if (ar.Length < 5)
                {
                    Log($"Bad format of input: {data}");
                    return;
                }
                #endregion

                var realmethodName = ar[0];
                var asmName = ar[1];
                var funcName = ar[2];
                var probeUid = ar[3];
                var probe = ar[4];

                //if (ClarifyBusinessMethodName(asmName, realmethodName, ref funcName))
                var businessMethod = GetBusinessMethodName(probeUid);
                if(businessMethod != null)
                    AddPoint(asmName, businessMethod, $"{probeUid}:{probe}"); //in the prod profiler only uid needed
            }
            catch (Exception ex)
            {
                Log($"{data}\n{ex}");
            }
        }

        internal static void Log(string mess)
        {
            //...
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
            Dictionary<string, List<string>> byFunctions = GetFunctions(!withPointRemoving);
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
                if (funcs.ContainsKey(funcSig))
                {
                    all.AddRange(funcs[funcSig]);
                    funcs.Remove(funcSig);
                }
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
            var point = _pointMap[probeUid] as CrossPoint;
            var method = _parentMap[point] as InjectedMethod;
            return method.FromMethod ?? method.Fullname;
        }
    }
}
