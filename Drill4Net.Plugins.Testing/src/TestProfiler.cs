using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
                var asmName = ar[1];
                var funcName = ar[2];
                CorrectMethodName(ref funcName);
                var points = GetPoints(id, asmName, funcName);
                var point = ar[3];
                points.Add(point);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public static List<string> GetPoints(string id, string asmName, string funcSig, bool withRemoving = false)
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
            var funcPath = $"{asmName};{funcSig}";
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

        internal static void CorrectMethodName(ref string curName)
        {
            var stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();

            for (int i = 2; i < stackFrames.Length; i++)
            {
                StackFrame stackFrame = stackFrames[i];
                var method = stackFrame.GetMethod() as MethodInfo;
                if (method == null)
                    continue;
                var mType = method.DeclaringType;
                if (mType.Name.StartsWith("<") || mType.Name == "ProfilerProxy")
                    continue;
                var asmName = mType.Assembly.GetName().Name;
                //GUANO! By file path is better?
                if (asmName.StartsWith("System.") || asmName.StartsWith("Microsoft."))
                    continue;

                var typeName = method.DeclaringType.FullName;
                var name = $"{typeName}::{method.Name}";
                var pars = method.GetParameters();
                var parNames = string.Empty;
                var lastInd = pars.Length - 1;
                for (int j = 0; j <= lastInd; j++)
                {
                    var p = pars[j];
                    parNames += p.ParameterType.FullName;
                    if (j < lastInd)
                        parNames += ",";
                }
                curName = $"{method.ReturnType.FullName} {name}({parNames})";
                break;
                //Console.WriteLine($"{typeName}.{method.Name} -> {stackFrame.GetILOffset()}");
            }
        }

        public override void Process(string data)
        {
            ProcessStatic(data);
        }
    }
}
