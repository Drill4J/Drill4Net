using System;
using System.Collections.Generic;
using System.Text;

namespace Drill4Net.Agent.Standard
{
    public interface ISender
    {
        public void Send(string messageType, string message);
    }
}
