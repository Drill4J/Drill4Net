namespace Drill4Net.Agent.Abstract
{
    public interface IReceiver
    {
        event AllSessionsCancelledHandler AllSessionsCancelled;
        event SessionCancelledHandler SessionCancelled;
        event SessionChangedHandler SessionChanged;
        event SessionStartedHandler SessionStarted;
        event SessionStopHandler SessionStop;
    }
}