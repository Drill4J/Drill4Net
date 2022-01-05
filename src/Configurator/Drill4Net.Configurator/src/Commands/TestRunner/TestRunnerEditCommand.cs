using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TestRunnerEditCommand : AbstractTestRunnerEditor
    {
        public TestRunnerEditCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();

            // source path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Edit the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config in interactive mode";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
