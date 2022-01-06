using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG, 
                         ConfiguratorConstants.COMMAND_ACTIVE)]
    public class TestRunnerActiveCommand : AbstractConfiguratorCommand
    {
        public TestRunnerActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.ActivateConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
