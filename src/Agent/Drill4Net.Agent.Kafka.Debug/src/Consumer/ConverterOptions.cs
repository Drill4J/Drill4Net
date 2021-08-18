using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class ConverterOptions : AbstractOptions
    {
        public List<string> Servers { get; set; }
        public string GroupId { get; set; }

        /// <summary>
        /// Gets or sets the topics for retrieving the Target's probes.
        /// </summary>
        /// <value>
        /// The topics.
        /// </value>
        public List<string> Topics { get; set; }

        public ConverterOptions()
        {
            Servers = new();
            Topics = new();
        }
    }
}
