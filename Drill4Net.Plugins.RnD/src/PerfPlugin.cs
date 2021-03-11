using System;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.RnD
{
    public class PerfPlugin : AbsractPlugin
    {
        public static void Do(long cnt)
        {
            var a = double.MinValue;
            for (var i = 0; i < cnt; i++)
            {
                a += Math.Sin(i);
            }
        }

        public override void Register(string data)
        {
            if (!long.TryParse(data, out long cnt))
                throw new ArgumentException(nameof(data));
            Do(cnt);
        }
    }
}
