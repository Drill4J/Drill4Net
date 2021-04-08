using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class InstructionHandlerStrategy
    {
        public string Name { get; set; }

        public List<AbstractInstructionHandler> Handlers { get; set; }

        public string Description { get; set; }

        /***********************************************************************************/

        public InstructionHandlerStrategy(List<AbstractInstructionHandler> handlers = null)
        {
            Handlers = handlers;
        }
    }
}
