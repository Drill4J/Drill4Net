using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    public class Communicator : ICommunicator
    {
        public event MessageReceivedHandler ReceivedHandler;

        /***********************************************************/

        public Communicator()
        {
        }

        /***********************************************************/

        public void Send(string messageType, string message)
        {
            throw new System.NotImplementedException();
        }
    }
}
