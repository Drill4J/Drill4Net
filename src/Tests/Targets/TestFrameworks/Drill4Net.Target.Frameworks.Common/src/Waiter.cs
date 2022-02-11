using System.Threading;

namespace Drill4Net.Target.Frameworks.Common
{
    /// <summary>
    /// Class to test simple long operations
    /// </summary>
    public class Waiter
    {
        /// <summary>
        /// Just waiting for specified time
        /// </summary>
        /// <param name="num"></param>
        public void Wait(int num = 5000)
        {
            if(num < 100)
                num = 100;
            if (num > 10_000)
                num = 10_000;
            Thread.Sleep(num);
        }
    }
}
