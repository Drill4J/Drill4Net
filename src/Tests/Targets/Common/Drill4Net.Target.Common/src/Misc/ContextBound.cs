#if NETFRAMEWORK
using System;
using System.Runtime.Remoting.Contexts;


namespace Drill4Net.Target.Common
{
    [Synchronization]
    public class ContextBound : ContextBoundObject
    {
        public int Prop { get; set; }
        
        /**********************************************/

        public ContextBound(bool cond)
        {
            Prop = cond ? 1 : -1;
        }
    }
}
#endif
