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
        public XUnitContexter() : base(nameof(XUnitContexter))
        {
        }

        /**********************************************************************/

        public override string GetContextId()
        {
            return null;
        }

        public override void RegisterCommand(int command, string data)
        {
            //TODO: accounting the test workflow changing

            var comTypes = Enum.GetValues(typeof(AgentCommandType)).Cast<int>().ToList();
            if (!comTypes.Contains(command))
            {
                //_logger.Error($"Unknown command: [{command}] -> [{data}]");
                return;
            }
            //
            var type = (AgentCommandType)command;
            //_logger.Debug($"Command: [{type}] -> [{data}]");

            TestCaseContext testCaseCtx;
            switch (type)
            {
                case AgentCommandType.ASSEMBLY_TESTS_START: StartTests(data); break;
                case AgentCommandType.ASSEMBLY_TESTS_STOP: StopTests(data); break;

                case AgentCommandType.TEST_CASE_START:
                    testCaseCtx = GetTestCaseContext(data);
                    //RegisterTest2RunInfoStart(testCaseCtx);
                    break;
                case AgentCommandType.TEST_CASE_STOP:
                    testCaseCtx = GetTestCaseContext(data);
                    //RegisterTest2RunInfoFinish(testCaseCtx);
                    break;
                default:
                    break;
            }
        }

        private void StopTests(string data)
        {
            throw new NotImplementedException();
        }

        private void StartTests(string data)
        {
            throw new NotImplementedException();
        }
    }
}
