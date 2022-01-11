using System.Threading.Tasks;
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

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.ActivateConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
