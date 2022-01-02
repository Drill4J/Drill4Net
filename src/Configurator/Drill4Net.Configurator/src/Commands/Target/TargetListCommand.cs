using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("trg", "cfg", "list")]
    public class TargetListCommand : AbstractConfiguratorCommand
    {
        public TargetListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /******************************************************************/

        public override Task<bool> Process()
        {
            throw new NotImplementedException();
        }
    }
}
