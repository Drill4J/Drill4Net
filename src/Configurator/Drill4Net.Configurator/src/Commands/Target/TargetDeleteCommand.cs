using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class TargetDeleteCommand : AbstractConfiguratorCommand
    {
        public TargetDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "Delete the specified Injector's config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
