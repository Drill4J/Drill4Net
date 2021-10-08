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
        const string DIR= @"C:\Sources\App\Drill4Net.Target.Net461.App\";
        const string FOLDER = "Drill4Net.Target.Net461.App";
        const string NS = "Drill4Net.Target.Net461.App";
        const string FILE_PATH = @"C:\Sources\App\Drill4Net.Target.Net461.App\";



        private static OptionHelper _helper=new OptionHelper();
        private static List<string> _directoryFilter = new List<string>
        {
            ""
        };
        private static List<string> _folderFilter = new List<string>
        {
            ""
        };
        private static List<string> _nsFilter = new List<string>
        {
            ""
        };
        public static IEnumerable<object[]> NeedProcessDirectoryTrue
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(null,null,)
                    },
                    new object[]
                    {
                    }
                };
            }
        }
    }
}
