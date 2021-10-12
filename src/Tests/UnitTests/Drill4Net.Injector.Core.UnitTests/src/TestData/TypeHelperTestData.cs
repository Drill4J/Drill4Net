using Drill4Net.TestDataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Injector.Core.UnitTests
{
    public class TypeHelperTestData
    {
        const string NS = "Drill4Net.Common";
        const string CLASS_NAME = NS+".CommonUtils";
        const string ATTRIBUTE = "CustomAttribute";

        /*****************************************************************************************/

        private static SourceFilterOptionsHelper _helper = new SourceFilterOptionsHelper();
        private static List<string> _classFilter = new List<string>
        {
            CLASS_NAME
        };
        private static List<string> _nsFilter = new List<string>
        {
            NS
        };
        private static List<string> _attributes = new List<string>
        {
            "CustomAttribute1",
            ATTRIBUTE
        };

        /*****************************************************************************************/

        public static IEnumerable<object[]> TypeHelperFalseData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        "<Module>",
                        new List<string>{ }
                    },
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        "System.Linq.List",
                        new List<string>{ }
                    },
                    new object[]
                    {
                        _helper.ExcludeClassFilterOptions(_classFilter),
                        CLASS_NAME,
                        new List<string>{ }
                    },
                    new object[]
                    {
                        _helper.ExcludeNamespaceFilterOptions(_nsFilter),
                        CLASS_NAME,
                        new List<string>{ }
                    },
                    new object[]
                    {
                        _helper.ExcludeAttributeFilterOptions(_attributes),
                        CLASS_NAME,
                        new List<string>
                        {
                            ATTRIBUTE
                        }
                    }
                };
            }
        }
        public static IEnumerable<object[]> TypeHelperTrueData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        CLASS_NAME,
                        new List<string>{ }
                    },
                    new object[]
                    {
                        _helper.CreateSourceFilterOptions(),
                        CLASS_NAME,
                        new List<string>
                        {
                            "Attribute1",
                            "Attribute2"
                        }
                    }
                };
            }
        }
    }
}
