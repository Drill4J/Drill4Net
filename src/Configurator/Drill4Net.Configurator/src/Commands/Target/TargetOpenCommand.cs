using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_OPEN)]
    public class TargetOpenCommand : AbstractConfiguratorCommand
    {
        public TargetOpenCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.OpenConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Open in external editor the config for {CoreConstants.SUBSYSTEM_INJECTOR}";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}
