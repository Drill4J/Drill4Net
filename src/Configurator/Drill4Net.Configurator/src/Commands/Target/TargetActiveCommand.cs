using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                     ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_ACTIVATE)]
    public class TargetActiveCommand : AbstractConfiguratorCommand
    {
        public TargetActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "Activate the specified Injector's config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
