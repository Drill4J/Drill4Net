using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    public class InitDataPart: AbstractOutgoingMessage
    {
        public List<AstEntity> AstEntities { get; set; }

        public InitDataPart() : base(AgentConstants.MESSAGE_OUT_INIT_DATA_PART)
        {
        }
    }
}