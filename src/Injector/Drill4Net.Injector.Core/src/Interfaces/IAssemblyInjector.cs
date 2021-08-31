namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Interface of Injector of instrumenting code called by Target for Agent
    /// </summary>
    public interface IAssemblyInjector
    {
        /// <summary>
        /// Inject assembly by specified contexts
        /// </summary>
        /// <param name="runCtx"></param>
        /// <param name="asmCtx"></param>
        void Inject(RunContext runCtx, AssemblyContext asmCtx);
    }
}