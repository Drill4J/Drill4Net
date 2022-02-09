using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_OPEN)]
    public class TargetOpenCommand : AbstractConfiguratorCommand
    {
        public TargetOpenCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.OpenConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Open in external editor the config for {CoreConstants.SUBSYSTEM_INJECTOR}.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "targetA")}";
        }
    }
}
