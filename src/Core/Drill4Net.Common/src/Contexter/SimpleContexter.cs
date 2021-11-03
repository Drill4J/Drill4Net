using System.Threading;

namespace Drill4Net.Common
{
    public class SimpleContexter : IContexter
    {
        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <returns></returns>
        public string GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode().ToString() ?? "";
        }
    }
}
