using System;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public record SwitchActiveScope : OutgoingMessage
    {
        public ActiveScopeChangePayload payload { get; set; }

        /******************************************************************************/

        public SwitchActiveScope() : base(AgentConstants.MESSAGE_OUT_SCOPE_SWITCH)
        {
            payload = new ActiveScopeChangePayload();
        }

        /******************************************************************************/

        public override string ToString()
        {
            return $"{type} -> {payload}";
        }
    }
}
