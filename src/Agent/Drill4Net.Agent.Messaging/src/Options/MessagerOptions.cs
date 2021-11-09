using Drill4Net.Configuration;
using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging
{
    public class MessagerOptions : AbstractOptions
    {
        public List<string> Servers { get; set; } = new();
        public MessageSenderOptions Sender { get; set; }
        public MessageReceiverOptions Receiver { get; set; }
    }
}
