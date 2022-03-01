using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_ACTIVE)]
    public class TestRunnerActiveCommand : AbstractConfiguratorCommand
    {
        public TestRunnerActiveCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.ActivateConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER} config.";
        }

        public override string GetHelp()
        {
            return HelpHelper.GetActiveConfigText(CoreConstants.SUBSYSTEM_TEST_RUNNER, true, RawContexts, "runner");
        }
    }
}
