using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Drill4Net.Target.Interfaces;

[assembly: InternalsVisibleTo("Drill4Net.Target.NetCore.Tests")]

namespace Drill4Net.Target.Core31
{
    public class InjectTarget : IInjectTarget
    {
        public async Task RunTests()
        {
            var common = new Common.InjectTarget();
            await common.RunTests();
        }
    }
}
