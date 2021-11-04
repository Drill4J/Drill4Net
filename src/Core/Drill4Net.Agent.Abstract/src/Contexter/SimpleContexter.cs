using System.Threading;

namespace Drill4Net.Agent.Abstract
{
    public class SimpleContexter : AbstractContexter
    {
        public SimpleContexter() : base(nameof(SimpleContexter))
        {
        }

        /**********************************************************************/

        /// <summary>
        /// Gets the context identifier.
        /// </summary>
        /// <returns></returns>
        public override string GetContextId()
        {
            var ctx = Thread.CurrentThread.ExecutionContext;
            return ctx?.GetHashCode().ToString() ?? "";
        }

        public override void RegisterCommand(int command, string data)
        {
            //nothing
        }
    }
}
