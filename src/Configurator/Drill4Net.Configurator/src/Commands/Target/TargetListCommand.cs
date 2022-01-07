using System.Threading.Tasks;
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
        public TargetListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /********************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();
            _cmdHelper.ListConfigs<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Get list of the {CoreConstants.SUBSYSTEM_INJECTOR}'s configs";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
