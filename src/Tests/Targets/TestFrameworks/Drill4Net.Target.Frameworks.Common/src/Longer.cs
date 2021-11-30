using System.Threading;

namespace Drill4Net.Target.Frameworks.Common
{
    public class Longer
    {
        public void DoLongWork(int num = 5000)
        {
            if(num < 100)
                num = 100;
            if (num > 10_000)
                num = 10_000;
            Thread.Sleep(num);
        }
    }
}
