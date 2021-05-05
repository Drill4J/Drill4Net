using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class InitActiveScope : IncomingMessage
    {
        public InitScopePayload Payload { get; set; }

        public InitActiveScope()
        {
        }

        public InitActiveScope(InitScopePayload payload): base(AgentConstants.MESSAGE_IN_INIT_ACTIVE_SCOPE)
        {
            Payload = payload;
        }
    }
}