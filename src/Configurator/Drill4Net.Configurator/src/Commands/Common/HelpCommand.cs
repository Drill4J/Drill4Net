using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_HELP)]
    public class HelpCommand : AbstractCliCommand
    {
        //https://docopt.org/

        public override Task<bool> Process()
        {
            const string mess = $@"  === Please, type:
  >>> '?' or 'help' to print this menu.
  --- Configurations:
  >>> '{ConfiguratorConstants.CONTEXT_SYS} {ConfiguratorConstants.CONTEXT_CFG}' to the system setup.
  >>> '{ConfiguratorConstants.CONTEXT_TARGET} {ConfiguratorConstants.CONTEXT_CFG}' to configure the target's injections.
  >>> '{ConfiguratorConstants.CONTEXT_RUNNER} {ConfiguratorConstants.CONTEXT_CFG}' to configure the tests' run.
  >>> '{ConfiguratorConstants.CONTEXT_CI} {ConfiguratorConstants.CONTEXT_CFG}' for the CI run's settings.
  --- Actions:
  >>> '{ConfiguratorConstants.CONTEXT_CI} {ConfiguratorConstants.COMMAND_START}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
            RaiseMessage($"\n{mess}", CliMessageType.Help);
            return Task.FromResult(true);
        }
    }
}
