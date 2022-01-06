using System.Threading.Tasks;
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
        public TargetViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetInjectorDirectory();
            return Task.FromResult(_cmdHelper.ViewFile<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc));
        }

        public override string GetShortDescription()
        {
            return $"View the content of specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
