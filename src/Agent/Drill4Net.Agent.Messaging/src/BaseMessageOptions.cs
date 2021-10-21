using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Messaging
{
    public class BaseMessageOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public List<string> Topics { get; set; }

        /******************************************************/

        public BaseMessageOptions()
        {
            Servers = new();
            Topics = new();
        }
    }
}
