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
        private static readonly ConcurrentDictionary<string, string> _funcNames;

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();
            _funcNames = new ConcurrentDictionary<string, string>();
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
                CorrectMethodName(asmName, ref funcName);
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

        internal static void CorrectMethodName(string asmName, ref string curName)
        {
            var stackTrace = new StackTrace(2); //skip local calls
            StackFrame[] stackFrames = stackTrace.GetFrames();

            for (var i = 0; i < stackFrames.Length; i++)
            {
                #region Checks
                StackFrame stackFrame = stackFrames[i];
                var method = stackFrame.GetMethod() as MethodInfo;
                if (method == null)
                    continue;
                if (method.GetCustomAttribute(typeof(DebuggerHiddenAttribute)) != null)
                    continue;
                var mType = method.DeclaringType;
                if ( mType.Name == "ProfilerProxy")
                    continue;
                if (mType.Name.StartsWith("<"))
                    continue;
                //GUANO! By file path is better?
                if (asmName.StartsWith("System.") || asmName.StartsWith("Microsoft."))
                    continue;

                #endregion

                //get simplifying signature of real method
                var typeName = method.DeclaringType.FullName;
                var key = $"{asmName};{typeName}::{method}"; 
                if (_funcNames.TryGetValue(key, out string cachedName)) //cached
                {
                    curName = cachedName;
                    return;
                }

                //get full signature with types of parameters & return
                var name = $"{typeName}::{method.Name}";
                var pars = method.GetParameters();
                var parNames = string.Empty;
                var lastInd = pars.Length - 1;
                for (var j = 0; j <= lastInd; j++)
                {
                    var p = pars[j];
                    parNames += p.ParameterType.FullName;
                    if (j < lastInd)
                        parNames += ",";
                }
                curName = $"{method.ReturnType.FullName} {name}({parNames})";
                _funcNames.TryAdd(key, curName);
                break;
            }
        }

        public override void Process(string data)
        {
            ProcessStatic(data);
        }
    }
}
