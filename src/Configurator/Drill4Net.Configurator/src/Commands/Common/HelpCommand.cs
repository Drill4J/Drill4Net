using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("?")]
    public class HelpCommand : AbstractCliCommand
    {
        //https://docopt.org/

        public override Task<bool> Process()
        {
            const string mess = $@"  === Please, type:
  >>> '?' or 'help' to print this menu.
  --- Configurations:
  >>> '{ConfiguratorConstants.COMMAND_SYS}' to the system setup.
  >>> '{ConfiguratorConstants.COMMAND_TARGET}' to target's injection configure.
  >>> '{ConfiguratorConstants.COMMAND_RUNNER}' to tests run's configure.
  >>> '{ConfiguratorConstants.COMMAND_CI}' for the CI run's settings.
  --- Actions:
  >>> '{ConfiguratorConstants.COMMAND_START}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
            RaiseMessage($"\n{mess}", CliMessageType.Help);
            return Task.FromResult(true);
        }
    }
}
