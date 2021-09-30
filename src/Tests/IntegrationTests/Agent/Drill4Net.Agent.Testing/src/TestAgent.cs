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
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Core.Repository;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Testing.NetFxUtils;

namespace Drill4Net.Agent.Testing
{
    /// <summary>
    /// Profiler for the Tester Subsystem (integration tests for the checking 
    /// of correctness of the injections in the model assembly)
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AbstractAgent" />
    public class TestAgent : AbstractAgent
    {
        private const string CONTEXT_UNKNOWN = "unknown";
        private static ConcurrentDictionary<string, Dictionary<string, List<string>>> _clientPoints;
        private static readonly Dictionary<string, InjectedMethod> _pointToMethods;
        private static readonly Dictionary<int, string> _execIdToTestId;
        private static readonly BanderLog.Logger _logger;

        /*****************************************************************************/

        static TestAgent()
        {
            AbstractRepository.PrepareEmergencyLogger();
            _logger = new TypedLogger<TestAgent>(CoreConstants.SUBSYSTEM_TESTER);
            _logger.Debug("Initializing...");

            try
            {
                var domain = AppDomain.CurrentDomain;
                _pointToMethods = domain.GetData(nameof(_pointToMethods)) as Dictionary<string, InjectedMethod>;
                if (_pointToMethods == null)
                {
                    //rep
                    var callingDir = FileUtils.GetCallingDir();
                    var cfg_path = Path.Combine(callingDir, CoreConstants.CONFIG_TESTS_NAME);
                    var rep = new TestAgentRepository(cfg_path);

                    //tree info
                    var targetsDir = rep.GetTargetsDir(callingDir);
                    var treePath = Path.Combine(targetsDir, CoreConstants.TREE_FILE_NAME);
                    var tree = rep.ReadInjectedTree(treePath); //full tree data
                    //var version = CommonUtils.GetCallingTargetVersioning(); //current version (not correct for NetFx)
                    //var moniker = SdkHelper.ConvertTargetTypeToMoniker(version.RawVersion);
                    //var verTree = tree.GetFrameworkRootDirectory(moniker); //tree data for current version

                    _pointToMethods = tree.MapPointToMethods();
                    domain.SetData(nameof(_pointToMethods), _pointToMethods);

                    _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
                    domain.SetData(nameof(_clientPoints), _clientPoints);

                    _execIdToTestId = new Dictionary<int, string>();
                    domain.SetData(nameof(_execIdToTestId), _execIdToTestId);

                    _logger.Debug("Initialized.");
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Error of {nameof(TestAgent)} initializing", ex);
            }
        }

        /*****************************************************************************/

        #region Register
        /// <summary>
        ///  Registers the probe data from the injected Target app
        /// </summary>
        /// <param name="data"></param>
        public static void RegisterStatic(string data) //don't combine this signature with next one
        {
            RegisterWithContextStatic(data, null);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        ///  Registers the probe data from the injected Target app
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctx"></param>
        public static void RegisterWithContextStatic(string data, string ctx)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    _logger.Error("Data is empty", null);
                    return;
                }

                var ctxId = ctx ?? GetContextId();
                var ar = data.Split('^'); //data can contains some additional info in the debug mode
                var probeUid = ar[0];
                //var asmName = ar[1];
                //var funcName = ar[2];
                //var probe = ar[3];  
                AddPoint(ctxId, probeUid);
            }
            catch (Exception ex)
            {
                _logger.Error($"{data}", ex);
            }
        }

        /// <summary>
        ///Registers the probe data from the injected Target app  
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public override void RegisterWithContext(string data, string ctx)
        {
            RegisterWithContextStatic(data, ctx);
        }

