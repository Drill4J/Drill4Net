﻿using System;
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
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Agent.Transport;

namespace Drill4Net.Agent.Standard
{
    public sealed class StandardAgentRepository : IAgentRepository
    {
        public AbstractCommunicator Communicator { get; }

        private readonly ConcurrentDictionary<int, string> _ctxToSession;
        private readonly ConcurrentDictionary<string, int> _sessionToCtx;
        private readonly ConcurrentDictionary<string, StartSessionPayload> _sessionToObject;
        private readonly ConcurrentDictionary<int, CoverageDispatcher> _ctxToDispatcher;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ExecClassData>> _ctxToExecData;

        private readonly TreeConverter _converter;
        private readonly IEnumerable<InjectedType> _injTypes;

        private readonly System.Timers.Timer _sendTimer;
        private readonly object _sendLocker;
        private bool _inTimer;

        /**************************************************************************************/

        public StandardAgentRepository()
        {
            //test names vs. session ids
            _ctxToSession = new ConcurrentDictionary<int, string>();
            _ctxToDispatcher = new ConcurrentDictionary<int, CoverageDispatcher>();
            
            //session maps
            _sessionToCtx = new ConcurrentDictionary<string, int>();
            _sessionToObject = new ConcurrentDictionary<string, StartSessionPayload>();
            
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
                        //here many copies of target for different runtimes
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
        #endregion
        #region Session
        #region Start
        public void StartSession(StartAgentSession info)
        {
            var load = info.Payload;
            RemoveSession(load.SessionId);
            AddSession(load);
            StartSendCycle();
        }

        internal void AddSession(StartSessionPayload session)
        {
            var ctxId = GetContextId();
            if (_ctxToSession.ContainsKey(ctxId))
                return;
            var sessionUid = session.SessionId;
            _sessionToObject.TryAdd(sessionUid, session);
            _ctxToSession.TryAdd(ctxId, sessionUid);
            _sessionToCtx.TryAdd(sessionUid, ctxId);
        }
        #endregion
        #region Stop
        public List<string> StopAllSessions()
        {
            StopSendCycle();
            SendCoverages();
            var uids = _sessionToCtx.Keys.ToList();
            ClearScopeData();
            return uids;
        }
        
        public void SessionStop(StopAgentSession info)
        {
            var uid = info.Payload.SessionId;
            
            //send remaining data
            SendCoverages();

            //removing session/data
            RemoveSession(uid);
            StopSendCycleIfNeeded();
        }
        #endregion
        #region Cancel
        public void CancelSession(CancelAgentSession info)
        {
            RemoveSession(info.Payload.SessionId);
        }

        public List<string> CancelAllSessions()
        {
            StopSendCycle();
            var uids = _sessionToCtx.Keys.ToList();
            ClearScopeData();
            return uids;
        }
        #endregion

        internal void RemoveSession(string sessionUid)
        {
            if (!_sessionToCtx.TryRemove(sessionUid, out var ctxId)) 
                return;
            _ctxToSession.TryRemove(ctxId, out var _);
            _ctxToExecData.TryRemove(ctxId, out var _);
            _ctxToDispatcher.TryRemove(ctxId, out var _);
            _sessionToObject.TryRemove(sessionUid, out var _);
        }
        
        internal void ClearScopeData()
        {
            _ctxToSession.Clear();
            _sessionToCtx.Clear();
            _ctxToExecData.Clear();
            _ctxToDispatcher.Clear();
            _sessionToObject.Clear();
        }
        #endregion
        #region Coverage data
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_inTimer)
                return;
            _inTimer = true;
            try
            {
                SendCoverages();
            }
            catch { } //TODO: log?
            finally
            {
                _inTimer = false;
            }
        }

        internal void SendCoverages()
        {
            lock (_sendLocker)
            {
                foreach (var ctxId in _ctxToDispatcher.Keys)
                {
                    if(!_ctxToDispatcher.TryGetValue(ctxId, out var disp))
                        continue;
                    if (!_ctxToSession.TryGetValue(ctxId, out var sessionUid))
                        continue;
                    //
                    var execClasses = disp.AffectedExecClasses.ToList();
                    var cnt = execClasses.Count();
                    switch (cnt)
                    {
                        case 0:
                            return;
                        case > 65535:
                            //TODO: implement in cycle by chunk
                            break;
                        default:
                            Communicator.Sender.SendCoverageData(sessionUid, execClasses);
                            break;
                    }
                    var session = disp.Session;
                    if (session is {IsRealtime: true})
                        Communicator.Sender.SendSessionChangedMessage(sessionUid, 0); //TODO: REAL COUNTS!!!!
                    disp.AffectedExecClasses.Clear();
                }
            }
        }

        private void StartSendCycle()
        {
            _sendTimer.Enabled = true;
        }
        
        private void StopSendCycle()
        {
            _sendTimer.Enabled = false;
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
                //TODO: do it properly! Need right binding ctx to session!
                var session = _sessionToObject.Values.FirstOrDefault(a => a.TestType == AgentConstants.TEST_MANUAL);
                disp = CreateCoverageDispatcher(session);
                _ctxToDispatcher.TryAdd(ctxId, disp);
            }
            return disp;
        }
        
        internal CoverageDispatcher CreateCoverageDispatcher(StartSessionPayload session)
        {
            return _converter.CreateCoverageDispatcher(session, _injTypes);
        }
        #endregion
        
        internal int GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode() ?? 0;
        }
    }
}
