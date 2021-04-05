using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        MainOptions Options { get; set; }

        void ValidateOptions();
        void CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers);

        IEnumerable<string> GetAssemblies(string directory);
        AssemblyVersion GetAssemblyVersion(string filePath);

        InjectedSolution ReadInjectedTree(string path);
        void WriteInjectedTree(string path, InjectedSolution tree);
        string GetTreeFilePath(InjectedSolution tree);
        string GetTreeFileHintPath(string path);
        string GenerateTreeFilePath(string targetDir);
    }
}