using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_OPEN)]
    public class TestRunnerOpenCommand : AbstractConfiguratorCommand
    {
        public TestRunnerOpenCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.OpenConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Open in external editor the config for the {CoreConstants.SUBSYSTEM_TEST_RUNNER}.";
        }

        public override string GetHelp()
        {
            return $@"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_TEST_RUNNER, RawContexts, "runner")}";
        }
    }
}
