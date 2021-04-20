using System.Collections.Generic;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Standard
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    public delegate void SessionStartedHandler(string sessionUid, string testType, bool isRealTime, long startTime);
    public delegate void SessionFinishedHandler(string sessionUid, long finishTime);
    public delegate void SessionCancelledHandler(string sessionUid, long cancelTime);
    public delegate void SessionChangedHandler(string sessionUid, int probeCnt);
    public delegate void AllSessionsCancelledHandler();
    public delegate void CoverageDataPartHandler(string sessionUid, List<ExecClassData> data);
    
    /******************************************************************/
    
    public class AgentReceiver
    {
        public event SessionStartedHandler SessionStarted;
        public event SessionFinishedHandler SessionFinished;
        public event SessionChangedHandler SessionChanged;
        public event SessionCancelledHandler SessionCancelled;
        public event AllSessionsCancelledHandler AllSessionsCancelled;
        public event CoverageDataPartHandler CoverageDataPartReceived;
    }
}