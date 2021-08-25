using System.Threading;

namespace Drill4Net.Common
{
    public static class Contexter
    {
        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <returns></returns>
        public static string GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode().ToString() ?? "";
        }
    }
}
