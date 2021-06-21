using System.Collections.Concurrent;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Registrar of the coverage data for the user's and global session
    /// </summary>
    public class CoverageRegistrator
    {
        /// <summary>
        /// Gets or sets the test session on the Drill Admin side.
        /// </summary>
        /// <value>
        /// The test session.
        /// </value>
        public StartSessionPayload Session { get; set; }

        /// <summary>
        /// Maps the cross-point's Uid to the corresponding target type.
        /// </summary>
        /// <value>
        /// The DTO of the Target type.
        /// </value>
        public ConcurrentDictionary<string, ExecClassData> PointToType { get; }

        /// <summary>
        /// Gets the range of the indexes in method's instructions for the specified cross-point's Uid.
        /// </summary>
        /// <value>
        /// The range of the indexes in method's instructions
        /// </value>
        public ConcurrentDictionary<string, (int, int)> PointToRange { get; }

        /// <summary>
        /// Gets the injected classes in DTO form.
        /// </summary>
        /// <value>
        /// The injected classes in DTO form.
        /// </value>
        public HashSet<ExecClassData> Types { get; }

        /// <summary>
        /// Gets the affected injected classes after last sending its data to the Drill admin side.
        /// </summary>
        /// <value>
        /// The affected injected classes.
        /// </value>
        public HashSet<ExecClassData> AffectedTypes { get; }

        /// <summary>
        /// Gets the affected probe count after last sending data to the Drill admin side.
        /// </summary>
        /// <value>
        /// The affected probe count.
        /// </value>
        public int AffectedProbeCount { get; private set; }

        /*************************************************************************/

        /// <summary>
        /// Create manager of the coverage data for the user's or global session
        /// </summary>
        /// <param name="session"></param>
        public CoverageRegistrator(StartSessionPayload session)
        {
            Session = session; // ?? throw new .....
            PointToType = new ConcurrentDictionary<string, ExecClassData>();
            PointToRange = new ConcurrentDictionary<string, (int, int)>();
            Types = new HashSet<ExecClassData>();
            AffectedTypes = new HashSet<ExecClassData>();
        }

        /*************************************************************************/

        /// <summary>
        /// Bind point Uid to the probe's range of the target class
        /// </summary>
        /// <param name="pointUid">Cross-point's Uid</param>
        /// <param name="typeData">Probes of instrumented assemblies for sending of collecting data to the Admin side</param>
        /// <param name="start">Start index of the cross-point's region in IL instructions of the its method</param>
        /// <param name="end">End index of the cross-point's region in IL instructions of the its method</param>
        public void BindPoint(string pointUid, ExecClassData typeData, int start, int end)
        {
            //link point to range
            if (PointToRange.ContainsKey(pointUid))
                return;
            PointToRange.TryAdd(pointUid, (start, end));

            //link point (probe) to class
            if (PointToType.ContainsKey(pointUid))
                return;
            PointToType.TryAdd(pointUid, typeData);
            
            //list of classes
            if (!Types.Contains(typeData))
            {
                lock (Types)
                {
                    if (!Types.Contains(typeData))
                        Types.Add(typeData);
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
            if (!PointToType.TryGetValue(pointUid, out var classData))
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
            lock (AffectedTypes)
            {
                for (var i = start; i <= end; i++)
                    probes[i] = true;
                AffectedProbeCount += end - start + 1;
                //
                if (!AffectedTypes.Contains(classData))
                    AffectedTypes.Add(classData);
            }
            return true;
        }

        /// <summary>
        /// Clearing the affected data (classes, probes, etc) after sending it to admin side
        /// </summary>
        public void ClearAffectedData()
        {
            AffectedTypes.Clear();
            AffectedProbeCount = 0;
        }
    }
}
