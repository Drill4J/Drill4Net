using System.Threading;

namespace Drill4Net.Target.Frameworks.Common
{
    public class Longer
    {
        public void DoLongWork(int num = 5000)
        {
            Thread.Sleep(num);
        }
    }
}
