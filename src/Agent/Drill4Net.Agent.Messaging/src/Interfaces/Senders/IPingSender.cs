using System;
using System.Collections.Specialized;

namespace Drill4Net.Agent.Messaging
{
    public interface IPingSender : IDisposable
    {
        void SendPing(StringDictionary state, string topic = null);
    }
}
