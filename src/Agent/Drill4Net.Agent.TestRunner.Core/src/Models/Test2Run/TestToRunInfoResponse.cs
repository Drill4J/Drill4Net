using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    public record TestToRunInfoResponse
    {
        public string Name { get; set; }

        public Dictionary<string, string> Metadata { get; set; } //<string, object>

        /******************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}
