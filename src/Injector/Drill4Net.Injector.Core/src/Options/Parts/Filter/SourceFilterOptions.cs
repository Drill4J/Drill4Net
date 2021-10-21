using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core
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
            if (Excludes?.Directories != null &&
                FilterHelper.IsMatchRegexFilterPattern(path.EndsWith("\\") ? path : $"{path}\\", Excludes.Directories))
                return false;
            if (Includes?.Directories == null || Includes.Directories.Count == 0)
                return true;
            if (Includes.IsDirectoryListed(path))
                return true;
            return FilterHelper.IsMatchRegexFilterPattern(path.EndsWith("\\")? path:$"{path}\\", Includes.Directories);
        }

        public bool IsFolderNeed(string folder)
        {
            if (Excludes?.Folders?.Contains("*") == true)
                return false;
            if (Excludes?.IsFolderListed(folder) == true)
                return false;
            if (Excludes?.Folders != null &&
                FilterHelper.IsMatchRegexFilterPattern(folder, Excludes.Folders))
                return false;
            if (Includes?.Folders == null || Includes.Folders.Count == 0)
                return true;
            if (Includes.IsFolderListed(folder))
                return true;
            return FilterHelper.IsMatchRegexFilterPattern(folder, Includes.Folders);
        }

        public bool IsFileNeedByPath(string filePath)
        {
            if (!IsFileNeed(Path.GetFileName(filePath)))
                return false;
            if (!IsNamespaceNeed(Path.GetFileNameWithoutExtension(filePath))) //TODO: FileName regex in IsFileNeed!
                return false;
            //
            if (!_typeChecker.CheckByAssemblyPath(filePath))
                return false;
            return true;
        }

        public bool IsFileNeed(string name)
        {
            if (Excludes?.IsFileListed(name) == true)
                return false;
            if (Excludes?.Files != null &&
                FilterHelper.IsMatchRegexFilterPattern(name, Excludes.Files))
                return false;
            //
            if (Includes?.Files == null || Includes.Files.Count == 0)
                return true;
            if (Includes.IsFileListed(name))
                return true;
            return FilterHelper.IsMatchRegexFilterPattern(name, Includes.Files);
        }

        public bool IsNamespaceNeed(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
                return false;
            if (Excludes?.IsNamespaceListedExactly(ns) == true)
                return false;
            //
            if (Excludes?.Namespaces != null)
            {
                foreach (var nsPart in Excludes.Namespaces)
                {
                    if (ns.StartsWith(nsPart))
                        return false;
                    if (FilterHelper.IsMatchRegexFilterPattern(ns, nsPart))
                        return false;
                }
            }
            //
            if (Includes?.Namespaces == null || !Includes.Namespaces.Any())
                return true;

            foreach (var nsPart in Includes.Namespaces)
            {
                if (ns.StartsWith(nsPart))
                    return true;
                if (FilterHelper.IsMatchRegexFilterPattern(ns, nsPart))
                        return true;
            }
            return false;
        }

        public bool IsClassNeed(string fullName)
        {
            if (Excludes?.IsClassListed(fullName) == true)
                return false;
            if (Excludes?.Classes != null &&
                FilterHelper.IsMatchRegexFilterPattern(fullName, Excludes.Classes))
                return false;
            //
            if (Includes?.Classes == null || Includes.Classes.Count == 0)
                return !_typeChecker.IsSystemType(fullName);
            if (Includes.IsClassListed(fullName))
                return true;
            return FilterHelper.IsMatchRegexFilterPattern(fullName, Includes.Classes);
        }

        public bool IsAttributeNeed(string name)
        {
            if (Excludes?.IsAttributeListed(name) == true)
                return false;
            if (Excludes?.Attributes != null &&
               FilterHelper.IsMatchRegexFilterPattern(name, Excludes.Attributes))
                return false;
            //
            if (Includes?.Attributes == null || Includes.Attributes.Count == 0)
                return true;
            if (Includes.IsAttributeListed(name))
                return true;
            return FilterHelper.IsMatchRegexFilterPattern(name, Includes.Attributes);
        }
    }
}
