using System;
#if NET48
using System.Runtime.Remoting.Contexts;
#endif

namespace Target.Common
{
#if NET48
    [Synchronization]
#endif
    public class ContextBound : ContextBoundObject
    {
        public int Prop { get; set; }

        /*********************************************/

        public ContextBound(int prop)
        {
            Prop = prop < 0 ? 0 : prop;
        }
    }
}
