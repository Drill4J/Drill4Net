using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    public struct InjectionAnswer
    {
        public Guid RequestUid { get; set; }
        public List<CrossPoint> Points { get; set; }

        /*********************************************************/

        public InjectionAnswer(Guid requestUid)
        {
            RequestUid = requestUid;
            Points = new List<CrossPoint>();
        }
    }
}
