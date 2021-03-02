using System.Diagnostics.CodeAnalysis;

namespace Injector.Core
{
    public interface IInjectorRepository
    {
        InjectOptions CreateOptions(string[] args);
        string GetInjectedDirectoryName([NotNull] string original);
        void ValidateOptions([NotNull] InjectOptions opts);
    }
}