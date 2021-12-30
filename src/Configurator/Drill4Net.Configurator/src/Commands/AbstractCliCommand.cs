using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using System.Threading.Tasks;

namespace Drill4Net.Configurator
{
    public delegate void MessageDeliveredDelegate(string message, bool isError, bool isFatal, string? source = null);

    /**********************************************************************************************************/

    /// <summary>
    /// Abstract command formed from CLI (command line interface)
    /// </summary>
    public abstract class AbstractCliCommand
    {
        public event MessageDeliveredDelegate? MessageDelivered;

        public string ContextId { get; }

        public CliArgument? SwitchArgument { get; }

        protected readonly ConfiguratorRepository? _rep;
        protected readonly List<CliArgument> _arguments;
        protected readonly Logger _logger;

        /********************************************************************/

        protected AbstractCliCommand(string contextId, List<CliArgument> arguments, ConfiguratorRepository? rep = null)
        {
            _rep = rep;
            if (string.IsNullOrWhiteSpace(contextId))
                throw new ArgumentNullException(nameof(contextId));
            _arguments = arguments;
            _logger = new TypedLogger<AbstractCliCommand>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
            SwitchArgument = arguments?.FirstOrDefault(a => a.Type == CliArgumentType.Switch);
            ContextId = contextId;
        }

        /********************************************************************/

        public abstract Task<bool> Process();

        protected string? GetParamVal(string paramName)
        {
            return _arguments?.FirstOrDefault(a => a.Name.Equals(paramName, StringComparison.InvariantCultureIgnoreCase))?.Value;
        }

        protected void RaiseMessageDelivered(string message, string? source = null)
        {
            MessageDelivered?.Invoke(message, false, false, source);
        }

        protected void RaiseMessageDelivered(string message, bool isError, bool isFatal = false, string? source = null)
        {
            MessageDelivered?.Invoke(message, isError, isFatal, source);
        }
    }
}
