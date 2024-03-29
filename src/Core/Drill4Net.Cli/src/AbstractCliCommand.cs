﻿using System.Reflection;
using Drill4Net.BanderLog;

namespace Drill4Net.Cli
{
    public delegate void MessageDeliveredDelegate(string source, string message, CliMessageType messType = CliMessageType.Annotation,
        MessageState state = MessageState.NewLine);

    /**********************************************************************************************************/

    /// <summary>
    /// Abstract command formed from CLI (command line interface)
    /// </summary>
    public abstract class AbstractCliCommand : CliMessager
    {
        public string RawContexts { get; }

        /// <summary>
        /// List of arguments for the command (which located in <see cref="Contexts"/> among others contexts' tags).
        /// </summary>
        public List<CliArgument>? Arguments => _desc?.Arguments;

        protected (bool done, Dictionary<string, object> results) FalseEmptyResult = new(false, new());
        protected (bool done, Dictionary<string, object> results) TrueEmptyResult = new(true, new());

        protected CliDescriptor? _desc;
        protected readonly Logger _logger;

        /********************************************************************/

        protected AbstractCliCommand(string subsystem)
        {
            var attr = SearchCommandAttribute();
            Id = attr.Id;
            RawContexts = attr.RawId;
            _logger = new TypedLogger<AbstractCliCommand>(subsystem);
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
        public abstract Task<(bool done, Dictionary<string, object> results)> Process();

        public async Task<(bool done, Dictionary<string, object> results)> ProcessFor(AbstractCliCommand cmd, CliDescriptor desc)
        {
            cmd.Init(desc);
            cmd.MessageDelivered += RaiseRawMessage;
            var res = await cmd.Process();
            cmd.MessageDelivered -= RaiseRawMessage;
            return res;
        }

        public abstract string GetShortDescription();
        public abstract string GetHelp();

        public CliDescriptor? GetDescriptor() => _desc;

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
        public List<CliArgument>? GetPositionals() => _desc?.GetPositionals();

        public string? GetPositional(int ind)
        {
            return _desc?.GetPositional(ind);
        }

        public bool IsSwitchSet(char sw)
        {
            return (_desc?.IsSwitchSet(sw)) ?? false;
        }
    }
}
