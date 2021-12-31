using System;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public class AbstractCongifuratorCommand : AbstractCliCommand
    {
        protected ConfiguratorRepository _rep;

        /****************************************************************************/

        public AbstractCongifuratorCommand(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
        }

        public override Task<bool> Process()
        {
            return Task.FromResult(false);
        }
    }
}
