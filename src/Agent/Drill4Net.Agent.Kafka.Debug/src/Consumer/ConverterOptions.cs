using Drill4Net.Common;
using System.Collections.Generic;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class ConverterOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public string GroupId { get; set; }
        public string Topic { get; set; }
    }
}
