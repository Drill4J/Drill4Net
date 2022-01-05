using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_NEW)]
    public class TestRunnerNewCommand : AbstractTestRunnerEditor
    {
        public TestRunnerNewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*******************************************************************************/

        public override Task<bool> Process()
        {
            var modelCfgPath = Path.Combine(_rep.Options.InstallDirectory, ConfiguratorConstants.CONFIG_TEST_RUNNER_MODEL);
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config not found: [{modelCfgPath}]");
                return Task.FromResult(false);
            }

            return Task.FromResult(Edit(modelCfgPath, true));
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config in interactive mode";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
