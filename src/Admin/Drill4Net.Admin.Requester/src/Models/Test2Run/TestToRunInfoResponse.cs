using Drill4Net.Agent.Abstract;

namespace Drill4Net.Admin.Requester
{
    public record TestToRunInfoResponse
    {
        public string Name { get; set; }

        public Test2RunMetadata Metadata { get; set; }

        /******************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}
