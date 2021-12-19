using System;
using Xunit.Abstractions;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Plugins.xUnit2
{
    // https://github.com/xunit/xunit/issues/621 - no test context in xUnit 2.4.x now. It is sad.
    // but in the discussion above and in the source xUnit 3.x (as silly class) it exists (not in NuGet package - commit on 23 Jule, 2021):
    // https://github.com/xunit/xunit/blob/32a168c759e38d25931ee91925fa75b6900209e1/src/xunit.v3.core/Sdk/Frameworks/TestContextAccessor.cs

    public class XUnitContexter : AbstractEngineContexter
    {
        private string _curCtx;

        /**********************************************************************/

        public XUnitContexter() : base(nameof(XUnitContexter))
        {
        }

        /**********************************************************************/

        public override string GetContextId()
        {
            return _curCtx;
        }

        public override TestEngine GetTestEngine()
        {
            return new TestEngine
            {
                Name = "Xunit",
                Version = FileUtils.GetProductVersion(typeof(IExecutionMessage)),
                MustSequential = true,
            };
        }

        public override bool RegisterCommand(int command, string data)
        {
            if (!_comTypes.Contains(command))
                return false;
            //
            switch ((AgentCommandType)command)
            {
                case AgentCommandType.TEST_CASE_START:
                    var testCaseCtx = GetTestCaseContext(data);
                    _curCtx = testCaseCtx.CaseName;
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    _curCtx = null;
                    break;
                //another commands we don't process here
            }
            return true;
        }
    }
}
