using System;
#if NET45
using System.Runtime.Remoting.Contexts;
#endif
#if NET48
using System.Runtime.Remoting.Contexts;
#endif

namespace Drill4Net.Target.Common
{
#if NET45
    [Synchronization]
#endif
#if NET48
    [Synchronization]
#endif
    public class ContextBound : ContextBoundObject
    {
        public int Prop { get; set; }
        
        /**********************************************/

        public ContextBound(int prop)
        {
            Prop = prop < 0 ? 0 : prop;
        }
    }
}
