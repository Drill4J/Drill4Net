﻿using System.IO;
using System.Threading.Tasks;
using Drill4Net.Common;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Handler for <see cref="AbstractAgent.Initialized"/>
    /// </summary>
    public delegate void AgentInitializedHandler();

    /// <summary>
    /// Abstract Agent collecting probe data of the cross-point in instrumented Target
    /// </summary>
    public abstract class AbstractAgent
    {
        /// <summary>
        /// The Agent is initialized
        /// </summary>
        public event AgentInitializedHandler Initialized;

        /**************************************************************************/

        protected void RaiseInitilizedEvent()
        {
            Initialized?.Invoke();
        }

        /// <summary>
        /// Register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public abstract void Register(string data);

        public abstract void RegisterWithContext(string data, string ctx);

        /// <summary>
        /// Async register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public Task RegisterAsync(string data)
        {
            return Task.Run(() => Register(data));
        }

        /// <summary>
        /// Async register the cross-point's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public Task RegisterWithContextAsync(string data, string ctx)
        {
            return Task.Run(() => RegisterWithContext(data, ctx));
        }

        public static string GetDefaultConnectorLogFilePath()
        {
            return Path.Combine(FileUtils.GetCommonLogDirectory(FileUtils.GetCallingDir()), AgentConstants.CONNECTOR_LOG_FILE_NAME);
        }
    }
}
