using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_INJECT)]
    public class TargetInjectCommand : AbstractConfiguratorCommand
    {
        public TargetInjectCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /****************************************************************/

        public async override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return FalseEmptyResult;
            //
            var dir = _rep.GetInjectorDirectory();
            var res2 = _cmdHelper.GetExistingSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR,
                dir, _desc, out var injCfgPath, out var _);
            if (!res2)
                return FalseEmptyResult;

            await InjectorProcess(injCfgPath);
            return TrueEmptyResult;
        }

        private async Task<(bool res, string error)> InjectorProcess(string cfgPath)
        {
            var args = $"--{CoreConstants.ARGUMENT_SILENT} --{CoreConstants.ARGUMENT_CONFIG_PATH}={cfgPath}";
            var path = _rep.GetInjectorPath();
            var (res, pid) = CommonUtils.StartProgramm(CoreConstants.SUBSYSTEM_INJECTOR, path, args, out var err);
            if (!res)
                return (false, err);

            //wait
            await CommonUtils.WaitForProcessExit(pid)
                .ConfigureAwait(false);
            return (true, "");
        }

        public override string GetShortDescription()
        {
            return "Inject the target by config";
        }

        public override string GetHelp()
        {
            return @$"The separated from the full CI pipeline process of the instrumenting the specified target by config.

{HelpHelper.GetActiveLastSwitchesDesc(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts)}

Also you can use config path directly:
    Example: {RawContexts} --cfg_path=""d:\Drill4Net\ci\targetA\inj.yml""";
        }
    }
}
