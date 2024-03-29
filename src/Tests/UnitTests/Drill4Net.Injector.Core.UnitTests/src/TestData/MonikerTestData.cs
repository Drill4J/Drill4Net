﻿using System.Collections.Generic;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core.UnitTests
{
    public class MonikerTestData 
    {
        const string NET61 = "net461";
        const string NET48 = "net48";
        const string NET50 = "net5.0";
        const string NETCORE22 = "netcoreapp2.2";
        const string NETCORE31 = "netcoreapp3.1";
        const string BASE_FOLDER_NET61 = @"Drill4Net.Target.Net461.App\";
        const string BASE_FOLDER_NET48 = "Drill4Net.Target.Net48.App";
        const string BASE_FOLDER_NET50 = @"Drill4Net.Target.Net50.App\net5.0";
        const string BASE_FOLDER_CORE22 = @"Drill4Net.Target.Core22.App\netcoreapp2.2";
        const string BASE_FOLDER_CORE31 = @"Drill4Net.Target.Core31.App\netcoreapp3.1";
        const string ROOT = @"C:\Sources\App\";
        const string DIR = @"C:\Sources\App\Drill4Net.Target.Core22.App";
        const string DIR2 = @"C:\Sources\App\Drill4Net.Target.Net461.App";

        /*****************************************************************************************/

        private static MonikerData CreateMonirerData(string baseFolder)
        {
            var monikerData = new MonikerData();
            monikerData.BaseFolder = baseFolder;
            return monikerData;
        }

        /*****************************************************************************************/

        public static IEnumerable<object[]> MonikerTrueData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        null,
                        ROOT,
                        DIR
                    },
                    new object[]
                    {
                        new Dictionary<string, MonikerData>(),
                        ROOT,
                        DIR
                    },
                    new object[]
                {
                    new Dictionary<string, MonikerData>
                    {
                        { NET61, CreateMonirerData(BASE_FOLDER_NET61) },
                        { NET48, CreateMonirerData(BASE_FOLDER_NET48) },
                        { NETCORE22, CreateMonirerData(BASE_FOLDER_CORE22) },
                        { NET50, CreateMonirerData(BASE_FOLDER_NET50) },
                        { NETCORE31, CreateMonirerData(BASE_FOLDER_CORE31) }
                    },
                    ROOT,
                    DIR
                },
                    new object[]
                {
                    new Dictionary<string, MonikerData>
                    {
                        { NET61, CreateMonirerData(BASE_FOLDER_NET61) },
                        { NET48, CreateMonirerData(BASE_FOLDER_NET48) },
                        { NETCORE22, CreateMonirerData(BASE_FOLDER_CORE22) },
                        { NET50, CreateMonirerData(BASE_FOLDER_NET50) },
                    },
                    ROOT,
                    DIR2
                }

                };
            }
        }
        public static IEnumerable<object[]> MonikerFalseData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                {
                    new Dictionary<string, MonikerData>
                    {
                        { NET61, CreateMonirerData(BASE_FOLDER_NET61) },
                        { NET48, CreateMonirerData(BASE_FOLDER_NET48) },
                        { NET50, CreateMonirerData(BASE_FOLDER_NET50) },
                    },
                    ROOT,
                    DIR
                }
                };
            }
        }
        public static IEnumerable<object[]> MonikerNullData
        {
            get
            {
                return new List<object[]>()
                {
                new object[]
                {
                    null,
                    ROOT,
                    DIR
                }
                };
            }
        }
    }
}
