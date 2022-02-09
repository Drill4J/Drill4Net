using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TestRunnerEditCommand : AbstractTestRunnerEditor
    {
        public TestRunnerEditCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);

            var dir = _rep.GetTestRunnerDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc,
                out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Edit the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER} config in interactive mode.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_TEST_RUNNER, RawContexts, "runner")}";
        }
    }
}
