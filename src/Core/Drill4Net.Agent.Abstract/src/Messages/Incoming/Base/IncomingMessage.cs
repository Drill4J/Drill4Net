using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    /// <summary>
    /// The class as label for incoming messages
    /// (AgentAction class on Kotlin on admin side)
    /// </summary>
    [Serializable]
    public class IncomingMessage : AbstractMessage
    {
        public IncomingMessage():base("NOT_INIT") { }

        protected IncomingMessage(string type): base(type)
        {
        }
    }
}