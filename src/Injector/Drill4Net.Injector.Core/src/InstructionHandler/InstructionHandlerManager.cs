using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class InstructionHandlerManager
    {
        public string Name { get; set; }

        public List<AbstractInstructionHandler> Handlers { get; set; }

        public string Description { get; set; }

        /***********************************************************************************/

        public InstructionHandlerManager(List<AbstractInstructionHandler> handlers = null)
        {
            Handlers = handlers;
        }
    }
}
