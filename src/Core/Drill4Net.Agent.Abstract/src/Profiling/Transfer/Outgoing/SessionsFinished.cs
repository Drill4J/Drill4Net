using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    public class SessionsFinished
    {
        public List<string> IDs { get; set; }
        public long Ts { get; set; }
    }
}