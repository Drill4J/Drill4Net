namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    #region Delegates
    /// <summary>
    /// Handler for <see cref="IReceiver.StartSession"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="testType">MANUAL or AUTO</param>
    /// <param name="isRealTime"></param>
    /// <param name="startTime">currentTimeMillis when session start</param>
    public delegate void StartSessionHandler(string sessionUid, string testType, bool isRealTime, long startTime);

    /// <summary>
    /// Handler for <see cref="IReceiver.StopSession"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="finishTime">currentTimeMillis when session finished</param>
    public delegate void StopSessionHandler(string sessionUid, long finishTime);
    
    /// <summary>
    /// Handler for <see cref="IReceiver.CancelSession"/>
    /// </summary>
    public delegate void StopAllSessionsHandler(long finishTime);

    /// <summary>
    /// Handler for <see cref="IReceiver.CancelSession"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="cancelTime">currentTimeMillis when session cancelled</param>
    public delegate void CancelSessionHandler(string sessionUid, long cancelTime);

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