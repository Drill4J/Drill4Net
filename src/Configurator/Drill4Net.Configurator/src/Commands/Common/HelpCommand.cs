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
  >>> '?' to print this menu.
  >>> '{ConfiguratorConstants.COMMAND_LIST}' to list all commands.
  --- Configurations:
  >>> '{ConfiguratorConstants.CONTEXT_SYS} {ConfiguratorConstants.CONTEXT_CFG}' to the system setup.
  >>> '{ConfiguratorConstants.CONTEXT_TARGET} {ConfiguratorConstants.COMMAND_NEW}' to configure new target's injections.
  >>> '{ConfiguratorConstants.CONTEXT_RUNNER} {ConfiguratorConstants.COMMAND_NEW}' to configure new tests' run.
  >>> '{ConfiguratorConstants.CONTEXT_CI} {ConfiguratorConstants.COMMAND_NEW}' for new CI run's settings.
  --- Actions:
  >>> '{ConfiguratorConstants.CONTEXT_CI} {ConfiguratorConstants.COMMAND_START}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
            RaiseMessage($"\n{mess}", CliMessageType.Help);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "View help for the commands";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
