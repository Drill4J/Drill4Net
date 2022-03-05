using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Plugins.NUnit3
{
    //https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html

    public class NUnitContexter : AbstractEngineContexter
    {
        private readonly ConcurrentDictionary<string, string> _method2ctxs;

        /*********************************************************************/

        public NUnitContexter() : base(nameof(NUnitContexter))
        {
            _method2ctxs = new();
        }

        /*********************************************************************/

        public override string GetContextId()
        {
            return GetContextIdInner(true).Result;
        }

        private async Task<string> GetContextIdInner(bool canWait)
        {
            var test = NUnit.Framework.TestContext.CurrentContext?.Test;
            if (test?.MethodName == null)
                return null;
            var method = test.FullName;
            if (method?.Contains("Internal.TestExecutionContext+") == true) //in fact, NUnit's context is absent
                return null;
            var timeout = DateTime.Now.AddSeconds(3);
            while (true)
            {
                if (_method2ctxs.TryGetValue(method, out var context)) //context == test case for autotests
                {
                    return context;
                }
                else
                {
                    if (!canWait)
                    {
                        return method;
                    }
                    else
                    {
                        if (DateTime.Now > timeout)
                            return method;
                        await Task.Delay(5);
                    }
                }
            }
        }

        public override TestEngine GetTestEngine()
        {
            return new TestEngine
            {
                Name = "NUnit",
                Version = FileUtils.GetProductVersion(typeof(NUnit.Framework.TestContext)),
                MustSequential = false,
            };
        }

        public override (bool Res, object Answer) RegisterCommand(int command, string data)
        {
            if (!_comTypes.Contains(command))
                return (false, null);
            if (string.IsNullOrWhiteSpace(data))
                return (true, null);
            var method = GetContextIdInner(false).Result; // in fact, initially it will be real method name, not useful context (test case name, etc)
            if(method == null)
                return (true, null); //true is normal - the context is not for NUnit
            //
            TestCaseContext testCaseCtx = null;
            switch ((AgentCommandType)command)
            {
                case AgentCommandType.TEST_CASE_START:
                    testCaseCtx = GetTestCaseContext(data);
                    //now bind the method and useful context, so next time (for probes) GetContextId() will return the real context
                    _method2ctxs.TryAdd(method, testCaseCtx.GetKey());
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    testCaseCtx = GetTestCaseContext(data);
                    break;
                //another commands we don't process here
            }
            return (true, testCaseCtx);
        }
    }
}
