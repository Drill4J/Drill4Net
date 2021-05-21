using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Testing
{
    public class TesterRepository : AbstractRepository<TesterOptions, BaseOptionsHelper<TesterOptions>>
    {
        public TesterRepository(string cfgPath) : base(cfgPath)
        {
        }
    }
}
