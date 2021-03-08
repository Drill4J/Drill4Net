using System;

namespace Plugins.Test
{
    public class PerfPlugin
    {
        public static void Do(long cnt)
        {
            var a = double.MinValue;
            for (var i = 0; i < cnt; i++)
            {
                a += Math.Sin(i);
            }
        }

        public void InstanceDo(long cnt)
        {
            Do(cnt);
        }
    }
}
