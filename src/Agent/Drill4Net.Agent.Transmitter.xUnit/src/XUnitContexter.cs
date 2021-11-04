using System;
using System.Linq;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transmitter.xUnit
{
    // https://github.com/xunit/xunit/issues/621 - they say, no test context in xUnit 2.4.x now. It is sad.
    // but in the discussion above and in the source xUnit 3.x (as silly class) it exists (not in NuGet package - commit on 23 Jule, 2021):
    // https://github.com/xunit/xunit/blob/32a168c759e38d25931ee91925fa75b6900209e1/src/xunit.v3.core/Sdk/Frameworks/TestContextAccessor.cs

    public class XUnitContexter : AbstractContexter
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

        public override bool RegisterCommand(int command, string data)
        {
            //TODO: accounting the test workflow changing

            var comTypes = Enum.GetValues(typeof(AgentCommandType)).Cast<int>().ToList();
            if (!comTypes.Contains(command))
                return false;
            //
            var type = (AgentCommandType)command;
            switch (type)
            {
                case AgentCommandType.TEST_CASE_START:
                    var testCaseCtx = GetTestCaseContext(data);
                    _curCtx = testCaseCtx.CaseName;
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    _curCtx = null;
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}
