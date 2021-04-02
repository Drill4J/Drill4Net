﻿namespace Drill4Net.Target.NetCore
{
    public static class TestConstants
    {
        #region Monikers
        public const string MONIKER_NET50 = "net5.0";
        public const string MONIKER_CORE31 = "netcoreapp3.1";
        public const string MONIKER_CORE22 = "netcoreapp2.2";
        public const string MONIKER_NET48 = "net48";
        public const string MONIKER_NET45 = "net45";
        #endregion

        public const string CLASS_DEFAULT_SHORT = "InjectTarget";
        public const string CLASS_DEFAULT_FULL = "Drill4Net.Target.Common.InjectTarget";

        public const string ASSEMBLY_COMMON = "Drill4Net.Target.Common.dll";
        public const string ASSEMBLY_NET31 = "Drill4Net.Target.Core31.dll";
        public const string ASSEMBLY_NET5 = "Drill4Net.Target.Net50.dll";

        public const string INFLUENCE = "The passing of the test is affected by some other asynchronous tests. May be test will pass in Debug Test Mode.";
    }
}
