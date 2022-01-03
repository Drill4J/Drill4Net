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
    }
}
