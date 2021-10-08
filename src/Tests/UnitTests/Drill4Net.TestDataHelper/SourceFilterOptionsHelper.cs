using Drill4Net.Injector.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.TestDataHelper
{
    public class SourceFilterOptionsHelper
    {
        public SourceFilterOptions CreateSourceFilterOptions(SourceFilterParams includes, SourceFilterParams excludes, List<string> directoriesInclude,
    List<string> foldersInclude, List<string> filesInclude, List<string> namespacesInclude, List<string> classesInclude, List<string> attributesInclude,
    List<string> directoriesExclude, List<string> foldersExclude, List<string> filesExclude, List<string> namespacesExclude, List<string> classesExclude,
    List<string> attributesExclude)
        {
            var options = new SourceFilterOptions
            {
                Includes = includes,
                Excludes = excludes
            };
            if (options.Includes != null)
            {
                options.Includes.Directories = directoriesInclude?.ToList();
                options.Includes.Folders = foldersInclude?.ToList();
                options.Includes.Files = filesInclude?.ToList();
                options.Includes.Namespaces = namespacesInclude?.ToList();
                options.Includes.Classes = classesInclude?.ToList();
                options.Includes.Attributes = attributesInclude?.ToList();
            }
            if (options.Excludes != null)
            {
                options.Excludes.Directories = directoriesExclude?.ToList();
                options.Excludes.Folders = foldersExclude?.ToList();
                options.Excludes.Files = filesExclude?.ToList();
                options.Excludes.Namespaces = namespacesExclude?.ToList();
                options.Excludes.Classes = classesExclude?.ToList();
                options.Excludes.Attributes = attributesExclude?.ToList();
            }
            return options;
        }
    }
}
