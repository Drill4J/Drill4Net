using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class CoverDataPart : AbstractMessage
    {
        public string sessionId { get; set; }

        public List<ExecClassData> data { get; set; }

        public CoverDataPart() : base(AgentConstants.MESSAGE_OUT_COVERAGE_DATA_PART)
        {
        }
    }
}