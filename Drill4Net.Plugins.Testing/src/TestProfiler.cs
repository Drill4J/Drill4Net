using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.Testing
{
    public class TestProfiler : AbsractPlugin
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, List<string>>> _clientPoints;

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
        }

        /*****************************************************************************/

        public static void ProcessStatic(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                    throw new ArgumentNullException(nameof(data));
                var ar = data.Split('^');
                if (ar.Length < 4)
                    throw new ArgumentException($"Bad format of input: {data}");
                //
                var id = ar[0];
                var source = ar[1];
                var points = GetPoints(id, source);
                points.Add(ar[3]);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public static List<string> GetPoints(string id, string funcPath, bool withRemoving = false)
        {
            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(id))
            {
                _clientPoints.TryGetValue(id, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                _clientPoints.TryAdd(id, byFunctions);
            }
            //
            List<string> points;
            if (byFunctions.ContainsKey(funcPath))
            {
                points = byFunctions[funcPath];
            }
            else
            {
                points = new List<string>();
                byFunctions.Add(funcPath, points);
            }
            //
            if (withRemoving)
                byFunctions.Remove(funcPath);
            return points;
        }

        public override void Process(string data)
        {
            ProcessStatic(data);
        }
    }
}
