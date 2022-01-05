using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_LIST)]
    public class TestRunnerListCommand : AbstractConfiguratorCommand
    {
        public TestRunnerListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();
            _cmdHelper.ListConfigs<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s configs";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
