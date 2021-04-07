using System;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractInstructionHandler
    {
        public string Name { get; set; }

        public Func<int, bool> Condition { get; set; }

        public AbstractInstructionHandler Successor { get; set; }

        /*****************************************************************************/

        protected AbstractInstructionHandler(string name, Func<int, bool> condition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /*****************************************************************************/

        public abstract void HandleRequest(int value);
    }
}
