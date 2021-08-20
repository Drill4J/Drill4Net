using System.Threading.Tasks;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Abstract Agent collecting probe data of the cross-point in instrumented Target
    /// </summary>
    public abstract class AbstractAgent
    {
        /// <summary>
        /// Register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public abstract void Register(string data, string ctx = null);

        /// <summary>
        /// Async register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public Task ProcessAsync(string data, string ctx = null)
        {
            return Task.Run(() => Register(data, ctx));
        }
    }
}
