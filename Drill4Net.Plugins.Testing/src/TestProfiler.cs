using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.Testing
{
    public class TestProfiler : AbsractPlugin
    {
        public static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        private static readonly ConcurrentDictionary<MethodInfo, string> _sigByInfo;
        private static readonly ConcurrentDictionary<string, MethodInfo> _infoBySig; // <string, byte> ?

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            _sigByInfo = new ConcurrentDictionary<MethodInfo, string>();
            _infoBySig = new ConcurrentDictionary<string, MethodInfo>();
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                    throw new ArgumentNullException(nameof(data));
                var ar = data.Split('^');
                if (ar.Length < 4)
                    throw new ArgumentException($"Bad format of input: {data}");
                //
                //var id = ar[0]; //not using yet
                var asmName = ar[1];
                var funcName = ar[2];
                ClarifyBusinessMethodName(asmName, ref funcName);
                AddPoint(asmName, funcName, ar[3]);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public override void Register(string data)
        {
            RegisterStatic(data);
        }

        internal static void AddPoint(string asmName, string funcSig, string point)
        {
            var points = GetPoints(asmName, funcSig);
            points.Add(point);
        }

        public static List<string> GetPoints(string asmName, string funcSig, bool withPointRemoving = false)
        {
            Dictionary<string, List<string>> byFunctions = GetFunctions(!withPointRemoving);
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
            if (withPointRemoving)
                byFunctions.Remove(funcPath);
            return points;
        }

        public static Dictionary<string, List<string>> GetFunctions(bool createNotExistsedFuncBranch)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var id = Thread.CurrentThread.ExecutionContext.GetHashCode();
            Debug.WriteLine($"Profiler: id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(id))
            {
                _clientPoints.TryGetValue(id, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if(createNotExistsedFuncBranch)
                    _clientPoints.TryAdd(id, byFunctions);
            }
            return byFunctions;
        }

        internal static void ClarifyBusinessMethodName(string asmName, ref string curSig)
        {
            //curSig may be a internal compiler's function (for generics, Enumerator,
            //AsyncStateMachine) and we must find parent business function from the call stack

            //FIRST cache
            var key = $"{asmName};{curSig}";
            if (_infoBySig.TryGetValue(key, out MethodInfo curInfo))
                return;

            //TODO: check performance...
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
                if ( mType.Name == "ProfilerProxy") //TODO: from constants/config
                    continue;
                if (mType.Name.StartsWith("<"))
                    continue;
                //GUANO! By file path is better? Config?
                if (asmName.StartsWith("System.") || asmName.StartsWith("Microsoft."))
                    continue;

                #endregion

                //SECOND cache
                if (_sigByInfo.TryGetValue(method, out string fullSig))
                {
                    curSig = fullSig;
                    return;
                }

                //at this stage we have simplified method's signature
                //get full signature with types of parameters & return
                var typeName = method.DeclaringType.FullName;
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

                //hm... TODO: get rid of the return type?
                var retType = method.ReturnType.FullName;
                if(retType.Contains("Version="))
                    retType = curSig.Split(' ')[0];
                curSig = $"{retType} {name}({parNames})";

                _infoBySig.TryAdd(key, method);
                _sigByInfo.TryAdd(method, curSig);
                break;
            }
        }
    }
}
