using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        MainOptions CreateOptions(string[] args);
        string GetInjectedDirectoryName([NotNull] string original, [NotNull] MainOptions opts);
        void ValidateOptions([NotNull] MainOptions opts);
    }
}