namespace Drill4Net.Agent.Standard
{
    public delegate void MessageReceivedHandler(string message);

    public interface IReceiver
    {
        public event MessageReceivedHandler ReceivedHandler;
    }
}
