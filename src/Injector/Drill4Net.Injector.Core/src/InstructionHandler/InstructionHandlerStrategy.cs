namespace Drill4Net.Injector.Core
{
    public class InstructionHandlerStrategy
    {
        public string Name { get; set; }

        public string Description { get; set; }

        private AbstractBaseHandler _first;
        private AbstractBaseHandler _last;

        /***********************************************************************************/

        protected void ConnectHandler(AbstractBaseHandler handler)
        {
            if (_first == null)
                _first = handler;
            else
                _last.Successor = handler;
            //
            _last = handler;
        }

        public virtual void StartMethod(InjectorContext ctx)
        {
            if (_first != null)
                _first.StartMethod(ctx);
        }

        public virtual void HandleInstruction(InjectorContext ctx)
        {
            _first?.HandleInstruction(ctx);
        }
    }
}
