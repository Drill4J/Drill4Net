using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorEngine
    {
        InjectedSolution Process();
        InjectedSolution Process(MainOptions opts);
    }
}