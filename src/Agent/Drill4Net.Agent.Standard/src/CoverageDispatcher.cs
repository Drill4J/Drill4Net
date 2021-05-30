using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Manager of the coverage data for the user's or global session
    /// </summary>
    public class CoverageDispatcher
    {
        public StartSessionPayload Session { get; set; }
        public ConcurrentDictionary<string, ExecClassData> PointToClass { get; }
        public ConcurrentDictionary<string, (int, int)> PointToRange { get; }
        public HashSet<ExecClassData> ExecClasses { get; }
        public HashSet<ExecClassData> AffectedExecClasses { get; }
        public int AffectedProbeCount { get; private set; }

        /*************************************************************************/

        /// <summary>
        /// Create manager of the coverage data for the user's or global session
        /// </summary>
        /// <param name="session"></param>
        public CoverageDispatcher(StartSessionPayload session)
        {
            Session = session; // ?? throw new .....
            PointToClass = new ConcurrentDictionary<string, ExecClassData>();
            PointToRange = new ConcurrentDictionary<string, (int, int)>();
            ExecClasses = new HashSet<ExecClassData>();
            AffectedExecClasses = new HashSet<ExecClassData>();
        }

        /*************************************************************************/

        /// <summary>
        /// Bind point Uid to probe's range of the target class
        /// </summary>
        /// <param name="pointUid"></param>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void BindPoint(string pointUid, ExecClassData data, int start, int end)
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

        /// <summary>
        /// Register the coverage data by point Uid coming from Target side
        /// </summary>
        /// <param name="pointUid"></param>
        public bool RegisterCoverage(string pointUid)
        {
            #region Checks
            //hm... log?
            if (!PointToRange.TryGetValue(pointUid, out (int Start, int End) range))
                return false; //it's error
            if (!PointToClass.TryGetValue(pointUid, out var classData))
                return false; //it's normal, but not the best (for block coverage we not need "Enter" type of cross-points, another case is a possible error)

            var probes = classData.probes;
            var start = range.Start;
            var end = range.End;
            if (start > end || start < 0 || start >= probes.Count || end < 0 || end >= probes.Count)
                return false; //it's error
            #endregion

            if (probes[start]) //already registered
                return true;
            //
            lock (AffectedExecClasses)
            {
                for (var i = start; i <= end; i++)
                    probes[i] = true;
                AffectedProbeCount += end - start + 1;
                //
                if (!AffectedExecClasses.Contains(classData))
                    AffectedExecClasses.Add(classData);
            }
            return true;
        }

        /// <summary>
        /// Clearing the affected data (classes, probes, etc) after sending it to admin side
        /// </summary>
        public void ClearAffectedData()
        {
            AffectedExecClasses.Clear();
            AffectedProbeCount = 0;
        }
    }
}
