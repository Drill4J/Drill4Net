using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_COPY)]
    public class TestRunnerCopyCommand : AbstractConfiguratorCommand
    {
        public TestRunnerCopyCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, this,
                out var sourcePath, out var fromSwitch, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            var delta = fromSwitch ? 1 : 0;

            // dest path
            var destName = GetPositional(1 - delta);
            res = _cmdHelper.GetConfigPath(dir, "destination", destName, false, out var destPath, out error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            var cfg = _rep.ReadTestRunnerOptions(sourcePath);

            //...maybe ask/set some properties...

            //save config
            res = _cmdHelper.SaveConfig(CoreConstants.SUBSYSTEM_INJECTOR, cfg, destPath);
            if (res)
                RaiseWarning($"Now you have to tune properties in the file directly: [{destPath}]");

            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Copy the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config to new one with some new parameters";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
