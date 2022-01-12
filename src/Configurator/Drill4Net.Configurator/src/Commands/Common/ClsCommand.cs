using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            Console.Clear();
            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "Clear the program screen.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
