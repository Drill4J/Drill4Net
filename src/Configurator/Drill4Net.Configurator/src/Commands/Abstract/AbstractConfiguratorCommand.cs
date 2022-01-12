using System;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public abstract class AbstractConfiguratorCommand : AbstractCliCommand
    {
        protected readonly CliInteractor _cli;
        protected readonly CommandHelper _cmdHelper;
        protected readonly CliCommandRepository _cliRep;
        protected readonly ConfiguratorRepository _rep;

        protected (bool done, Dictionary<string, object> results) OkCheck =
            new(true, new() { { ConfiguratorConstants.CHECK_KEY, ConfiguratorConstants.CHECK_VALUE_OK } });
        protected (bool done, Dictionary<string, object> results) NotCheck =
            new(true, new() { { ConfiguratorConstants.CHECK_KEY, ConfiguratorConstants.CHECK_VALUE_NOT } });

        /****************************************************************************/

        protected AbstractConfiguratorCommand(ConfiguratorRepository rep, CliCommandRepository cliRep): base(rep.Subsystem)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _cliRep = cliRep ?? throw new ArgumentNullException(nameof(cliRep));
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
