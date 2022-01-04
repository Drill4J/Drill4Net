using System;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public abstract class AbstractConfiguratorCommand : AbstractCliCommand
    {
        protected readonly CommandHelper _commandHelper;
        protected readonly ConfiguratorRepository _rep;

        /****************************************************************************/

        protected AbstractConfiguratorCommand(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _commandHelper = new(rep);
        }
    }
}
