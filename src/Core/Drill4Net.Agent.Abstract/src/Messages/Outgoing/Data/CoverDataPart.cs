using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class CoverDataPart : AbstractMessage
    {
        public string SessionId { get; set; }
        public List<ExecClassData> Data { get; set; }

        public CoverDataPart() : base(AgentConstants.MESSAGE_OUT_COVERAGE_DATA_PART)
        {
        }
    }
}