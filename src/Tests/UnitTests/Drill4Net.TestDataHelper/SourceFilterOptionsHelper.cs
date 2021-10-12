using System.Collections.Generic;
using Drill4Net.Injector.Core;

namespace Drill4Net.TestDataHelper
{
    public class SourceFilterOptionsHelper
    {
        /// <summary>
        ///Create SourceFilterOptions with filters.
        /// </summary>
        ///<param name="includes">Include filters</param>
        ///<param name="excludes">Exclude filters</param>
        ////// <returns></returns>
        public SourceFilterOptions CreateSourceFilterOptions(SourceFilterParams includes, SourceFilterParams excludes)
        {
            var options = new SourceFilterOptions
            {
                Includes = includes,
                Excludes = excludes
            };
            return options;
        }

        /// <summary>
        ///Create SourceFilterOptions without filters.
        /// </summary>
        ////// <returns></returns>
        public SourceFilterOptions CreateSourceFilterOptions()
        {
            var options = new SourceFilterOptions
            {
                Includes = new SourceFilterParams(),
                Excludes = new SourceFilterParams()
            };
            return options;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for files.
        /// </summary>
        ///<param name="fileFilter">Filter for files</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeFileFilterOptions(List<string> fileFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Files = fileFilter;
            return flt;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for directories.
        /// </summary>
        ///<param name="directoryFilter">Directory for directories</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeDirectoryFilterOptions(List<string> directoryFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Directories = directoryFilter;
            return flt;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for folders.
        /// </summary>
        ///<param name="folderFilter">Filter for folders</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeFolderFilterOptions(List<string> folderFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Folders = folderFilter;
            return flt;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for classes.
        /// </summary>
        ///<param name="classFilter">Filter for classes</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeClassFilterOptions(List<string> classFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Classes = classFilter;
            return flt;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for namespaces.
        /// </summary>
        ///<param name="namespaceFilter">Filter for namespaces</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeNamespaceFilterOptions(List<string> namespaceFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Namespaces = namespaceFilter;
            return flt;
        }

        /// <summary>
        ///Create SourceFilterOptions with exclude filter for attributes.
        /// </summary>
        ///<param name="attributeFilter">Filter for attributes</param>
        ////// <returns></returns>
        public SourceFilterOptions ExcludeAttributeFilterOptions(List<string> attributeFilter)
        {
            var flt = CreateSourceFilterOptions();
            flt.Excludes.Attributes = attributeFilter;
            return flt;
        }
    }
}
