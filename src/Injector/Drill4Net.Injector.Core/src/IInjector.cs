namespace Drill4Net.Injector.Core
{
    public interface IInjector
    {
        void Inject(RunContext runCtx, AssemblyContext asmCtx);
    }
}