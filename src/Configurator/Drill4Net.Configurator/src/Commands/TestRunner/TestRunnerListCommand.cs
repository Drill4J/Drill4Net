using System.Threading.Tasks;
using System.Collections.Generic;
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
        public TestRunnerListCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();
            _cmdHelper.ListConfigs<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir);
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_TEST_RUNNER} configs.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
