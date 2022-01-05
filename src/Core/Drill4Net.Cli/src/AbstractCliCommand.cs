using System.Reflection;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Cli
{
    public delegate void MessageDeliveredDelegate(string source, string message, CliMessageType messType = CliMessageType.Annotation,
        MessageState state = MessageState.NewLine);

    /**********************************************************************************************************/

    /// <summary>
    /// Abstract command formed from CLI (command line interface)
    /// </summary>
    public abstract class AbstractCliCommand
    {
        public event MessageDeliveredDelegate? MessageDelivered;

        public string Id { get; }

        public string RawContexts { get; }

        /// <summary>
        /// List of arguments for the command (which located in <see cref="Contexts"/> among others contexts' tags).
        /// </summary>
        public List<CliArgument> Arguments => _desc == null ? new() : _desc.Arguments;

        protected CliDescriptor? _desc;
        protected readonly Logger _logger;

        /********************************************************************/

        protected AbstractCliCommand()
        {
            var attr = SearchCommandAttribute();
            Id = attr.Id;
            RawContexts = attr.RawId;
            _logger = new TypedLogger<AbstractCliCommand>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
        }

        /********************************************************************/

        /// <summary>
        /// You must initialize the command using the specified command-line interface 
        /// descriptor before calling the process method.
        /// </summary>
        /// <param name="desc"></param>
        public void Init(CliDescriptor desc)
        {
            _desc = desc;
        }

        protected CliCommandAttribute SearchCommandAttribute()
        {
            var attrs = GetType().GetCustomAttributes();
            return (CliCommandAttribute)attrs.First(a => a.GetType().Name == nameof(CliCommandAttribute));
        }

        /// <summary>
        /// Main method for the Command
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> Process();

        public abstract string GetShortDescription();
        public abstract string GetHelp();

        /// <summary>
        /// Get the parameter value by its name.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="isSwitch">Is it CLI switch (one char, e.g. for 'a' in string "-abc" -> is it setted)?</param>
        /// <returns>Value of the parameter. For switches it will be strings "true" or "false"</returns>
        public string? GetParameter(string name, bool isSwitch = false)
        {
            return _desc?.GetParameter(name, isSwitch);
        }

        /// <summary>
        /// Get the alone values (parameters without their names and without prefix "-" or "--")
        /// </summary>
        /// <returns></returns>
        public List<CliArgument> GetPositionals() => _desc == null ? new() : _desc.GetPositionals();

        public string GetPositional(int ind)
        {
            if (ind < 0)
                return string.Empty;
            var poses = GetPositionals();
            if (ind >= poses.Count)
                return string.Empty;
            return poses[ind].Value;
        }

        public bool IsSwitchSet(char sw)
        {
            return (_desc?.IsSwitchSet(sw)) ?? false;
        }

        #region RaiseMessage
        protected void RaiseMessage(string message, CliMessageType messType = CliMessageType.Annotation)
        {
            MessageDelivered?.Invoke(Id, message, messType);
        }

        protected void RaiseQuestion(string message)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Question);
        }

        protected void RaiseWarning(string message, MessageState state = MessageState.NewLine)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Warning, state);
        }

        protected void RaiseError(string message)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Error);
        }
        #endregion
    }
}
