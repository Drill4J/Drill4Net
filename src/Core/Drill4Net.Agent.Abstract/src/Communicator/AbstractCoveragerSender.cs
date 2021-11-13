using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCoveragerSender : IAgentCoveragerSender
    {
        private string _test2RunSessionId;
        private long _startTestTime;
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
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new ScopeInitialized(load.Id, load.Name, load.PrevId, ts));
        }

        /// <summary>
        /// "Agent is starting init process" message ("INIT")
        /// </summary>
        public virtual void SendInitMessage(int classesCount)
        {
            _logger.Debug($"Send init message. Class count={classesCount}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitInfo(classesCount));
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            _logger.Debug($"Send classes data. Count={entities?.Count}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitDataPart { astEntities = entities });
        }

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            _logger.Debug("Send Initialized message");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new Initialized { msg = "Initialized" }); //can be any string
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
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, data);
        }

        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            _logger.Debug($"Send SessionFinished message. Session={sessionUid}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionFinished { sessionId = sessionUid, ts = ts });
        }

        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            _logger.Debug($"Send AllSessionFinished message. Sessions={string.Join(",", sessionUids)}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsFinished { ids = sessionUids, ts = ts });
        }

        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            _logger.Debug($"Send SessionCancelled message. Session={uid}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionCancelled { sessionId = uid, ts = ts });
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            _logger.Debug($"Send AllSessionCancelled message. Sessions={string.Join(",", uids)}");
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsCancelled { ids = uids, ts = ts });
        }
        #endregion
        #region Scope
        public virtual void SendFinishScopeAction()
        {
            var data = new SwitchActiveScope
            {
                payload = new ActiveScopeChangePayload()
                {
                    forceFinish = true,
                    prevScopeEnabled = true,
                    savePrevScope = true,
                    scopeName = "",
                }
            };
            _logger.Debug($"Send FinishScope action. {data}");
            SendActionToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, data);
        }
        #endregion
        #region Coverage
        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            //no logging here!
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new CoverDataPart { data = data, sessionId = sessionUid });
        }

        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            //no logging here!
            SendMessageToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionChanged { sessionId = sessionUid, probeCount = probeCount });
        }
        #endregion
        #region Test2Run
        //https://kb.epam.com/display/EPMDJ/API+End+points+for+Back-end+admin+service
        //https://github.com/Drill4J/js-auto-test-agent/blob/master/src/admin-connect/index.ts
        //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints

        #region Session (managed on Agent side)
        public virtual void SendStartSessionCommand(string sessionId)
        {
            ClearAutoSessionData();
            StartSessionConcrete(AgentConstants.ADMIN_PLUGIN_NAME,
                sessionId,
                isRealtime: true, //?
                isGlobal: false);
            _test2RunSessionId = sessionId;
        }

        public virtual void SendStopSessionCommand(string sessionUid)
        {
            StopSessionConcrete(AgentConstants.ADMIN_PLUGIN_NAME, sessionUid);
            ClearAutoSessionData();
        }

        private void ClearAutoSessionData()
        {
            _startTestTime = 0;
            _test2RunSessionId = null;
            _testCaseCtxs.Clear();
        }
        #endregion
        #region Test case data
        private int _testCaseCnt;

        /// <summary>
        /// Send info about started test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void RegisterTestCaseStart(TestCaseContext testCtx)
        {
            var info = PrepareTest2RunInfo(testCtx);
            _testCaseCtxs.TryAdd(info.name, info);
            if (_startTestTime == 0)
                _startTestTime = info.startedAt;

            _logger.Debug($"Test cases count: {++_testCaseCnt}");
        }

        /// <summary>
        /// Send info about finished test to the admin side
        /// </summary>
        /// <param name="testCtx"></param>
        public virtual void RegisterTestCaseFinish(TestCaseContext testCtx)
        {
            //executed test case
            string test = testCtx.GetKey();
            if (!_testCaseCtxs.TryRemove(test, out Test2RunInfo info)) //it is bad
                info = PrepareTest2RunInfo(testCtx);
            info.result = testCtx.Result ?? nameof(TestResult.UNKNOWN);
            info.finishedAt = testCtx.FinishTime;
            _logger.Debug($"Test name for Admin service: [{info.name}]");
            
            //test run
            var testRun = new TestRun
            {
                startedAt = _startTestTime == 0 ? info.startedAt : _startTestTime,
                finishedAt = info.finishedAt
            };
            testRun.tests.Add(info);

            //send it
            var message = new TestRunMessage(_test2RunSessionId, testRun);
            RegisterTestsRunConcrete(AgentConstants.ADMIN_PLUGIN_NAME, Serialize(message));

            ////TEST!!!
            //var testCtx2 = new TestCaseContext()
            //{
            //    CaseName = $"TestCase_{_testCaseCnt}",
            //    IsFinished = true,
            //    Result = "FAILED",
            //    StartTime = info.startedAt,
            //    FinishTime = info.finishedAt,
            //    AssemblyPath = @"c:\\test.dll"
            //};
            //var info2 = PrepareTest2RunInfo(testCtx2);
            //testRun.tests.Clear();
            //testRun.tests.Add(info2);
            //message = new TestRunMessage(_test2RunSessionId, testRun);
            //RegisterTestsRunConcrete(AgentConstants.ADMIN_PLUGIN_NAME, Serialize(message));
        }

        internal Test2RunInfo PrepareTest2RunInfo(TestCaseContext testCtx)
        {
            var test = testCtx.GetKey();
            var(type, method) = GetNames(testCtx);
            var metaData = GetTestCaseMetadata(testCtx, test);

            var info = new TestName
            {
                engine = GetFullEngineName(testCtx),
                className = type,
                method = method,
                //classParams =  // ??
                methodParams = GetMethodParams(testCtx.CaseName), // TODO: +??
            };
            return new Test2RunInfo(test, info, testCtx.StartTime, testCtx.Result ?? nameof(TestResult.UNKNOWN), metaData);
        }

        internal string GetFullEngineName(TestCaseContext testCtx)
        {
            var engine = ".NET";
            if (!string.IsNullOrWhiteSpace(testCtx.Engine))
                engine += "/" + testCtx.Engine;
            if (testCtx.Engine != testCtx.Adapter && !string.IsNullOrWhiteSpace(testCtx.Adapter))
                engine += "/" + testCtx.Adapter;
            return engine;
        }

        internal (string type, string method) GetNames(TestCaseContext testCtx)
        {
            var qName = testCtx.QualifiedName;
            if (qName.Contains(".")) //normal full method name
                return Common.CommonUtils.DeconstructFullMethodName(qName);
            return (testCtx.Group.Replace("/", "."), qName); //Group is full type namme with "/", and qName is short method name
        }

        internal string GetMethodParams(string testCase)
        {
            if (string.IsNullOrWhiteSpace(testCase))
                return null;
            var ind = testCase.IndexOf('(');
            if (ind == -1)
                return null;
            return testCase.Substring(ind, testCase.Length - ind - 2);
        }

        internal Dictionary<string, string> GetTestCaseMetadata(TestCaseContext testCtx, string testNameForAdmin)
        {
            return new Dictionary<string, string>
            {
                { AgentConstants.KEY_TESTCASE_TESTNAME_FOR_ADMIN, testNameForAdmin },
                { AgentConstants.KEY_TESTCASE_CONTEXT, JsonConvert.SerializeObject(testCtx) },
            };
        }
        #endregion
        #endregion
        #endregion
        #region Send API
        #region Send
        public void Send(string route, AbstractMessage message)
        {
            SendMessageConcrete(message.type, route, Serialize(message));
        }

        protected abstract void SendMessageConcrete(string messageType, string route, string message);
        #endregion
        #region SendToPlugin
        public void SendMessageToPlugin(string pluginId, AbstractMessage message)
        {
            SendMessageToPluginConcrete(pluginId, Serialize(message));
        }

        public void SendActionToPlugin(string pluginId, AbstractMessage message)
        {
            SendActionToPluginConcrete(pluginId, Serialize(message));
        }

        protected abstract void SendMessageToPluginConcrete(string pluginId, string message);
        protected abstract void SendActionToPluginConcrete(string pluginId, string message);

        /// <summary>
        /// Register info about running tests.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="tests2Run"></param>
        public abstract void RegisterTestsRunConcrete(string pluginId, string tests2Run);
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