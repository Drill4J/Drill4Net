using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Messaging
{
    public class MessagerOptions : AbstractOptions
    {
        public List<string> Servers { get; set; } = new();
        public MessageSenderOptions Sender { get; set; }
        public MessageReceiverOptions Receiver { get; set; }
    }
}
