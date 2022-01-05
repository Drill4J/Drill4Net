using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_VIEW)]
    public class SysViewCommand : AbstractConfiguratorCommand
    {
        public SysViewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "View the basic system properties (connections, etc)";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}
