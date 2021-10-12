using System.Collections.Generic;

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
                        null
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
                        null
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
                        null
                    }
                };
            }
        }
        public static IEnumerable<object[]> NamespaceData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        NS
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        NS
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        NS
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        new SourceFilterParams(),
                        null
                    }
                };
            }
        }
        public static IEnumerable<object[]> ClassData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        CLASS
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        CLASS
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        CLASS
                    }
                };
            }
        }
        public static IEnumerable<object[]> AttributeData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        null,
                        ATTRIBUTE
                    },
                    new object[]
                    {
                        null,
                        new SourceFilterParams(),
                        ATTRIBUTE
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        null,
                        ATTRIBUTE
                    },
                    new object[]
                    {
                        new SourceFilterParams(),
                        new SourceFilterParams(),
                        null
                    }
                };
            }
        }

    }
}
