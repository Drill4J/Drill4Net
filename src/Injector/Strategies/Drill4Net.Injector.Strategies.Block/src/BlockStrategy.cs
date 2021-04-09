using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Strategies.Block
{
    public class BlockStrategy : InstructionHandlerStrategy
    {
        public BlockStrategy()
        {
            ConnectHandler(new BlockHandler());
        }
    }
}
