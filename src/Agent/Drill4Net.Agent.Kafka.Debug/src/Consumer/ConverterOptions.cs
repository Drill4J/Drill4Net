using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class ConverterOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public string GroupId { get; set; }
        public string Topic { get; set; }
    }
}
