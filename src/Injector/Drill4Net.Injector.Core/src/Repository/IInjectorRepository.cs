using Drill4Net.Profiling.Tree;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        MainOptions Options { get; set; }

        void NormalizePathes(MainOptions opts);
        void ValidateOptions();
        void ValidateOptions(MainOptions opts);

        string GetFullPath(string path);
        string GetSourceDirectory(MainOptions opts);
        string GetDestinationDirectory(MainOptions opts, string currentDir);
        void CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers);
        void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs = true);

        IEnumerable<string> GetAssemblies(string directory);
        AssemblyVersion GetAssemblyVersion(string filePath);

        InjectedSolution ReadInjectedTree(string path);
        void WriteInjectedTree(string path, InjectedSolution tree);
        string GetTreeFilePath(InjectedSolution tree);
        string GetTreeFileHintPath(string path);
        string GenerateTreeFilePath(string targetDir);
    }
}