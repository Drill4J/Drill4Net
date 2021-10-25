using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCoveragerSender : IAgentCoveragerSender
    {
        private Test2RunInfo _firstTest2RunInfo;
        private readonly ConcurrentDictionary<string, Test2RunInfo> _testCaseCtxs;
        private readonly Logger _logger;

        /********************************************************************************/

        protected AbstractCoveragerSender(string subsystem)
        {
            _logger = new TypedLogger<AbstractCoveragerSender>(subsystem);
            _testCaseCtxs = new ConcurrentDictionary<string, Test2RunInfo>();
        }

        /********************************************************************************/

        #region Send messages
        #region Init
        /// <summary>
        /// "Send Scope Initialized" message ("SCOPE_INITIALIZED")
        /// </summary>
        public virtual void SendScopeInitialized(InitActiveScope scope, long ts)
        {
            _logger.Debug("Send ScopeInitialized");
            var load = scope.Payload;
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new ScopeInitialized(load.Id, load.Name, load.PrevId, ts));
        }
        
        /// <summary>
        /// "Agent is starting init process" message ("INIT")
        /// </summary>
        public virtual void SendInitMessage(int classesCount)
        {
            _logger.Debug($"Send init message. Class count={classesCount}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitInfo(classesCount));
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            _logger.Debug($"Send classes data. Count={entities?.Count}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitDataPart { astEntities = entities });
        }

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            _logger.Debug("Send Initialized message");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new Initialized { msg = "Initialized" }); //can be any string
        }
        #endregion
        #region Session (managed on Admin side)
        public virtual void SendSessionStartedMessage(string sessionUid, string testType, bool isRealTime, long ts)
        {
            var data = new SessionStarted
            {
                sessionId = sessionUid,
                isRealtime = isRealTime,
                testType = testType,
                ts = ts,
            };
            _logger.Debug($"Send SessionStarted message. {data}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, data);
        }

        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            _logger.Debug($"Send SessionFinished message. Session={sessionUid}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionFinished { sessionId = sessionUid, ts = ts });
        }

        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            _logger.Debug($"Send AllSessionFinished message. Sessions={string.Join(",", sessionUids)}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsFinished { ids = sessionUids, ts = ts });
        }

        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            _logger.Debug($"Send SessionCancelled message. Session={uid}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionCancelled { sessionId = uid, ts = ts });
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            _logger.Debug($"Send AllSessionCancelled message. Sessions={string.Join(",", uids)}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsCancelled { ids = uids, ts = ts });
        }
        #endregion
        #region Coverage
        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            //no logging here!
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new CoverDataPart { data = data, sessionId = sessionUid });
        }

        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            //no logging here!
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionChanged { sessionId = sessionUid, probeCount = probeCount });
        }
        #endregion
        #region Test2Run

        //https://kb.epam.com/display/EPMDJ/API+End+points+for+Back-end+admin+service
        //https://github.com/Drill4J/js-auto-test-agent/blob/master/src/admin-connect/index.ts

        #region Session (managed on Agent side)
        public virtual void SendStartSessionCommand(string sessionId)
        {
            ClearSessionData();
            StartSessionConcrete(AgentConstants.ADMIN_PLUGIN_NAME,
                sessionId,
                isRealtime: true, //?
                isGlobal: false);
        }

        public virtual void SendStopSessionCommand(string sessionUid)
        {
            StopSessionConcrete(AgentConstants.ADMIN_PLUGIN_NAME, sessionUid);
            ClearSessionData();
        }

        private void ClearSessionData()
        {
            _firstTest2RunInfo = null;
            _testCaseCtxs.Clear();
        }
        #endregion
        #region Test case data
        /// <summary>
        /// Send info about started test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void SendTestCaseStart(TestCaseContext testCtx)
        {
            var info = PrepareTest2RunInfo(testCtx);
            _testCaseCtxs.TryAdd(info.name, info);
            if (_firstTest2RunInfo == null)
                _firstTest2RunInfo = info;

            var testRun = new TestRun
            {
                startedAt = _firstTest2RunInfo.startedAt
            };
            testRun.tests.Add(info);

            //send it
            RegisterTestsRun(AgentConstants.ADMIN_PLUGIN_NAME, Serialize(testRun));
        }

        /// <summary>
        /// Send info about finished test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void SendTestCaseFinish(TestCaseContext testCtx)
        {
            string test = testCtx.GetKey();
            if (!_testCaseCtxs.TryGetValue(test, out Test2RunInfo info)) //it is bad
                info = PrepareTest2RunInfo(testCtx);
            info.finishedAt = testCtx.FinishTime;
            info.result = testCtx.Result;
            //
            var testRun = new TestRun
            {
                //but _firstTest2RunInfo == null is abnormal
                startedAt = _firstTest2RunInfo == null ? info.startedAt : _firstTest2RunInfo.startedAt,
                finishedAt = info.finishedAt
            };
            testRun.tests.Add(info);

            //send it
            RegisterTestsRun(AgentConstants.ADMIN_PLUGIN_NAME, Serialize(testRun));
        }

        internal Test2RunInfo PrepareTest2RunInfo(TestCaseContext testCtx)
        {
            var test = testCtx.GetKey();
            var metaData = GetTestCaseMetadata(testCtx);
            return new Test2RunInfo
            {
                name = test,
                startedAt = testCtx.StartTime,
                metadata = metaData,
            };
        }

        internal Dictionary<string, object> GetTestCaseMetadata(TestCaseContext testCtx)
        {
            return new Dictionary<string, object> { { AgentConstants.KEY_TESTCASE_CONTEXT, testCtx } };
        }
        #endregion
        #endregion
        #endregion
        #region Send API
        #region Send
        public void Send(string route, AbstractMessage message)
        {
            SendConcrete(message.type, route, Serialize(message));
        }
        
        protected abstract void SendConcrete(string messageType, string route, string message);
        #endregion
        #region SendToPlugin
        public void SendToPlugin(string pluginId, AbstractMessage message)
        {
            SendToPluginConcrete(pluginId, Serialize(message));
        }

        protected abstract void SendToPluginConcrete(string pluginId, string message);

        /// <summary>
        /// Register info about running tests.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="tests2Run"></param>
        public abstract void RegisterTestsRun(string pluginId, string tests2Run);
        protected abstract void StartSessionConcrete(string pluginId, string sessionId, bool isRealtime, bool isGlobal);
        protected abstract void StopSessionConcrete(string pluginId, string sessionId);
        #endregion

        protected abstract string Serialize(object message);

        #region Debug sendings
        public virtual void DebugSendOutgoingTest(OutgoingMessage data)
        {
        }
        
        public virtual void DebugSendOutgoingTest(string topic, OutgoingMessage data)
        {
        }
        
        public virtual void DebugSendIncomingTest(string topic, IncomingMessage message)
        {
        }
        #endregion
        #endregion
    }
}