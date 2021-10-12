using Drill4Net.Injector.Core;
using Drill4Net.TestDataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class FileTestData
    {
        const string FILE_PATH = @"C:\Sources\App\Drill4Net.Target.Net461.App\Class.cs";
        const string FILE_NAME = "Class.cs";

        private static SourceFilterOptionsHelper _helper = new SourceFilterOptionsHelper();
        private static List<string> _fileFilter = new List<string>
        {
            FILE_NAME
        };

        private static SourceFilterOptions ExcludeFileFilterOptions(List<string> fileFilter)
        {
            var flt = _helper.CreateSourceFilterOptions();
            flt.Excludes.Files =fileFilter;
            return flt;
        }

        public static IEnumerable<object[]> ProcessFileTrueData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        FILE_PATH
                    },
                   new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        FILE_PATH
                    },
                };
            }
        }

        public static IEnumerable<object[]> ProcessFileFalseData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        ExcludeFileFilterOptions(_fileFilter),
                        FILE_PATH
                    }
                };
            }
        }
        public static IEnumerable<object[]> ProcessFileNullData
        {
            get
            {
                return new List<object[]>()
                {
                   new object[]
                    {
                        null,
                        FILE_PATH
                    }
                };
            }
        }
    }
}

