using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Transport;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgentRepository : IAgentRepository
    {
        private readonly ConcurrentDictionary<int, string> _ctxToSession;
        private readonly ConcurrentDictionary<string, int> _sessionToCtx;
        private readonly ConcurrentDictionary<int, CoverageDispatcher> _ctxToDispatcher;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _ctxToExecData;

        private readonly TreeConverter _converter;
        private readonly IEnumerable<InjectedType> _injTypes;

        private readonly System.Timers.Timer _sendTimer;
        private readonly object _sendLocker;

        /**************************************************************************************/

        public StandardAgentRepository()
        {
            //test names vs. session ids
            _ctxToSession = new ConcurrentDictionary<int, string>();
            _sessionToCtx = new ConcurrentDictionary<string, int>();
            _ctxToDispatcher = new ConcurrentDictionary<int, CoverageDispatcher>();

            // execution data by session ids
            _ctxToExecData = new ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>>();

            //Injector rep
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_STD_NAME);
            var injRep = new InjectorRepository(cfg_path);
            _converter = new TreeConverter();
            _sendLocker = new object();

            //target class tree
            var tree = injRep.ReadInjectedTree();
            _injTypes = FilterTypes(tree);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(5000);
            _sendTimer.Elapsed += Timer_Elapsed;
        }

        /**************************************************************************************/

        #region Init
        public ICommunicator GetCommunicator()
        {
            return new Communicator();
        }

        internal IEnumerable<InjectedType> FilterTypes(InjectedSolution tree)
        {
            IEnumerable<InjectedType> injTypes = null;

            // check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = tree.GetDirectories();
            if (rootDirs.Count() > 1)
            {
                var asmNameByDirs = (from dir in rootDirs
                                     select dir.GetAssemblies()
                                               .Select(a => a.Name)
                                               .Where(a => a.EndsWith(".dll"))
                                               .ToList())
                                     .ToList();
                if (asmNameByDirs[0].Count > 0)
                {
                    var multi = true;
                    for (var i = 1; i < asmNameByDirs.Count; i++)
                    {
                        var prev = asmNameByDirs[i - 1];
                        var cur = asmNameByDirs[i];
                        if (prev.Count != cur.Count || prev.Intersect(cur).Count() == 0)
                        {
                            multi = false;
                            break;
                        }
                    }
                    if (multi) //here many copies of target for diferent runtimes
                    {
                        var execVer = CommonUtils.GetEntryTargetVersioning();
                        InjectedDirectory targetDir = null;
                        foreach (var dir in rootDirs)
                        {
                            var asms = dir.GetAssemblies().ToList();
                            if (asms[0].Version.Version != execVer.Version)
                                continue;
                            targetDir = dir;
                            break;
                        }
                        injTypes = targetDir.GetAssemblies().SelectMany(a => a.GetAllTypes());
                    }
                }
            }
            else
            {
                injTypes = tree.GetAllTypes();
            }
            injTypes = injTypes.Where(a => !a.IsCompilerGenerated);
            return injTypes;
        }

        public List<AstEntity> GetEntities()
        {
            return _converter.ToAstEntities(_injTypes);
        }

        public CoverageDispatcher CreateCoverageDispatcher(string testName)
        {
            return _converter.CreateCoverageDispatcher(testName, _injTypes);
        }
        #endregion
        #region Session
        #region Started
        public void SessionStarted(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            RemoveSession(sessionUid);
            AddSession(sessionUid);
            StartSendCycle();
        }

        internal void AddSession(string sessionUid)
        {
            var ctxId = GetContextId();
            if (!_ctxToSession.ContainsKey(ctxId))
                return;
            _ctxToSession.TryAdd(ctxId, sessionUid);
        }
        #endregion
        #region Finished
        public void SessionStop(string sessionUid, long finishTime)
        {
            //TODO: send remaining data... ??

            //removing
            RemoveSession(sessionUid);
            StopSendCycleIfNeeded();
        }

        internal void RemoveSession(string sessionUid)
        {
            if (!_sessionToCtx.ContainsKey(sessionUid))
                return;
            //
            var ctxId = _sessionToCtx[sessionUid];
            _ctxToSession.TryRemove(ctxId, out var _);
            _ctxToExecData.TryRemove(ctxId, out var _);
            _ctxToDispatcher.TryRemove(ctxId, out var _);
        }
        #endregion
        #region Cancelled
        private void AllSessionsCancelled()
        {
            throw new NotImplementedException();
        }

        private void SessionCancelled(string sessionUid, long cancelTime)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
        #region Send coverage data
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendCoverages();
        }

        public void SendCoverages()
        {
            lock (_sendLocker)
            {

            }
        }
        #endregion

        internal void StartSendCycle()
        {
            _sendTimer.Enabled = true;
        }

        internal void StopSendCycleIfNeeded()
        {
            if(_ctxToDispatcher.Count == 0)
                _sendTimer.Enabled = false;
        }

        public CoverageDispatcher GetCoverageDispather()
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var ctxId = GetContextId();
            Debug.WriteLine($"Profiler: id={ctxId}, trId={Thread.CurrentThread.ManagedThreadId}");

            CoverageDispatcher disp;
            if (_ctxToDispatcher.ContainsKey(ctxId))
            {
                _ctxToDispatcher.TryGetValue(ctxId, out disp);
            }
            else
            {
                var testName = $"test_{ctxId}"; //TODO: real name is...????
                disp = CreateCoverageDispatcher(testName);
                _ctxToDispatcher.TryAdd(ctxId, disp);
            }
            return disp;
        }

        public int GetContextId()
        {
            return Thread.CurrentThread.ExecutionContext.GetHashCode();
        }
    }
}
