using System;
using System.Collections.Concurrent;
using System.Linq;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    public class CoverageDispatcher
    {
        public ConcurrentDictionary<string, ExecClassData> PointToClass { get; }
        public ConcurrentDictionary<string, (int, int)> PointToRange { get; }

        /*************************************************************************/

        public CoverageDispatcher()
        {
            PointToClass = new ConcurrentDictionary<string, ExecClassData>();
            PointToRange = new ConcurrentDictionary<string, (int, int)>();
        }

        /*************************************************************************/

        public void AddPoint(string pointUid, ExecClassData data, int start, int end)
        {
            //PointToRange
            if (PointToRange.ContainsKey(pointUid))
                return;
            PointToRange.TryAdd(pointUid, (start, end));

            // PointToClass
            if (PointToClass.ContainsKey(pointUid))
                return;
            PointToClass.TryAdd(pointUid, data);
        }

        public void RegisterCoverage(string pointUid)
        {
            #region Checks
            //hm... log?
            if (!PointToClass.TryGetValue(pointUid, out ExecClassData classData))
                return;
            if (!PointToRange.TryGetValue(pointUid, out (int Start, int End) range))
                return;
            var probes = classData.probes;
            var start = range.Start;
            var end = range.End;
            if (start > end || start < 0 || start >= probes.Count || end < 0 || end >= probes.Count)
                return;
            #endregion

            if (probes[start]) //yet registered
                return;
            for(var i = start; i <= end; i++)
                probes[i] = true;
        }
    }
}
