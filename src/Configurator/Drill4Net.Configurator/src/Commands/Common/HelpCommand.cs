using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_HELP)]
    public class HelpCommand : AbstractConfiguratorCommand
    {
        private readonly string _mess;

        /*****************************************************************/

        public HelpCommand(ConfiguratorRepository rep) : base(rep)
        {
            _mess = $@"  === Please, type:
  >>> '?' to print this menu.
  >>> '{ConfiguratorConstants.COMMAND_LIST}' to list all commands.
  --- Configurations:
  >>> '{new SysConfigureCommand(_rep).RawContexts}' to the system setup.
  >>> '{new TargetNewCommand(_rep).RawContexts}' to configure new target's injections.
  >>> '{new TestRunnerNewCommand(_rep).RawContexts}' to configure new tests' run.
  >>> '{new CiNewCommand(_rep).RawContexts}' for new CI run's settings.
  --- Actions:
  >>> '{new CiStartCommand(_rep).RawContexts}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
        }

        /*******************************************************************/

        //https://docopt.org/
        public override Task<bool> Process()
        {
            RaiseMessage($"\n{_mess}", CliMessageType.Help);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "View help for the commands";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
