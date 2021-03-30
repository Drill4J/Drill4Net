using System;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.RnD
{
    public class PerfAgent : AbsractAgent
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
