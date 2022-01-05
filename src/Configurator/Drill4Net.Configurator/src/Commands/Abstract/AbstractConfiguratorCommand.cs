using System;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public abstract class AbstractConfiguratorCommand : AbstractCliCommand
    {
        protected readonly CliInteractor _cli;
        protected readonly CommandHelper _cmdHelper;
        protected readonly ConfiguratorRepository _rep;

        /****************************************************************************/

        protected AbstractConfiguratorCommand(ConfiguratorRepository rep): base(rep.Subsystem)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            //
            _cli = new(Id);
            _cli.MessageDelivered += MessageDeliveredHandler;

            _cmdHelper = new(_cli, rep);
            _cmdHelper.MessageDelivered += MessageDeliveredHandler;
        }

        private void MessageDeliveredHandler(string source, string message, CliMessageType messType = CliMessageType.Annotation,
            MessageState state = MessageState.NewLine)
        {
            RaiseDelivered(message, messType, state);
        }
    }
}
