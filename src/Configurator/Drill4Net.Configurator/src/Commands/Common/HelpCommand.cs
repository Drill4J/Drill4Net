using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_HELP)]
    public class HelpCommand : AbstractConfiguratorCommand
    {
        private Dictionary<string, AbstractCliCommand>? _commands;
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

        public void SetCommands(Dictionary<string, AbstractCliCommand> commands)
        {
            _commands = commands;
        }

        //https://docopt.org/
        public override Task<bool> Process()
        {
            var contexts = GetPositionals();
            string? s;
            if (contexts == null || contexts.Count == 0)
            {
                s = _mess;
            }
            else
            {
                if (_commands == null)
                {
                    RaiseError("No commands were given");
                    return Task.FromResult(false);
                }
                var args = string.Join(" ", contexts.Select(a => a.Value));
                var desc = new CliDescriptor(args, true);
                AbstractCliCommand cmd;
                if (_commands.ContainsKey(desc.CommandId))
                {
                    cmd = _commands[desc.CommandId];
                    s = CreateHelp(cmd);
                }
                else
                {
                    RaiseWarning($"The command was not found: [{args}]");
                    return Task.FromResult(false);
                }
            }
            RaiseMessage($"\n{s}", CliMessageType.Help);
            return Task.FromResult(true);
        }

        internal string CreateHelp(AbstractCliCommand cmd)
        {
            return @$"Command: {cmd.RawContexts}
{cmd.GetShortDescription()}

{cmd.GetHelp()}";
        }

        public override string GetShortDescription()
        {
            return "View help for the commands";
        }

        public override string GetHelp()
        {
            return @"Shows the help article for the specified command. Use positional parameters with -- symbols for dividing).
   
Example: ? -- ci view
";
        }
    }
}
