using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(CliConstants.COMMAND_HELP)]
    public class HelpCommand : AbstractConfiguratorCommand
    {
        private readonly string _mess;

        /*****************************************************************/

        public HelpCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
            _mess = $@"  === Please, type:
  >>> '?' to print this menu.
  >>> '? ?' to read about help system.
  >>> 'about' to read about programm.
  >>> '{ConfiguratorConstants.COMMAND_LIST}' to list all commands.
  --- Configurations:
  >>> '{new SysConfigureCommand(_rep, _cliRep).RawContexts}' to the system setup.
  >>> '{new TargetNewCommand(_rep, _cliRep).RawContexts}' to configure new target's injections.
  >>> '{new TestRunnerNewCommand(_rep, _cliRep).RawContexts}' to configure new tests' run.
  >>> '{new CiNewCommand(_rep, _cliRep).RawContexts}' for new CI run's settings.
  --- Actions:
  >>> '{new CiStartCommand(_rep, _cliRep).RawContexts}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
        }

        /*******************************************************************/

        //https://docopt.org/
        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var contexts = GetPositionals();
            string? s;
            if (contexts == null || contexts.Count == 0)
            {
                s = _mess;
            }
            else
            {
                var args = string.Join(" ", contexts.Select(a => a.Value));
                var desc = new CliDescriptor(args, true);
                AbstractCliCommand cmd;
                var commands = _cliRep.Commands
                    .Where(a => !a.Value.Id.Equals(CliConstants.COMMAND_NULL,
                                StringComparison.InvariantCultureIgnoreCase))
                    .ToDictionary(a => a.Key);
                if (commands.ContainsKey(desc.CommandId))
                {
                    cmd = _cliRep.Commands[desc.CommandId];
                    s = CreateHelp(cmd);
                }
                else
                {
                    RaiseWarning($"The command is not found: [{args}]");
                    return Task.FromResult(FalseEmptyResult);
                }
            }
            RaiseMessage($"\n{s}", CliMessageType.Help);
            return Task.FromResult(TrueEmptyResult);
        }

        internal string CreateHelp(AbstractCliCommand cmd)
        {
            return @$"Command: {cmd.RawContexts}
{cmd.GetShortDescription()}

{cmd.GetHelp()}";
        }

        public override string GetShortDescription()
        {
            return "View help article for the specified commands.";
        }

        public override string GetHelp()
        {
            return @"You can use positional parameters with symbols -- for clear separation. It is also allowed to use the 'help' command. 

Those examples show the way to get the help description for the command 'ci view'.   
   Example 1: ? ci check
   Example 2: help ci check
   Example 3: help -- ci check";
        }
    }
}
