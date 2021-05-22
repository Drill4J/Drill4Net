using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        InjectorOptions Options { get; set; }
        IInjector GetInjector();
        CodeHandlerStrategy GetStrategy();
        void ValidateOptions();
        void CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers);

        IEnumerable<string> GetAssemblies(string directory);
        AssemblyVersioning GetAssemblyVersion(string filePath);

        InjectedSolution ReadInjectedTree(string path);
        void WriteInjectedTree(string path, InjectedSolution tree);
        string GetTreeFilePath(InjectedSolution tree);
        string GetTreeFileHintPath(string path);
        string GetTreeFilePath(string targetDir);
    }
}