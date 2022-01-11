using System;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_LIST)]
    public class ListCommand : AbstractConfiguratorCommand
    {
        public ListCommand(ConfiguratorRepository rep, CliCommandRepository cliRep): base(rep, cliRep)
        {
        }

        /****************************************************************/

        public override Task<bool> Process()
        {
            var commands = _cliRep.Commands.Values
                .Where(a => !a.Id.Equals(CliConstants.COMMAND_NULL, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            var maxIdLen = commands.Max(a => a.RawContexts.Length) + 2;
            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];
                var num = $"{i+1}.".PadRight(4);
                var desc = "";
                try { desc = cmd.GetShortDescription().Trim(); } catch { }
                if (!string.IsNullOrWhiteSpace(desc))
                    desc = " => " + desc;
                var s = $"{num}{cmd.RawContexts}".PadRight(maxIdLen + num.Length);
                RaiseMessage($"{s}{desc}");
            }
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Get the list of available commands.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
