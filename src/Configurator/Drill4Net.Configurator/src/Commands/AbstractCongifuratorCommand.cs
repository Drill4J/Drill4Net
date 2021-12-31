using System;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public abstract class AbstractCongifuratorCommand : AbstractCliCommand
    {
        protected ConfiguratorRepository _rep;

        /****************************************************************************/

        protected AbstractCongifuratorCommand(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
        }
    }
}
