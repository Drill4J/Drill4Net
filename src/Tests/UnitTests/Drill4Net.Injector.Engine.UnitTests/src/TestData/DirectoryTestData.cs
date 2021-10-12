using System;
using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.TestDataHelper;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class DirectoryTestData
    {
        const string DIR = @"C:\Sources\App\Drill4Net.Target.Net461.App\";
        const string FOLDER = "Drill4Net.Target.Net461.App";
        const string NS = "Drill4Net.Target.Net461.App";
        const string FILE_PATH = @"C:\Sources\App\Drill4Net.Target.Net461.App\";

        /*****************************************************************************************/

        private static SourceFilterOptionsHelper _helper = new SourceFilterOptionsHelper();
        private static List<string> _directoryFilter = new List<string>
        {
            DIR
        };
        private static List<string> _folderFilter = new List<string>
        {
            FOLDER
        };

        /*****************************************************************************************/

        public static IEnumerable<object[]> ProcessDirectoryTrueData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        DIR,
                        false
                    },
                   new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        DIR,
                        true
                    },
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        DIR,
                        false
                    }
                };
            }
        }
        public static IEnumerable<object[]> ProcessDirectoryFalseData
        {
            get
            {
                return new List<object[]>()
                {
                   new object[]
                    {
                        _helper.ExcludeDirectoryFilterOptions(_directoryFilter),
                        DIR,
                        false
                    },
                    new object[]
                    {
                        _helper.ExcludeFolderFilterOptions(_folderFilter),
                        DIR,
                        false
                    }
                };
            }
        }
        public static IEnumerable<object[]> ProcessDirectoryNullData
        {
            get
            {
                return new List<object[]>()
                {
                   new object[]
                    {
                        null,
                        DIR,
                        false
                    }
                };
            }
        }
    }
}
