using System.IO;
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
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.DeleteConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, this);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
