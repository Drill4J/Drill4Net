using System;
using System.IO;
using System.Linq;

namespace Drill4Net.Common
{
    /// <summary>
    /// Options for the Filter of directories, folders, namespaces, type names, etc
    /// </summary>
    [Serializable]
    public class SourceFilterOptions
    {
        private readonly TypeChecker _typeChecker;

        /**********************************************************************/

        public SourceFilterOptions()
        {
            _typeChecker = new TypeChecker();
        }

        /**********************************************************************/

        /// <summary>
        /// What are we include in Target. <see cref="Includes"/> takes precedence over it
        /// </summary>
        public SourceFilterParams Includes { get; set; }

        /// <summary>
        /// What are we skip in Target. It takes precedence over <see cref="Includes"/>
        /// </summary>
        public SourceFilterParams Excludes { get; set; }

        /*********************************************************/

        public bool IsDirectoryNeed(string path)
        {
            if (Excludes?.IsDirectoryListed(path) == true)
                return false;
            if (Includes?.Directories == null || !Includes.Directories.Any())
                return true;
            return Includes.IsDirectoryListed(path);
        }

        public bool IsFolderNeed(string folder)
        {
            if (Excludes?.Folders?.Contains("*") == true)
                return false;
            if (Excludes?.IsFolderListed(folder) == true)
                return false;
            if (Includes?.Folders == null || !Includes.Folders.Any())
                return true;
            return Includes.IsFolderListed(folder);
        }

        public bool IsFileNeedByPath(string filePath)
        {
            if (!IsFileNeed(Path.GetFileName(filePath)))
                return false;
            if (!IsNamespaceNeed(Path.GetFileNameWithoutExtension(filePath))) //TODO: FileName regex in IsFileNeed!
                return false;
            if (!_typeChecker.CheckByAssemblyPath(filePath))
                return false;
            return true;
        }

        public bool IsFileNeed(string name)
        {
            if (Excludes?.IsFileListed(name) == true)
                return false;
            if (Includes?.Files == null || !Includes.Files.Any())
                return true;
            return Includes.IsFileListed(name);
        }

        public bool IsNamespaceNeed(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
                return false;
            if (Excludes?.IsNamespaceListedExactly(ns) == true)
                return false;
            if (Includes?.Namespaces == null || !Includes.Namespaces.Any())
                return true;
            foreach (var nsPart in Includes.Namespaces)
            {
                if (ns.StartsWith(nsPart))
                    return true;
            }
            return false;
        }

        public bool IsClassNeed(string fullName)
        {
            if (Excludes?.IsClassListed(fullName) == true)
                return false;
            if (Includes?.Classes == null || !Includes.Classes.Any())
                return !_typeChecker.IsSystemType(fullName);
            return Includes.IsClassListed(fullName);
        }

        public bool IsAttributeNeed(string name)
        {
            if (Excludes?.IsAttributeListed(name) == true)
                return false;
            if (Includes?.Attributes == null || !Includes.Attributes.Any())
                return true;
            return Includes.IsAttributeListed(name);
        }
    }
}
