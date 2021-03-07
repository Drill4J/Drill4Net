using System;

namespace Plugins.Logger
{
    public class TestPlugin
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
