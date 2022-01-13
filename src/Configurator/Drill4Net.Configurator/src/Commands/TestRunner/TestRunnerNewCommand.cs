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
            var modelCfgPath = _rep.GetTestRunnerModelConfigPath();
            if (!File.Exists(modelCfgPath))
            {
                RaiseError($"Model {CoreConstants.SUBSYSTEM_TEST_RUNNER} config not found: [{modelCfgPath}]");
                return Task.FromResult(FalseEmptyResult);
            }
            var res = Edit(modelCfgPath, true);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_TEST_RUNNER} config in interactive mode.";
        }

        public override string GetHelp()
        {
            return $@"This command allows you to interactively create a configuration for the {CoreConstants.SUBSYSTEM_TEST_RUNNER} ""run"" of injected automatic tests in the target application (SUT - system under test). You should simply answer a number of clarifying questions. This is the second stage in the full workflow of the Drill for .NET (after ""just injection of the target"").

The command does not accept any clarifying arguments yet, but this may be done in the near future.";
        }
    }
}
