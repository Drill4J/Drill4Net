using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Configurator
{
    public class SystemConfiguration
    {
        public string? AdminUrl { get; set; }
        public string? MiddlewareUrl { get; set; }
        public string? InjectorPluginDirectory { get; set; }
        public string? AgentPluginDirectory { get; set; }
        public List<LogData> Logs { get; }

        /*************************************************/

        public SystemConfiguration()
        {
            Logs = new();
        }
    }
}
