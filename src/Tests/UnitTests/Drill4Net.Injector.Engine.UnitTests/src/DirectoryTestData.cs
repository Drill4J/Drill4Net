using Drill4Net.Injector.Core;
using Drill4Net.TestDataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class DirectoryTestData
    {
        const string DIR= @"C:\Sources\App\Drill4Net.Target.Net461.App\";
        const string FOLDER = "Drill4Net.Target.Net461.App";
        const string NS = "Drill4Net.Target.Net461.App";
        const string FILE_PATH = @"C:\Sources\App\Drill4Net.Target.Net461.App\";



        private static SourceFilterOptionsHelper _helper = new SourceFilterOptionsHelper();
        private static List<string> _directoryFilter = new List<string>
        {
            DIR
        };
        private static List<string> _folderFilter = new List<string>
        {
            FOLDER
        };
        private static List<string> _nsFilter = new List<string>
        {
            NS
        };
        public static IEnumerable<object[]> NeedProcessDirectoryTrue
        {
            get
            {
                return new List<object[]>()
                {
                    //new object[]
                    //{
                    //    _helper.CreateSourceFilterOptions(null,null,)
                    //},
                    new object[]
                    {
                        null,
                        DIR,
                        false
                    },
                   new object[]
                    {
                        _helper.CreateSourceFilterOptions(null,null, null, null,null, null, null,null, 
                        null, null, null, null, null, null),
                        DIR,
                        true
                    },
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(new SourceFilterParams(),null, _directoryFilter, null,null,
                        null, null,null, null, null, null, null, null, null),
                        DIR,
                        false
                    }
                };
            }
        }
    }
}
