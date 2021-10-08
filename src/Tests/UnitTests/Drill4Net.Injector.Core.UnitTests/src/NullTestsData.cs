using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Core.UnitTests
{
    internal class NullTestsData
    {
        const string DIR = @"C:\bin\Debug\Test.File";
        const string FOLDER= "Test";
        const string FILE = "Test.File.cs";
        const string NS = "Drill4Net.Common.Utils";
        const string CLASS = "CommonUtils";
        const string ATTRIBUTE = "CustomAttribute";

        /***************************************************************************/

        public static IEnumerable<object[]> DirectoryData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        DIR
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        DIR
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        DIR
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        new SourceFilterParams(),
                        DIR
                    }
                };
            }
        }
        public static IEnumerable<object[]> FolderData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        FOLDER
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        FOLDER
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        FOLDER
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        new SourceFilterParams(),
                        FOLDER
                    }
                };
            }
        }
        public static IEnumerable<object[]> FileData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        FILE
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        FILE
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        FILE
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        new SourceFilterParams(),
                        FILE
                    }
                };
            }
        }





    }
}