        /// <summary>
        ///  Registers the probe data from the injected Target app
        /// </summary>
        /// <param name="data"></param>
        public override void Register(string data) //don't combine this signature with next one
        {
            RegisterStatic(data);
        }
        #endregion
        #region Context
        internal static string GetContextId()
        {
#if NETFRAMEWORK
            //What if the NUnit or Tester Engine comes here?
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

            //...and for NetCore tests NUnit uses AsyncLocal.
            //var lstFlds = typeof(ExecutionContext).GetFields();
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
                        if (_execIdToTestId == null)
                            return CONTEXT_UNKNOWN;

                        //This defines the logical execution path of function callers regardless
                        //of whether threads are created in async/await or Parallel.For
                        //It doesn't work very well on its own, at least not for everyone's version 
                        //of the framework.
                        var execId = Thread.CurrentThread.ExecutionContext.GetHashCode();

                        try
                        {
                            var testCtx = ctxFld.GetValue(lstFldVal) as TestExecutionContext;

                            var id = GetContextId(testCtx);
                            if (!_execIdToTestId.ContainsKey(execId))
                                _execIdToTestId.Add(execId, id);

                            return id;
                        }
                        catch
                        {
                            //here we will be, for example, for object's Finalizers
                            typeValMap = Type.GetType("System.Threading.AsyncLocalValueMap+OneElementAsyncLocalValueMap");
                            ctxFld = Array.Find(typeValMap
                                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "_value1");
                            //no context info about concrete test
                            var testCtx = ctxFld.GetValue(lstFldVal) as TestExecutionContext;
                            var testOutput = GetContextOutput(testCtx);

                            return _execIdToTestId.ContainsKey(execId) ? _execIdToTestId[execId] : CONTEXT_UNKNOWN; 
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
        
        internal static string GetContextOutput(TestExecutionContext ctx)
        {
            return ctx?.CurrentResult?.Output ?? CONTEXT_UNKNOWN;
        }
        #endregion
        #region Cross-points
        internal static void AddPoint(string ctxId, string pointUid)
        {
            if (_pointToMethods == null)
                throw new InvalidOperationException("Points' map is null");
            var method = _pointToMethods.ContainsKey(pointUid) ? _pointToMethods[pointUid] : null;
            if (method == null)
                return; //it's may be normal only for the init events of the Target on TesterEngine's side
            //
            var points = GetPoints(ctxId, method.AssemblyName, method.BusinessMethod);
            points.Add(pointUid);
        }

        /// <summary>
        /// Gets the probe cross-points of Target by Test Context, function signature, etc.
        /// </summary>
        /// <param name="ctxId">The context of test identifier.</param>
        /// <param name="asmName">Name of the assembly of function.</param>
        /// <param name="funcSig">The function signature.</param>
        /// <returns></returns>
        public static List<string> GetPoints(string ctxId, string asmName, string funcSig)
        {
            var byFunctions = GetMethods(true, ctxId);
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
            return points;
        }

        /// <summary>
        /// Gets all cross-points ignoring execution context.
        /// </summary>
        /// <param name="fullSig">The full function signature with assembly name is formed as $"{asmName};{funcSig}.</param>
        /// <returns>List of points</returns>
        public static List<string> GetPointsIgnoringContext(string fullSig)
        {
            var all = new List<string>();
            foreach (var funcs in _clientPoints.Values)
            {
                if (!funcs.ContainsKey(fullSig))
                    continue;
                all.AddRange(funcs[fullSig]);
                funcs.Remove(fullSig);
            }
            return all;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Gets list of the functions by current execution and logical contexts.
        /// </summary>
        /// <param name="createNotExistedBranch">If set to <c>true</c> create not existed branch for the current execution or logical context.</param>
        /// <param name="ctxId">Test context. If empty will try get current context</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetMethods(bool createNotExistedBranch, string ctxId = null)
        {
            if(string.IsNullOrWhiteSpace(ctxId))
                ctxId = GetContextId();

            if (_clientPoints == null && AppDomain.CurrentDomain.GetData(nameof(_clientPoints))
                    is ConcurrentDictionary<string, Dictionary<string, List<string>>> clientPoints)
            {
                _clientPoints = clientPoints;
            }

            if (_clientPoints == null)
                _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
            //
            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(ctxId))
            {
                _clientPoints.TryGetValue(ctxId, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if (createNotExistedBranch)
                    _clientPoints.TryAdd(ctxId, byFunctions);
            }
            return byFunctions;
        }

        ///// <summary>
        ///// Gets the business name of the tested method.
        ///// </summary>
        ///// <param name="pointUid">The cross-point's uid.</param>
        ///// <returns></returns>
        //internal static string GetBusinessMethodName(string pointUid)
        //{
        //    if (_pointToMethods == null)
        //    {
        //        if (AppDomain.CurrentDomain.GetData(nameof(_pointToMethods)) is Dictionary<string, InjectedMethod> pointToMethods)
        //            _pointToMethods = pointToMethods;
        //    }
        //    if (_pointToMethods?.ContainsKey(pointUid) != true)
        //        return null;
        //    return _pointToMethods[pointUid].BusinessMethod;
        //}
        #endregion

        //this method must exists due to common injection's logic
        //TODO: do something with it
        public static void DoCommand(int command, string data)
        {
        }
    }
}
