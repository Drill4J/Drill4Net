using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Runtime.Remoting.Messaging;
#endif
using System.Linq;
using System.Threading;
using System.Reflection;
using NUnit.Framework.Internal;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Testing.NetFxUtils;

namespace Drill4Net.Agent.Testing
{
    public class TesterProfiler : AbstractAgent
    {
        private const string CONTEXT_UNKNOWN = "unknown";
        private static ConcurrentDictionary<string, Dictionary<string, List<string>>> _clientPoints;
        private static Dictionary<string, InjectedMethod> _pointToMethods;
        private static Dictionary<int, string> _execIdToMethodOutput;

        /*****************************************************************************/

        static TesterProfiler()
        {
            PrepareLogger();
            Log.Debug("Initializing...");

            try
            {
                var domain = AppDomain.CurrentDomain;
                _pointToMethods = domain.GetData(nameof(_pointToMethods)) as Dictionary<string, InjectedMethod>;
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
                    domain.SetData(nameof(_pointToMethods), _pointToMethods);

                    _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
                    domain.SetData(nameof(_clientPoints), _clientPoints);

                    _execIdToMethodOutput = new Dictionary<int, string>();
                    domain.SetData(nameof(_execIdToMethodOutput), _execIdToMethodOutput);

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
                var logCtx = GetContextId();
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

        internal static string GetContextId()
        {
#if NETFRAMEWORK
            //What if NUnit or Tester Engine comes here?
            var testCtx = LogicalContextManager.GetTestFromLogicalContext();
            return GetContextId(testCtx);
#else
            // the Tester Engine will be on NetCore version - and for NetFx's tests, and for NetCore ones 
            try
            {
                //try load old CallContext type - for NetFx successfully
                var testCtx = LogicalContextManager.GetTestFromLogicalContext();
                if (testCtx != null)
                    return GetContextId(testCtx);
            }
            catch { } //it's normal under the NetCore

            //...and for NetCore tests NUnit uses AsyncLocal
            var lstFld = Array.Find(typeof(ExecutionContext)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "m_localValues");
            if (lstFld != null)
            {
                var lstFldVal = lstFld.GetValue(Thread.CurrentThread.ExecutionContext);
                if (lstFldVal != null)
                {
                    //don't cache... (TODO: check it again)
                    var typeValMap = Type.GetType("System.Threading.AsyncLocalValueMap+ThreeElementAsyncLocalValueMap");
                    var ctxFld = Array.Find(typeValMap
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "_value3");
                    if (ctxFld != null)
                    {
                        try
                        {
                            var testCtx = ctxFld.GetValue(lstFldVal) as TestExecutionContext;
                            return GetContextId(testCtx);
                        }
                        catch 
                        {
                            //here we will be, for example, for object's Finalizers
                            typeValMap = Type.GetType("System.Threading.AsyncLocalValueMap+OneElementAsyncLocalValueMap");
                            ctxFld = Array.Find(typeValMap
                                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "_value1");
                            //no context info about concrete test
                            var testCtx = ctxFld.GetValue(lstFldVal) as TestExecutionContext;
                            var testOutput = testCtx?.CurrentResult?.Output ?? CONTEXT_UNKNOWN;

                            //This defines the logical execution path of function callers regardless
                            //of whether threads are created in async/await or Parallel.For
                            //It doesn't work very well on its own, at least not for everyone's version 
                            //of the framework.
                            var execId = Thread.CurrentThread.ExecutionContext.GetHashCode();
                            if(_execIdToMethodOutput.ContainsKey(execId))
                                _execIdToMethodOutput.Add(execId, testOutput);

                            return GetContextId(testCtx);
                        }
                    }
                }
            }
            return CONTEXT_UNKNOWN;
#endif
        }

        internal static string GetContextId(TestExecutionContext ctx)
        {
            return ctx?.CurrentTest?.FullName ?? CONTEXT_UNKNOWN;
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

        /// <summary>
        /// Gets list of the functions by current execution and logical contexts.
        /// </summary>
        /// <param name="createNotExistedBranch">If set to <c>true</c> create not existed branch for the current execution or logical context.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetFunctions(bool createNotExistedBranch)
        {
            //Debug.WriteLine($"Profiler({createNotExistedBranch}): id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            var id = GetContextId();

            if (_clientPoints == null && 
                AppDomain.CurrentDomain.GetData(nameof(_clientPoints)) is ConcurrentDictionary<string, Dictionary<string, List<string>>> clientPoints)
                _clientPoints = clientPoints;
            if (_clientPoints == null)
                _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
            //
            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(id))
            {
                _clientPoints.TryGetValue(id, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if (createNotExistedBranch)
                    _clientPoints.TryAdd(id, byFunctions);
            }
            return byFunctions;
        }

        internal static string GetBusinessMethodName(string probeUid)
        {
            if (_pointToMethods == null)
            {
                if (AppDomain.CurrentDomain.GetData(nameof(_pointToMethods)) is Dictionary<string, InjectedMethod> pointToMethods)
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
