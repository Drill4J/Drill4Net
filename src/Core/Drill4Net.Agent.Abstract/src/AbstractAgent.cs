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
    }
}
