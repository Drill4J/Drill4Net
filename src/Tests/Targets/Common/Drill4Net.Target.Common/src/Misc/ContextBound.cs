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

        public ContextBound(int prop)
        {
            Prop = prop < 0 ? 0 : prop;
        }
    }
}
#endif
