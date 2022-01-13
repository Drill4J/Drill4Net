using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_LIST)]
    public class TargetListCommand : AbstractConfiguratorCommand
    {
        public TargetListCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /********************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var dir = _rep.GetInjectorDirectory();
            _cmdHelper.ListConfigs<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir);
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_INJECTOR} configs.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
