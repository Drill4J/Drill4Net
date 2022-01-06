using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class TargetDeleteCommand : AbstractConfiguratorCommand
    {
        public TargetDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.DeleteConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}
