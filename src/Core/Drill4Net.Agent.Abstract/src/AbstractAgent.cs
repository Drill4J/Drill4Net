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

        public abstract void RegisterWithContext(string data, string ctx);

        /// <summary>
        /// Async register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public Task RegisterAsync(string data)
        {
            return Task.Run(() => Register(data));
        }

        /// <summary>
        /// Async register the cross-point's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public Task RegisterWithContextAsync(string data, string ctx)
        {
            return Task.Run(() => RegisterWithContext(data, ctx));
        }
    }
}
