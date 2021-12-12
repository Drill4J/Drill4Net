using System;
using System.Collections.Generic;
using System.Reflection;

namespace Drill4Net.Target.Tests.Engine
{
    public class TestMetadata
    {
        internal MethodInfo Info { get; set; }
        internal string Signature { get; set; }
        internal bool IgnoreContextForSig { get; set; }
        internal List<string> Checks { get; set; }
        internal bool NeedSort { get; set; }

        /**********************************************************************************/

        public TestMetadata(MethodInfo mi, List<string> checks = null, bool needSort = false)
        {
            Info = mi ?? throw new ArgumentNullException(nameof(mi));
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            NeedSort = needSort;
        }

        public TestMetadata(string sig, bool ignoreCtx, List<string> checks = null,bool needSort = false)
        {
            Signature = sig ?? throw new ArgumentNullException(nameof(sig));
            IgnoreContextForSig = ignoreCtx;
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            NeedSort = needSort;
        }

        /**********************************************************************************/

        public override string ToString()
        {
            return Info?.Name ?? Signature;
        }
    }
}