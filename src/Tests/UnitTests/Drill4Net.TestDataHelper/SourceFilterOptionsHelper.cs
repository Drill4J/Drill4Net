using Drill4Net.Injector.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
