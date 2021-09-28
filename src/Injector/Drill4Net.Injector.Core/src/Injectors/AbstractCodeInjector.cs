using Mono.Cecil;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractCodeInjector : BaseCodeInjector
    {
        public abstract void InjectTo(AssemblyDefinition assembly, bool isNetFX = false);
    }
}
