using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_ACTIVE)]
    public class TargetActiveCommand : AbstractConfiguratorCommand
    {
        public TargetActiveCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /******************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.ActivateConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_INJECTOR} config.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
