using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.COMMAND_CLS)]
    public class ClsCommand : AbstractCliCommand
    {
        public ClsCommand(string subsystem) : base(subsystem)
        {
        }

        /*************************************************************/

        public override Task<bool> Process()
        {
            Console.Clear();
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Clear the program screen";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}
