using System;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;
using C = Drill4Net.Injector.Core.InjectorCoreConstants;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractProbeHelper
    {
        public virtual string PrepareProbeData(InjectedMethod injMeth, CrossPointType pointType, 
            Dictionary<string, object> data = null)
        {
            //TODO: extensions
            var index = data?.ContainsKey(C.PROBE_KEY_INDEX) == true ? (int)data[C.PROBE_KEY_INDEX] : -1;
            return PrepareProbeData(injMeth, pointType, index, data);
        }
        
        public virtual string PrepareProbeData(InjectedMethod injMeth, CrossPointType pointType, int index, 
            Dictionary<string, object> data = null)
        {
            var id = index == -1 ? null : index.ToString();
            var pointUid = Guid.NewGuid().ToString();

            var point = new CrossPoint(pointUid, id, pointType)
            {
                //TODO: PDB data
            };
            injMeth.AddChild(point);

            return GenerateProbeData(injMeth, point, pointUid, data);
        }

        protected abstract string GenerateProbeData(InjectedMethod injMeth, CrossPoint point, string pointUid,
            Dictionary<string, object> data);
    }
}
