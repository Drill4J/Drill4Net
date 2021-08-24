using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging
{
    public interface IPingSender : IDisposable
    {
        void SendPing(Dictionary<string, string> state, string topic = null);
    }
}
