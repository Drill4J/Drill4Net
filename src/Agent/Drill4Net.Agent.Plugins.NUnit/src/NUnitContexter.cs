using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Plugins.NUnit3
{
    //https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html

    public class NUnitContexter : AbstractEngineContexter
    {
        public NUnitContexter() : base(nameof(NUnitContexter))
        {
        }

        /*********************************************************************/

        public override string GetContextId()
        {
            var test = NUnit.Framework.TestContext.CurrentContext?.Test;
            if (test?.MethodName == null)
                return null;
            var ctx = test.FullName;
            if (ctx?.Contains("Internal.TestExecutionContext+") == true) //in fact, NUnit's context is absent
                return null;
            return ctx; //TODO: check !!!!
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
            if (!_comTypes.Contains(command) || GetContextId() == null)
                return (false, null);
            //
            TestCaseContext testCaseCtx = null;
            switch ((AgentCommandType)command)
            {
                case AgentCommandType.TEST_CASE_START:
                case AgentCommandType.TEST_CASE_STOP:
                    testCaseCtx = GetTestCaseContext(data);
                    break;
                    //another commands we don't process here
            }
            return (true, testCaseCtx);
        }
    }
}
