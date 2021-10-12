using Drill4Net.Agent.Abstract;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    public record TestToRunInfoResponse
    {
        public string Name { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        /******************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}
