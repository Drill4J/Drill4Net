using System;
using System.Collections.Generic;
using System.Reflection;

namespace Drill4Net.Target.Tests.Common
{
    public class TestInfo
    {
        internal MethodInfo Info { get; set; }
        internal string Signature { get; set; }
        internal bool IgnoreContextForSig { get; set; }
        internal List<string> Checks { get; set; }
        internal bool NeedSort { get; set; }

        /*****************************************************/

        public TestInfo(MethodInfo mi, List<string> checks = null, bool needSort = false)
        {
            Info = mi ?? throw new ArgumentNullException(nameof(mi));
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            NeedSort = needSort;
        }

        public TestInfo(string sig, bool ignoreCtx, List<string> checks = null,bool needSort = false)
        {
            Signature = sig ?? throw new ArgumentNullException(nameof(sig));
            IgnoreContextForSig = ignoreCtx;
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            NeedSort = needSort;
        }
    }
}