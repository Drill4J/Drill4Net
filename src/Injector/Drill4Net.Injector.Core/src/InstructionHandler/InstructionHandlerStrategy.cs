namespace Drill4Net.Injector.Core
{
    public class InstructionHandlerStrategy
    {
        public string Name { get; set; }

        public string Description { get; set; }

        private AbstractInstructionHandler _starter;
        private AbstractInstructionHandler _last;

        /***********************************************************************************/

        public void ConnectHandler(AbstractInstructionHandler handler)
        {
            if (_starter == null)
                _starter = handler;
            else
                _last.Successor = handler;
            //
            _last = handler;
        }

        public virtual void Handle(InjectorContext ctx)
        {
            if (_starter != null)
                _starter.HandleRequest(ctx);
        }
    }
}
