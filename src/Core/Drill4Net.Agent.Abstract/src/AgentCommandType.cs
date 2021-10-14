
namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Type of command for Agent subsystem
    /// </summary>
    public enum AgentCommandType
    {
        CLASS_TESTS_START = 0,
        CLASS_TESTS_STOP = 1,

        TEST_START = 2,
        TEST_STOP = 3,

        TEST_CASE_START = 4,
        TEST_CASE_STOP = 5,
    }
}
