using Drill4Net.Injector.Core;
using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Injector.Engine
{
    public interface IInjectorEngine
    {
        InjectedSolution Process();
        InjectedSolution Process([NotNull] MainOptions opts);
    }
}