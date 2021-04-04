using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    public interface IInjectorEngine
    {
        InjectedSolution Process();
        InjectedSolution Process( MainOptions opts);
    }
}