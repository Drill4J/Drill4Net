using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        MainOptions GetOptions(string[] args);
        void ValidateOptions(MainOptions opts);
    }
}