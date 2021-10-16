using System.Threading;

namespace Drill4Net.Target.Testers.Common
{
    public class Longer
    {
        public int DoLongWork(int num = 5000)
        {
            Thread.Sleep(num);
            return 0;
        }
    }
}
