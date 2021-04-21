namespace Drill4Net.Agent.Abstract
{
    public delegate void MessageReceivedHandler(string message);

    public interface IReceiver
    {
        public event MessageReceivedHandler ReceivedHandler;
    }
}
