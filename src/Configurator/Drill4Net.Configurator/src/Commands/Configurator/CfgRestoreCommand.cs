using System;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CFG, ConfiguratorConstants.COMMAND_RESTORE)]
    public class CfgRestoreCommand : AbstractConfiguratorCommand
    {
        public CfgRestoreCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return $"";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}
