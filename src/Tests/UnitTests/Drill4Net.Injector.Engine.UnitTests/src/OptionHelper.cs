using Drill4Net.Injector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Engine.UnitTests
{
    internal  class OptionHelper
    {
        internal SourceFilterOptions CreateSourceFilterOptions(List<string> directoriesInclude, List<string> directoriesExclude, 
            List<string> foldersInclude, List<string> foldersExclude, List<string> NamespacesInclude, List<string> NamespacesExclude)
        {

            var opt = new SourceFilterOptions
            {
                Includes = new SourceFilterParams(),
                Excludes = new SourceFilterParams()
            };
            opt.Includes.Directories = directoriesInclude;
            opt.Excludes.Directories = directoriesExclude;
            opt.Includes.Folders = foldersInclude;
            opt.Excludes.Folders = foldersExclude;
            opt.Includes.Namespaces = NamespacesInclude;
            opt.Excludes.Namespaces = NamespacesExclude;
            return opt;
        }
    }
}
