namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    #region Delegates
    /// <summary>
    /// Handler for <see cref="IReceiver.SessionStart"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="testType">MANUAL or AUTO</param>
    /// <param name="isRealTime"></param>
    /// <param name="startTime">currentTimeMillis when session start</param>
    public delegate void SessionStartHandler(string sessionUid, string testType, bool isRealTime, long startTime);

    /// <summary>
    /// Handler for <see cref="IReceiver.SessionStop"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="finishTime">currentTimeMillis when session finished</param>
    public delegate void SessionStopHandler(string sessionUid, long finishTime);
    
    /// <summary>
    /// Handler for <see cref="IReceiver.SessionCancell"/>
    /// </summary>
    public delegate void SessionStopAllHandler(long finishTime);

    /// <summary>
    /// Handler for <see cref="IReceiver.SessionCancell"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="cancelTime">currentTimeMillis when session cancelled</param>
    public delegate void SessionCancellHandler(string sessionUid, long cancelTime);

    /// <summary>
    /// Handler for <see cref="IReceiver.SessionCancellAll"/>
    /// </summary>
    public delegate void SessionCancellAllHandler();
    #endregion
    
    public interface IReceiver
    {
        /// <summary>
        /// Session is started on admin side
        /// </summary>
        event SessionStartHandler SessionStart;

        /// <summary>
        /// Session must stopped
        /// </summary>
        event SessionStopHandler SessionStop;
        
        /// <summary>
        /// All session must stopped
        /// </summary>
        event SessionStopAllHandler SessionStopAll;

        /// <summary>
        /// Session is cancelled on admin side
        /// </summary>
        event SessionCancellHandler SessionCancell;

        /// <summary>
        /// All sessions are cancelled on admin side
        /// </summary>
        event SessionCancellAllHandler SessionCancellAll;
    }
}