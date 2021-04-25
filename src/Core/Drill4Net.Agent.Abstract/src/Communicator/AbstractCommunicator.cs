using System;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCommunicator : ICommunicator
    {
        public IReceiver Receiver { get; protected set; }
        public AbstractSender Sender { get; protected set; }

        /***************************************************************************/

        protected AbstractCommunicator() { }

        protected AbstractCommunicator(IReceiver receiver, AbstractSender sender)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }
    }
}
