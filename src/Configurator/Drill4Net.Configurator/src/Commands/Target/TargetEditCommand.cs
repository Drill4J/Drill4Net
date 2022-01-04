using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TargetEditCommand : AbstractInteractiveCommand
    {
        public TargetEditCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "Edit the specified Injector's config in interactive mode";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
