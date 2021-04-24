using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract.Transfer
{
    public class CoverDataPart
    {
        public string SessionId { get; set; }
        public List<ExecClassData> Data { get; set; }
    }
}