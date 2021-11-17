using System.Threading;

namespace Drill4Net.Agent.Abstract
{
    public class SimpleContexter : IEngineContexter
    {
        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <returns></returns>
        public string GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return $"$$$_{ctx?.GetHashCode()}";
        }

        public TestEngine GetTestEngine()
        {
            return null;
        }

        public bool RegisterCommand(int command, string data)
        {
            return true;
        }
    }
}
