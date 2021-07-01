using System;

namespace Drill4Net.Agent.RnD
{
    public class PerfAgent
    {
        public static void Do(long cnt)
        {
            var a = double.MinValue;
            for (var i = 0; i < cnt; i++)
            {
                a += Math.Sin(i);
            }
        }
    }
}
