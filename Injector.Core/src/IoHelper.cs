using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Injector.Core
{
    public static class IoHelper
    {
        public static bool IsSameDirectories([NotNull] string dir1, [NotNull] string dir2)
        {
            if (!dir1.EndsWith("\\"))
                dir1 += "\\";
            if (!dir2.EndsWith("\\"))
                dir2 += "\\";
            return dir1 == dir2;
        }

        public static IEnumerable<string> GetAssemblies([NotNull] string directory, EnumerationOptions searchOpts)
        {
            return Directory.EnumerateFiles(directory, "*", searchOpts)
                .Where(a => a.EndsWith(".exe") || a.EndsWith(".dll"));
        }

        public static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir}");

            var dirs = dir.GetDirectories();  
            Directory.CreateDirectory(destDir);

            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
