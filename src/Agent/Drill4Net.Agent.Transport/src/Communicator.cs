using Drill4Net.Agent.Standard;

namespace Drill4Net.Agent.Transport
{
    public class Communicator : ISender, IReceiver
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
