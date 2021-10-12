using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.TestDataHelper;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class FileTestData
    {
        const string FILE_PATH = @"C:\Sources\App\Drill4Net.Target.Net461.App\Class.cs";
        const string FILE_NAME = "Class.cs";

        /*****************************************************************************************/

        private static SourceFilterOptionsHelper _helper = new SourceFilterOptionsHelper();
        private static List<string> _fileFilter = new List<string>
        {
            FILE_NAME
        };

        /*****************************************************************************************/

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
                        _helper.ExcludeFileFilterOptions(_fileFilter),
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

