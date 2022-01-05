using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TargetEditCommand : AbstractTargetEditor
    {
        public TargetEditCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, this, out var sourcePath,
                out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Edit the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config in interactive mode";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
