using System;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Target.Comon.Tests
{
    public class TestData
    {
        internal MethodInfo Info { get; set; }
        internal List<string> Checks { get; set; }
        internal bool NeedSort { get; set; }

        /*****************************************************/

        public TestData(MethodInfo info, List<string> checks, bool needSort = false)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            NeedSort = needSort;
        }
    }
}