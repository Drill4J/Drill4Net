using System;
using System.Threading.Tasks;

namespace Drill4Net.Target.Core31
{
    public class InjectTarget
    {
        public async Task RunTests()
        {
            var common = new Common.InjectTarget();
            await common.RunTests();
        }
    }
}
