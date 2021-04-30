﻿using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    #region Delegates
    /// <summary>
    /// Handler for <see cref="IReceiver.StartSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void StartSessionHandler(StartAgentSession info);

    /// <summary>
    /// Handler for <see cref="IReceiver.StopSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void StopSessionHandler(StopAgentSession info);
    
    /// <summary>
    /// Handler for <see cref="IReceiver.CancelSession"/>
    /// </summary>
    public delegate void StopAllSessionsHandler();

    /// <summary>
    /// Handler for <see cref="IReceiver.CancelSession"/>
    /// </summary>
    /// <param name="info"></param>
    public delegate void CancelSessionHandler(CancelAgentSession info);

    /// <summary>
    /// Handler for <see cref="IReceiver.CancelAllSessions"/>
    /// </summary>
    public delegate void CancelAllSessionsHandler();
    #endregion
    
    public interface IReceiver
    {
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