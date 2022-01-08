using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_LIST)]
    public class ListCommand : AbstractConfiguratorCommand
    {
        private List<AbstractCliCommand>? _commands;

        /****************************************************************/

        public ListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /****************************************************************/

        public void SetCommands(Dictionary<string, AbstractCliCommand> commands)
        {
            _commands = commands.Values
                .Where(a => a.Id != CliConstants.COMMAND_NULL)
                .OrderBy(a => a.RawContexts)
                .ToList();
        }

        public override Task<bool> Process()
        {
            if (_commands == null)
            {
                RaiseError("No commands were given, set them");
                return Task.FromResult(false);
            }
            //
            var maxIdLen = _commands.Max(a => a.RawContexts.Length) + 2;
            for (int i = 0; i < _commands.Count; i++)
            {
                var cmd = _commands[i];
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
