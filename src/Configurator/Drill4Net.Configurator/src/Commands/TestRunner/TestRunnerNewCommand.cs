using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_NEW)]
    public class TestRunnerNewCommand : AbstractTestRunnerEditor
    {
        public TestRunnerNewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*******************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var modelCfgPath = Path.Combine(_rep.GetInstallDirectory(), ConfiguratorConstants.CONFIG_TEST_RUNNER_MODEL);
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config not found: [{modelCfgPath}]");
                return Task.FromResult(FalseEmptyResult);
            }
            var res = Edit(modelCfgPath, true);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config in interactive mode.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
