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
        public event SessionStartHandler SessionStart;

        /// <summary>
        /// Session must stopped
        /// </summary>
        public event SessionStopHandler SessionStop;
        
        /// <summary>
        /// All session must stopped
        /// </summary>
        public event SessionStopAllHandler SessionStopAll;

        /// <summary>
        /// Session is cancelled on admin side
        /// </summary>
        public event SessionCancellHandler SessionCancell;

        /// <summary>
        /// All sessions are cancelled on admin side
        /// </summary>
        public event SessionCancellAllHandler SessionCancellAll;
        #endregion
    }
}