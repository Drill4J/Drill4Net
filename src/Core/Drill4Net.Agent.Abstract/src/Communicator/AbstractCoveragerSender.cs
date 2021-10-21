using System;
using System.Collections.Generic;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCoveragerSender : IAgentCoveragerSender
    {
        private readonly Logger _logger;

        /********************************************************************************/

        protected AbstractCoveragerSender(string subsystem)
        {
            _logger = new TypedLogger<AbstractCoveragerSender>(subsystem);
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
        #region Session (managed on Agent side)
        public virtual void SendStartSessionCommand(string name)
        {
            //start the session
            //...
        }

        public virtual void SendStopSessionCommand(string name)
        {
            //stop the session
            //...
        }
        #endregion

        /// <summary>
        /// Send info about started test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void SendTestRunStart(TestCaseContext testCtx)
        {
            // https://kb.epam.com/display/EPMDJ/API+End+points+for+Back-end+admin+service
            // https://github.com/Drill4J/js-auto-test-agent/blob/master/src/admin-connect/index.ts

            var test = GetTestCaseName(testCtx);
            var metaData = GetTestCaseMetadata(testCtx);
            var info = new Test2RunInfo
            {
                name = test,
                startedAt = testCtx.StartTime,
                metadata = metaData,
            };
            //
            var _testRun = new TestRun
            {
                startedAt = testCtx.StartTime
            };
            _testRun.tests.Add(info);

            //send it
            //SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, _testRun);
        }

        /// <summary>
        /// Send info about finished test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void SendTestRunFinish(TestCaseContext testCtx)
        {
            var test = GetTestCaseName(testCtx);
            var metaData = GetTestCaseMetadata(testCtx);
            var info = new Test2RunInfo
            {
                name = test,
                startedAt = testCtx.StartTime,
                finishedAt = testCtx.FinishTime,
                result = testCtx.Result,
                metadata = metaData,
            };
            //
            var testRun = new TestRun
            {
                startedAt = testCtx.StartTime,
                finishedAt = testCtx.FinishTime
            };
            testRun.tests.Add(info);

            //send it
            //SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, _testRun);
        }

        internal string GetTestCaseName(TestCaseContext testCtx)
        {
            if(testCtx == null)
                throw new ArgumentNullException(nameof(testCtx));
            return testCtx.QualifiedName ?? testCtx.CaseName ?? testCtx.DisplayName;
        }

        internal Dictionary<string, object> GetTestCaseMetadata(TestCaseContext testCtx)
        {
            return new Dictionary<string, object> { { AgentConstants.KEY_TESTCASE_CONTEXT, testCtx } };
        }
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