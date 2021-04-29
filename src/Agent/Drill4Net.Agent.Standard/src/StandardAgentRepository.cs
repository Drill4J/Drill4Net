using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Agent.Transport;

namespace Drill4Net.Agent.Standard
{
    public sealed class StandardAgentRepository : IAgentRepository
    {
        public AbstractCommunicator Communicator { get; }

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
            Communicator = GetCommunicator();

            //target class tree
            var tree = injRep.ReadInjectedTree();
            _injTypes = FilterTypes(tree);

            //timer for periodically sending coverage data to admin side
            _sendTimer = new System.Timers.Timer(2000);
            _sendTimer.Elapsed += Timer_Elapsed;
        }

        /**************************************************************************************/

        #region Init
        private AbstractCommunicator GetCommunicator()
        {
            return new Communicator(GetBaseAddress(), GetAgentPartConfig());
        }

        internal string GetBaseAddress()
        {
            //TODO: real from cfg
            //return "ws://localhost:8090/agent/attach";
            return "localhost:8090";
        }

        internal AgentPartConfig GetAgentPartConfig()
        {
            return new AgentPartConfig("PROG_A", "0.0.1");
        }

        internal IEnumerable<InjectedType> FilterTypes(InjectedSolution tree)
        {
            IEnumerable<InjectedType> injTypes = null;

            // check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = tree.GetDirectories().ToList();
            if (rootDirs.Count > 1)
            {
                //investigate the versionable copies of target
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
                        if (prev.Count == cur.Count && prev.Intersect(cur).Any()) //the same structure
                            continue;
                        multi = false;
                        break;
                    }
                    if (multi) 
                    {
                        //here many copies of target for diferent runtimes
                        //we need only one
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
                        injTypes = targetDir?.GetAssemblies().SelectMany(a => a.GetAllTypes());
                    }
                }
            }
            else
            {
                injTypes = tree.GetAllTypes();
            }
            injTypes = injTypes?.Where(a => !a.IsCompilerGenerated);
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
        public void StartSession(StartAgentSession info)
        {
            var uid = info.Payload.SessionId;
            RemoveSession(uid);
            AddSession(uid);
            StartSendCycle();
        }

        internal void AddSession(string sessionUid)
        {
            var ctxId = GetContextId();
            if (_ctxToSession.ContainsKey(ctxId))
                return;
            _ctxToSession.TryAdd(ctxId, sessionUid);
            _sessionToCtx.TryAdd(sessionUid, ctxId);
        }
        #endregion
        #region Stop
        public void SessionStop(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            
            //send remaining data
            SendCoverages();

            //removing session/data
            RemoveSession(uid);
            StopSendCycleIfNeeded();
        }

        internal void RemoveSession(string sessionUid)
        {
            if (!_sessionToCtx.ContainsKey(sessionUid))
                return;
            //
            var ctxId = _sessionToCtx[sessionUid];
            _ctxToSession.TryRemove(ctxId, out var _);
            _sessionToCtx.TryRemove(sessionUid, out var _);
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

        internal void SendCoverages()
        {
            lock (_sendLocker)
            {
                foreach (var ctxId in _ctxToDispatcher.Keys)
                {
                    var sessionUid = _ctxToSession[ctxId];
                    var disp = _ctxToDispatcher[ctxId];
                    var execClasses = disp.PointToClass.Values.ToList();
                    var cnt = execClasses.Count();
                    switch (cnt)
                    {
                        case 0:
                            return;
                        case > 65535:
                            //TODO: implement by cycles
                            break;
                        default:
                            Communicator.Sender.SendCoverageData(sessionUid, execClasses);
                            break;
                    }
                }
            }
        }
        #endregion

        private void StartSendCycle()
        {
            _sendTimer.Enabled = true;
        }

        private void StopSendCycleIfNeeded()
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
                var testName = $"{AgentConstants.TEST_NAME_DEFAULT}_{ctxId}"; //TODO: real name is...????
                disp = CreateCoverageDispatcher(testName);
                _ctxToDispatcher.TryAdd(ctxId, disp);
            }
            return disp;
        }

        public int GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode() ?? 0;
        }

        public void CancelSession(CancelAgentSession info)
        {
            
        }

        public List<string> CancelAllSessions()
        {
            var uids = _sessionToCtx.Keys.ToList();
            // TODO: cancel
            return uids;
        }

        public List<string> StopAllSessions()
        {
            var uids = _sessionToCtx.Keys.ToList();
            // TODO: stopping
            return uids;
        }
    }
}
