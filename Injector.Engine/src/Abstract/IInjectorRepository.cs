using System.Diagnostics.CodeAnalysis;

namespace Injector.Engine
{
    public interface IInjectorRepository
    {
        InjectOptions CreateOptions(string[] args);
        string GetInjectedDirectoryName([NotNull] string original);
        void ValidateOptions([NotNull] InjectOptions opts);
    }
}