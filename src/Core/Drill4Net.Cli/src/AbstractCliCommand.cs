using System.Reflection;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Cli
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

        public CliArgument? SwitchArgument { get; private set; }

        protected List<CliArgument> _arguments;
        protected readonly Logger _logger;

        /********************************************************************/

        protected AbstractCliCommand()
        {
            ContextId = SearchCommandIdByAttribute();
            _arguments = new();
            _logger = new TypedLogger<AbstractCliCommand>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
        }

        /********************************************************************/

        public void Init(List<CliArgument> arguments)
        {
            _arguments = arguments;
            SwitchArgument = arguments?.FirstOrDefault(a => a.Type == CliArgumentType.Switch);
        }

        protected string SearchCommandIdByAttribute()
        {
            var attrs = GetType().GetCustomAttributes();
            var attr = (CliCommandAttribute)attrs.First(a => a.GetType().Name == nameof(CliCommandAttribute));
            return attr.Id;
        }

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
