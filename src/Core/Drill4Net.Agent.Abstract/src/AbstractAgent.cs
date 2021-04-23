using System;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractAgent
    {
        public abstract void Register(string data);

        public Task ProcessAsync(string data)
        {
            return Task.Run(() => Register(data));
        }

        public static long GetCurrentUnixTimeMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static DateTime ConvertUnixTime(long ts)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(ts);
            return offset.DateTime;
        }
    }
}
