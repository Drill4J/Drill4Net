using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    public class CoverageDispatcher
    {
        public StartSessionPayload Session { get; set; }
        public ConcurrentDictionary<string, ExecClassData> PointToClass { get; }
        public ConcurrentDictionary<string, (int, int)> PointToRange { get; }
        public HashSet<ExecClassData> ExecClasses { get; }
        public HashSet<ExecClassData> AffectedExecClasses { get; }
        public int AffectedProbeCount { get; private set; }

        /*************************************************************************/

        public CoverageDispatcher(StartSessionPayload session)
        {
            Session = session; // ?? throw new .....
            PointToClass = new ConcurrentDictionary<string, ExecClassData>();
            PointToRange = new ConcurrentDictionary<string, (int, int)>();
            ExecClasses = new HashSet<ExecClassData>();
            AffectedExecClasses = new HashSet<ExecClassData>();
        }

        /*************************************************************************/

        public void AddPoint(string pointUid, ExecClassData data, int start, int end)
        {
            //link point to range
            if (PointToRange.ContainsKey(pointUid))
                return;
            PointToRange.TryAdd(pointUid, (start, end));

            //link point (probe) to class
            if (PointToClass.ContainsKey(pointUid))
                return;
            PointToClass.TryAdd(pointUid, data);
            
            //list of classes
            if (!ExecClasses.Contains(data))
            {
                lock (ExecClasses)
                {
                    if (!ExecClasses.Contains(data))
                        ExecClasses.Add(data);
                }
            }
        }

        public void RegisterCoverage(string pointUid)
        {
            #region Checks
            //hm... log?
            if (!PointToClass.TryGetValue(pointUid, out var classData))
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
            //
            for(var i = start; i <= end; i++)
                probes[i] = true;
            AffectedProbeCount += end - start + 1;

            //affected classes
            if (!AffectedExecClasses.Contains(classData))
            {
                lock (AffectedExecClasses)
                {
                    if (!AffectedExecClasses.Contains(classData))
                        AffectedExecClasses.Add(classData);
                }
            }
        }

        public void ClearAffectedData()
        {
            AffectedExecClasses.Clear();
            AffectedProbeCount = 0;
        }
    }
}
