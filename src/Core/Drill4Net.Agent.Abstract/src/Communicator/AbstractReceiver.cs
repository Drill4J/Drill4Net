namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    #region Delegates
    /// <summary>
    /// Handler for <see cref="AbstractReceiver.SessionStarted"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="testType">MANUAL or AUTO</param>
    /// <param name="isRealTime"></param>
    /// <param name="startTime">currentTimeMillis when session start</param>
    public delegate void SessionStartedHandler(string sessionUid, string testType, bool isRealTime, long startTime);

    /// <summary>
    /// Handler for <see cref="AbstractReceiver.SessionStop"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="finishTime">currentTimeMillis when session finished</param>
    public delegate void SessionStopHandler(string sessionUid, long finishTime);

    /// <summary>
    /// Handler for <see cref="AbstractReceiver.SessionCancelled"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="cancelTime">currentTimeMillis when session cancelled</param>
    public delegate void SessionCancelledHandler(string sessionUid, long cancelTime);

    /// <summary>
    /// Handler for <see cref="AbstractReceiver.SessionChanged"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="probeCnt"></param>
    public delegate void SessionChangedHandler(string sessionUid, int probeCnt);

    /// <summary>
    /// Handler for <see cref="AbstractReceiver.AllSessionsCancelled"/>
    /// </summary>
    public delegate void AllSessionsCancelledHandler();
    #endregion

    /****************************************************************************/

    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public abstract class AbstractReceiver : IReceiver
    {
        #region Events
        /// <summary>
        /// Session is started on admin side
        /// </summary>
        public event SessionStartedHandler SessionStarted;

        /// <summary>
        /// Session must stopped
        /// </summary>
        public event SessionStopHandler SessionStop;

        /// <summary>
        /// ???
        /// </summary>
        public event SessionChangedHandler SessionChanged;

        /// <summary>
        /// Session is cancelled on admin side
        /// </summary>
        public event SessionCancelledHandler SessionCancelled;

        /// <summary>
        /// All sessions are cancelled on admin side
        /// </summary>
        public event AllSessionsCancelledHandler AllSessionsCancelled;
        #endregion
    }
}