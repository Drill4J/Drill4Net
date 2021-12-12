using Drill4Net.Agent.Abstract;

namespace Drill4Net.Admin.Requester
{
    public record TestToRunInfoResponse
    {
        public string Name { get; set; }

        public TestDetails Details { get; set; }

        /****************************************************/

        public override string ToString()
        {
            return $"{Name} -> {Details}";
        }
    }
}
