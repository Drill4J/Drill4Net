using System.Threading.Tasks;
using System.Collections.Generic;
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
        public TestRunnerDeleteCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.DeleteConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc, out var _);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
