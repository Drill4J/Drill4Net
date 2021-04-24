namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public abstract class AbstractReceiver : IReceiver
    {
        #region Events
        /// <summary>
        /// Session is started on admin side
        /// </summary>
        public event SessionStartHandler StartSession;

        /// <summary>
        /// Session must stopped
        /// </summary>
        public event SessionStopHandler StopSession;
        
        /// <summary>
        /// All session must stopped
        /// </summary>
        public event SessionStopAllHandler StopAllSessions;

        /// <summary>
        /// Session is cancelled on admin side
        /// </summary>
        public event SessionCancelHandler CancelSession;

        /// <summary>
        /// All sessions are cancelled on admin side
        /// </summary>
        public event SessionCancelAllHandler CancelAllSessions;
        #endregion
    }
}