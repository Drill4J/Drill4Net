namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Base strategy for the processing the IL code
    /// </summary>
    public class CodeHandlerStrategy
    {
        /// <summary>
        /// Name of the strategy
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the strategy
        /// </summary>
        public string Description { get; set; }

        private AbstractBaseHandler _first;
        private AbstractBaseHandler _last;

        /***********************************************************************************/

        /// <summary>
        /// Add handler to the chain of the strategy's handlers
        /// </summary>
        /// <param name="handler">Some handler</param>
        protected void AddHandler(AbstractBaseHandler handler)
        {
            if (_first == null)
                _first = handler;
            else
                _last.Successor = handler;
            //
            _last = handler;
        }

        /// <summary>
        /// Primary act for the chain of the handlers on whole set of IL code (if some one implement it)
        /// </summary>
        /// <param name="ctx"></param>
        public virtual void Preprocess(MethodContext ctx)
        {
            _first?.Preprocess(ctx);
        }

        public virtual void HandleInstruction(MethodContext ctx)
        {
            _first?.HandleInstruction(ctx);
        }

        /// <summary>
        /// Post act for the chain of the handlers on whole set of IL code (if some one implement it)
        /// </summary>
        /// <param name="ctx"></param>
        public virtual void Postprocess(MethodContext ctx)
        {
            _first?.Postprocess(ctx);
        }
    }
}
