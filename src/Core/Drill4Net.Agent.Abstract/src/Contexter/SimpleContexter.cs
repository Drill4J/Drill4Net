﻿using System.Threading;

namespace Drill4Net.Agent.Abstract
{
    public class SimpleContexter : AbstractEngineContexter
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

        public override TestEngine GetTestEngine()
        {
            return null;
        }

        public override bool RegisterCommand(int command, string data)
        {
            return true;
        }
    }
}
