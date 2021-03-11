using Drill4Net.Injector.Core;
using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Injector.Engine
{
    public interface IInjectorEngine
    {
        void Process();
        void Process([NotNull] MainOptions opts);
    }
}