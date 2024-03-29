﻿using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    //https://kb.epam.com/pages/viewpage.action?pageId=1376251876
    //https://kb.epam.com/pages/viewpage.action?pageId=986565061

    #region Delegates
    /// <summary>
    /// Handler for <see cref="IAgentReceiver.InitScopeData"/>
    /// </summary>
    public delegate void InitScopeDataHandler(InitActiveScope scope);

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.PluginLoaded"/>
    /// </summary>
    public delegate void PluginLoadedHandler();

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.RequestClassesData"/>
    /// </summary>
    public delegate void RequestClassesDataHandler();
    
    /// <summary>
    /// Handler for <see cref="IAgentReceiver.TogglePlugin"/> 
    /// </summary>
    public delegate void TogglePluginHandler(string plugin);

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.StartSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void StartSessionHandler(StartAgentSession info);

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.StopSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void StopSessionHandler(StopAgentSession info);
    
    /// <summary>
    /// Handler for <see cref="IAgentReceiver.CancelSession"/>
    /// </summary>
    public delegate void StopAllSessionsHandler();

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.CancelSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void CancelSessionHandler(CancelAgentSession info);

    /// <summary>
    /// Handler for <see cref="IAgentReceiver.CancelAllSessions"/>
    /// </summary>
    public delegate void CancelAllSessionsHandler();
    #endregion

    public interface IAgentReceiver
    {
        /// <summary>
        /// New scope data is initialized
        /// </summary>
        event InitScopeDataHandler InitScopeData;

        /// <summary>
        /// Some plugin is loaded from Admin side
        /// </summary>
        event PluginLoadedHandler PluginLoaded;

        /// <summary>
        /// Command for start session
        /// </summary>
        event RequestClassesDataHandler RequestClassesData;
        
        /// <summary>
        /// On admin side toggled some plugin
        /// </summary>
        event TogglePluginHandler TogglePlugin;

        /// <summary>
        /// Command for start session
        /// </summary>
        event StartSessionHandler StartSession;

        /// <summary>
        /// Command for stop the session
        /// </summary>
        event StopSessionHandler StopSession;
        
        /// <summary>
        /// Command for stop the all sessions
        /// </summary>
        event StopAllSessionsHandler StopAllSessions;

        /// <summary>
        /// Command for cancel the session
        /// </summary>
        event CancelSessionHandler CancelSession;

        /// <summary>
        /// Command for cancel the all sessions
        /// </summary>
        event CancelAllSessionsHandler CancelAllSessions;
    }
}