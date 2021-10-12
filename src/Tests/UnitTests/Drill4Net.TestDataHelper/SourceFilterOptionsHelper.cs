using System.Collections.Generic;
using Drill4Net.Injector.Core;

namespace Drill4Net.TestDataHelper
{
    public class SourceFilterOptionsHelper
    {
        public SourceFilterOptions CreateSourceFilterOptions(SourceFilterParams includes, SourceFilterParams excludes)
        {
            var options = new SourceFilterOptions
            {
                Includes = includes,
                Excludes = excludes
            };
            return options;
        }
        public SourceFilterOptions CreateSourceFilterOptions()
        {
            var options = new SourceFilterOptions
            {
                Includes = new SourceFilterParams(),
                Excludes = new SourceFilterParams()
            };
            return options;
        }

        public SourceFilterOptions ExcludeFileFilterOptions(List<string> fileFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Files = fileFilter;
            return flt;
        }

        public SourceFilterOptions ExcludeDirectoryFilterOptions(List<string> directoryFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Directories = directoryFilter;
            return flt;
        }

        public SourceFilterOptions ExcludeFolderFilterOptions(List<string> folderFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Folders = folderFilter;
            return flt;
        }
        public SourceFilterOptions ExcludeClassFilterOptions(List<string> classFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Classes = classFilter;
            return flt;
        }
        public SourceFilterOptions ExcludeNamespaceFilterOptions(List<string> namespaceFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Namespaces = namespaceFilter;
            return flt;
        }
        public SourceFilterOptions ExcludeAttributeFilterOptions(List<string> attributeFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Attributes = attributeFilter;
            return flt;
        }
    }
}
