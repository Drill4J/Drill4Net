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
        public abstract void Register(string data);

        /// <summary>
        /// Async register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public Task ProcessAsync(string data)
        {
            return Task.Run(() => Register(data));
        }
    }
}
