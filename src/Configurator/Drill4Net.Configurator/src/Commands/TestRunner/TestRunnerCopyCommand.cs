using System.Threading.Tasks;
using System.Collections.Generic;
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
        public TestRunnerCopyCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);

            var defDir = _rep.GetTestRunnerDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, defDir, _desc,
                out var sourcePath, out var fromSwitch, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            var delta = fromSwitch ? 1 : 0;

            // dest path
            var destName = GetPositional(1 - delta) ?? "";
            res = _cmdHelper.GetConfigPath(defDir, "destination", destName, false, out var destPath, out error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            var cfg = _rep.ReadTestRunnerOptions(sourcePath);

            //...maybe ask/set some properties...

            //save config
            res = _cmdHelper.SaveConfig(CoreConstants.SUBSYSTEM_TEST_RUNNER, cfg, destPath);
            if (res)
                RaiseWarning($"Now you can to tune properties in the file directly: [{destPath}]");

            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Copy the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER} config to new one with some new parameters.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetCopyConfigDesc(CoreConstants.SUBSYSTEM_TEST_RUNNER, RawContexts, "runner")}";
        }
    }
}
