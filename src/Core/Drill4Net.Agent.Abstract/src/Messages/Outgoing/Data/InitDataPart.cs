using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class InitDataPart: BaseMessage
    {
        public List<AstEntity> astEntities { get; set; }

        public InitDataPart() : base(AgentConstants.MESSAGE_OUT_INIT_DATA_PART)
        {
        }
    }
}