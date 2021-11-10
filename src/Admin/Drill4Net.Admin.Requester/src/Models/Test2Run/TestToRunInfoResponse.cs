using System.Collections.Generic;

namespace Drill4Net.Admin.Requester
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
