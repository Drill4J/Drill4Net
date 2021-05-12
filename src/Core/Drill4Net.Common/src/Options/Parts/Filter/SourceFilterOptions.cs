using System.Linq;

namespace Drill4Net.Common
{
    public class SourceFilterOptions
    {
        public SourceFilterParams Includes { get; set; } 
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
            if (Excludes?.IsNamespaceListedExactly(ns) == true)
                return false;
            if (Includes?.Namespaces == null || !Includes.Namespaces.Any())
                return true;
            foreach (var nsPart in Includes.Namespaces)
                if (ns.StartsWith(nsPart))
                    return true;
            return false;
        }

        public bool IsClassNeed(string fullName)
        {
            if (Excludes?.IsClassListed(fullName) == true)
                return false;
            if (Includes?.Classes == null || !Includes.Classes.Any())
                return true;
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
