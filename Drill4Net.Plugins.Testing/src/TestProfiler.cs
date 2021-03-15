using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.Testing
{
    public class TestProfiler : AbsractPlugin
    {
        public static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        public static readonly ConcurrentDictionary<int, string> _lastFuncById;
        private static readonly ConcurrentDictionary<MethodInfo, string> _parentByInfo;
        private static readonly ConcurrentDictionary<string, MethodInfo> _infoBySig; // <string, byte> ?

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            _parentByInfo = new ConcurrentDictionary<MethodInfo, string>();
            _infoBySig = new ConcurrentDictionary<string, MethodInfo>();
            _lastFuncById = new ConcurrentDictionary<int, string>();
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
                if (funcName == null)
                    return;
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

        public static Dictionary<string, List<string>> GetFunctions(bool createNotExistedBranch)
        {
            //This defines the logical execution path of function callers regardless
            //of whether threads are created in async/await or Parallel.For
            var id = Thread.CurrentThread.ExecutionContext.GetHashCode();
            Debug.WriteLine($"Profiler({createNotExistedBranch}): id={id}, trId={Thread.CurrentThread.ManagedThreadId}");

            Dictionary<string, List<string>> byFunctions;
            if (_clientPoints.ContainsKey(id))
            {
                _clientPoints.TryGetValue(id, out byFunctions);
            }
            else
            {
                byFunctions = new Dictionary<string, List<string>>();
                if(createNotExistedBranch)
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
            //TODO: combine both dictionaries?
            //if (_infoBySig.TryGetValue(key, out MethodInfo curInfo))
            //{
            //    _parentByInfo.TryGetValue(curInfo, out curSig);
            //    return;
            //}

            var isDisplay = curSig.Contains("c__DisplayClass");

            //TODO: check performance...
            var stackTrace = new StackTrace(2); //skip local calls
            StackFrame[] stackFrames = stackTrace.GetFrames();

            var processed = false;
            for (var i = 0; i < stackFrames.Length; i++)
            {
                #region Checks
                StackFrame stackFrame = stackFrames[i];
                var method = stackFrame.GetMethod() as MethodInfo;
                if (method == null)
                    continue;
                if (method.GetCustomAttribute(typeof(DebuggerHiddenAttribute)) != null)
                    continue;
                var type = method.DeclaringType;
                if (type == null)
                    continue;
                if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null)
                    continue;
                if (type.Name == "ProfilerProxy") //TODO: from constants/config
                    continue;
                var typeFullName = type.FullName;
                //GUANO! By file path is better? Config?
                if (typeFullName.StartsWith("System.") || typeFullName.StartsWith("Microsoft."))
                    continue;
                var funcFullName = method.ToString();
                if (funcFullName.Contains("(System.Dynamic."))
                {
                    curSig = null;
                    return;
                }

                //all borders have been breached for call stack... but!
                if (isDisplay)
                    break;
                #endregion

                ////SECOND cache
                //if (_parentByInfo.TryGetValue(method, out string fullSig))
                //{
                //    curSig = fullSig;
                //    return;
                //}

                //at this stage we have simplified method's signature
                //get full signature with types of parameters & return
                var name = $"{typeFullName}::{method.Name}";
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
                //need simplify strong named type
                if (retType.Contains("Version=")) 
                    retType = curSig.Split(' ')[0];
                curSig = $"{retType} {name}({parNames})";

                //caching
                _infoBySig.TryAdd(key, method);
                _parentByInfo.TryAdd(method, curSig);
                processed = true;
                break;
            }

            // for async linq/lambda -> business func not in stack (guanito for parallels...)
            // TODO: take into account Id of thread?
            var id = Thread.CurrentThread.ExecutionContext.GetHashCode();
            if (!processed)
            {
                if (_lastFuncById.ContainsKey(id))
                {
                    curSig = _lastFuncById[id];
                }
                else
                {
                    // GUANO !!!!
                    //search pure func name - TODO: regex
                    var curName = curSig.Split("::")[1].Split("(")[0]
                        .Replace("<", null).Replace(">", " ")
                        .Split(" ")[0];
                    
                    string curName2 = null;
                    if (isDisplay)
                    {
                        var ar = curSig.Split("/");
                        curName2 = ar[ar.Length-1];
                        if (curName2.Contains("::"))
                            curName2 = curName2.Split("::")[1];
                        var ind = curName2.StartsWith("<<") ? 2 : 1;
                        curName2 = curName2.Substring(ind, curName2.IndexOf(">") - ind);
                    }
                    foreach (var existStr in _infoBySig.Keys)
                    {
                        var existName = existStr.Split("::")[1].Split("(")[0];
                        if (existName != curName && existName != curName2)
                            continue;
                        curSig = existStr.Split(";")[1];
                        _lastFuncById.TryAdd(id, existStr);
                        processed = true;
                    }
                    if(!processed)
                    {
                    }
                }
            }
            else
            {
                //last func
                if (_lastFuncById.ContainsKey(id))
                    _lastFuncById[id] = curSig;
                else
                    _lastFuncById.TryAdd(id, curSig);
            }
        }
    }
}
