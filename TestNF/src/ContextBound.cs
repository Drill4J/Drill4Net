using System;
using System.Runtime.Remoting.Contexts;

namespace TestNF
{
    [Synchronization]
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
