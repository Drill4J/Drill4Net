using Drill4Net.Injector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class DirectoryTestData
    {
        private SourceFilterOptions CreateSourceFilterOptions(List<string> directoriesInclude, List<string> directoriesExclude, List<string> foldersInclude, List<string> foldersExclude)
        {

            var opt= new SourceFilterOptions
            {
                Includes = new SourceFilterParams(),
                Excludes = new SourceFilterParams()
            };
            opt.Includes.Directories
        }
        public static IEnumerable<object[]> NeedProcessDirectoryTrue
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                    },
                    new object[]
                    {
                    }
                };
            }
        }
    }
}
