using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_CLS)]
    public class ClsCommand : AbstractCliCommand
    {
        public override Task<bool> Process()
        {
            Console.Clear();
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Clears the program screen";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
