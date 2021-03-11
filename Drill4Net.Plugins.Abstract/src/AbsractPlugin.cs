using System.Threading.Tasks;

namespace Drill4Net.Plugins.Abstract
{
    public abstract class AbsractPlugin
    {
        public abstract void Register(string data);

        public Task ProcessAsync(string data)
        {
            return Task.Run(() => Register(data));
        }
    }
}
