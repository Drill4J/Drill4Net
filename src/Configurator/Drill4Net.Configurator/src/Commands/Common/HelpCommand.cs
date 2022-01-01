using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("?")]
    public class HelpCommand : AbstractConfiguratorCommand
    {
        public HelpCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**********************************************************************/

        //https://docopt.org/
        public override Task<bool> Process()
        {
            const string mess = $@"  === Please, type:
  >>> '?' or 'help' to print this menu.
  --- Configurations:
  >>> '{ConfiguratorConstants.COMMAND_SYS}' for the system settings.
  >>> '{ConfiguratorConstants.COMMAND_TARGET}' for target's injection.
  >>> '{ConfiguratorConstants.COMMAND_RUNNER}' for the Test Runner.
  >>> '{ConfiguratorConstants.COMMAND_CI}' for CI settings.
  --- Actions:
  >>> '{ConfiguratorConstants.COMMAND_START}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
            RaiseMessage($"\n{mess}", CliMessageType.Help);
            return Task.FromResult(true);
        }
    }
}
