using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_LIST)]
    public class ListCommand : AbstractConfiguratorCommand
    {
        public ListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /****************************************************************/

        public override Task<bool> Process()
        {
            CliCommandRepository cmdRep = new(_rep);
            var list = cmdRep.Commands.Values.OrderBy(a => a.ContextId)
                .Where(a => a.ContextId != CliConstants.COMMAND_NULL)
                .ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var cmd = list[i];
                var sig = cmd.ContextId.ToLower().Replace("_", " ");
                var desc = cmd.GetShortDescription().Trim();
                if (!string.IsNullOrWhiteSpace(desc))
                    desc = " => " + desc;
                RaiseMessage($"{i}. {sig}{desc}");
            }

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Get the list of available commands";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
