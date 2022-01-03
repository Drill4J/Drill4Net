using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("cls")]
    public class ClsCommand : AbstractCliCommand
    {
        public override Task<bool> Process()
        {
            Console.Clear();
            return Task.FromResult(true);
        }
    }
}
