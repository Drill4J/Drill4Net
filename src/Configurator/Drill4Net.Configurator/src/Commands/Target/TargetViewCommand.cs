using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_VIEW)]
    public class TargetViewCommand : AbstractConfiguratorCommand
    {
        public TargetViewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /*****************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetInjectorDirectory();
            var done = _cmdHelper.ViewFile<InjectionOptions>(CoreConstants.SUBSYSTEM_INJECTOR,
                dir, _desc, out var _);
            return Task.FromResult((done, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"View the content of specified {CoreConstants.SUBSYSTEM_INJECTOR} config.";
        }

        public override string GetHelp()
        {
            return @$"{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "targetA")}";
        }
    }
}
