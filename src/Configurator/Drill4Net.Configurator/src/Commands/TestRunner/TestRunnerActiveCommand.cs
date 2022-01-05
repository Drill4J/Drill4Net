using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, 
                         //ConfiguratorConstants.CONTEXT_CFG, 
                         ConfiguratorConstants.COMMAND_ACTIVATE)]
    public class TestRunnerActiveCommand : AbstractConfiguratorCommand
    {
        public TestRunnerActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();

            //source path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            //activate
            var path = _rep.CalcRedirectConfigPath(dir);
            SaveRedirectFile(CoreConstants.SUBSYSTEM_INJECTOR, Path.GetFileNameWithoutExtension(sourcePath), path); //better set just file name but its path

            return Task.FromResult(true);
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
