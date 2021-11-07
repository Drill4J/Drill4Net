using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Messaging
{
    public class BaseMessageOptions : AbstractOptions
    {
        public List<string> Topics { get; set; } = new();
    }
}
