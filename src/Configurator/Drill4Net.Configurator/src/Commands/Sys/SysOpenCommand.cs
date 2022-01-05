using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_SYS, ConfiguratorConstants.COMMAND_OPEN)]
    public class SysOpenCommand : AbstractConfiguratorCommand
    {
        public SysOpenCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }

        public override string GetShortDescription()
        {
            return "Open in external editor the configs with system properties (connections, etc)";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}
