namespace Drill4Net.Injector.Core
{
    public class InstructionHandlerStrategy
    {
        public string Name { get; set; }

        public string Description { get; set; }

        private AbstractBaseHandler _starter;
        private AbstractBaseHandler _last;

        /***********************************************************************************/

        protected void ConnectHandler(AbstractBaseHandler handler)
        {
            if (_starter == null)
                _starter = handler;
            else
                _last.Successor = handler;
            //
            _last = handler;
        }

        public virtual void StartMethod(InjectorContext ctx)
        {
            if (_starter != null)
                _starter.StartMethod(ctx);
        }

        public virtual void HandleInstruction(InjectorContext ctx)
        {
            if (_starter != null)
                _starter.HandleInstruction(ctx);
        }
    }
}
