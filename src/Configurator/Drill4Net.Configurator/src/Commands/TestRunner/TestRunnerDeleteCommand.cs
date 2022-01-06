using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class TestRunnerDeleteCommand : AbstractConfiguratorCommand
    {
        public TestRunnerDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.DeleteConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
