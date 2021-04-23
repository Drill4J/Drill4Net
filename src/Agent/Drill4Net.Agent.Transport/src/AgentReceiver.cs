using System;
using Websocket.Client;
using Websocket.Client.Models;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    #region Delegates
    /// <summary>
    /// Handler for <see cref="AgentReceiver.SessionStarted"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="testType">MANUAL or AUTO</param>
    /// <param name="isRealTime"></param>
    /// <param name="startTime">currentTimeMillis when session start</param>
    public delegate void SessionStartedHandler(string sessionUid, string testType, bool isRealTime, long startTime);

    /// <summary>
    /// Handler for <see cref="AgentReceiver.SessionStop"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="finishTime">currentTimeMillis when session finished</param>
    public delegate void SessionStopHandler(string sessionUid, long finishTime);

    /// <summary>
    /// Handler for <see cref="AgentReceiver.SessionCancelled"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="cancelTime">currentTimeMillis when session cancelled</param>
    public delegate void SessionCancelledHandler(string sessionUid, long cancelTime);

    /// <summary>
    /// Handler for <see cref="AgentReceiver.SessionChanged"/>
    /// </summary>
    /// <param name="sessionUid"></param>
    /// <param name="probeCnt"></param>
    public delegate void SessionChangedHandler(string sessionUid, int probeCnt);

    /// <summary>
    /// Handler for <see cref="AgentReceiver.AllSessionsCancelled"/>
    /// </summary>
    public delegate void AllSessionsCancelledHandler();
    #endregion

    /****************************************************************************/

    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public class AgentReceiver : AbstractReceiver
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

        private readonly WebsocketClient _receiver;

        /************************************************************************/

        public AgentReceiver(WebsocketClient receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _receiver.ReconnectionHappened.Subscribe(info => ReconnectHandler(info));
            _receiver.MessageReceived.Subscribe(msg => ReceivedHandler(msg));
            _receiver.Start();
        }

        /************************************************************************/

        private void ReceivedHandler(ResponseMessage message)
        {

        }

        private void ReconnectHandler(ReconnectionInfo info)
        {

        }
    }
}