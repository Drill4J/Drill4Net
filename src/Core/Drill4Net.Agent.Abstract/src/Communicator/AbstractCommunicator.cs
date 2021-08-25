using System;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Abstract communicator interacing with some admin side
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.ICommunicator" />
    public abstract class AbstractCommunicator : ICommunicator
    {
        /// <summary>
        /// Receiver of the data from Admin side.
        /// </summary>
        /// <value>
        /// The receiver.
        /// </value>
        public IAgentReceiver Receiver { get; protected set; }

        /// <summary>
        /// Sender of the data from Admin side.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public AbstractSender Sender { get; protected set; }
        
        /******************************************************************************/

        protected AbstractCommunicator() { }

        protected AbstractCommunicator(IAgentReceiver receiver, AbstractSender sender)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /******************************************************************************/

        /// <summary>
        /// Connect to the Admin side.
        /// </summary>
        public abstract void Connect();
    }
}
