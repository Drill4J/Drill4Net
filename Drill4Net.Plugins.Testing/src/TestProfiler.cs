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
        public static readonly ConcurrentDictionary<int, string> _lastFuncByCtx;
        private static readonly ConcurrentDictionary<MethodInfo, string> _parentByInfo;
        private static readonly ConcurrentDictionary<string, MethodInfo> _infoBySig; // <string, byte> ?

        private const string DISPLAY_CLASS = "c__DisplayClass";

        /*****************************************************************************/

        static TestProfiler()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            _parentByInfo = new ConcurrentDictionary<MethodInfo, string>();
            _infoBySig = new ConcurrentDictionary<string, MethodInfo>();
            _lastFuncByCtx = new ConcurrentDictionary<int, string>();
        }

        /*****************************************************************************/

        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log("Data is empty");
                    return;
                }
                //
                var ar = data.Split('^');
                if (ar.Length < 4)
                {
                    Log($"Bad format of input: {data}");
                    return;
                }
                #endregion

                var realmethodName = ar[0];
                var asmName = ar[1];
                var funcName = ar[2];
                var mess = ar[3];
                if (ClarifyBusinessMethodName(asmName, realmethodName, ref funcName))
                    AddPoint(asmName, funcName, mess);
            }
            catch (Exception ex)
            {
                Log($"{data}\n{ex}");
            }
        }

        internal static void Log(string mess)
        {
            //...
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

        public static List<string> GetPointsIgnoringContext(string funcSig)
        {
            var all = new List<string>();
            foreach (var funcs in _clientPoints.Values)
            {
                if (funcs.ContainsKey(funcSig))
                {
                    all.AddRange(funcs[funcSig]);
                    funcs.Remove(funcSig);
                }
            }
            return all;
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

        internal static bool ClarifyBusinessMethodName(string asmName, string realMethodName, ref string curSig)
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
                if (!string.IsNullOrWhiteSpace(realMethodName) && method.Name != realMethodName)
                    continue;
                if (method.GetCustomAttribute(typeof(DebuggerHiddenAttribute)) != null)
                    continue;
                if (method.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null)
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
                    return false;

                //all borders have been breached for call stack... but!
                if (typeFullName.Contains(DISPLAY_CLASS) || funcFullName.Contains(DISPLAY_CLASS))
                    continue;
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
                Debug.WriteLine($"Not processed immediately: {curSig}");
                if (_lastFuncByCtx.ContainsKey(id))
                {
                    //be aware for multithread environment
                    curSig = _lastFuncByCtx[id];
                }
                else
                {
                    //extract pure func name - TODO: regex
                    var curName = curSig.Split("::")[1].Split("(")[0]
                        .Replace("<", null).Replace(">", " ")
                        .Split(" ")[0];

                    //name from compiler generated 'DisplayClass'
                    string curName2 = null;
                    if (curSig.Contains(DISPLAY_CLASS))
                    {
                        var ar = curSig.Split("/");
                        curName2 = ar[ar.Length-1];
                        if (curName2.Contains("::"))
                            curName2 = curName2.Split("::")[1];
                        var ind = curName2.StartsWith("<<") ? 2 : 1;
                        curName2 = curName2.Substring(ind, curName2.IndexOf(">") - ind);
                    }

                    //search using both vars
                    foreach (var existStr in _infoBySig.Keys)
                    {
                        var existName = existStr.Split("::")[1].Split("(")[0];
                        if (existName != curName && existName != curName2)
                            continue;
                        curSig = existStr.Split(";")[1];
                        _lastFuncByCtx.TryAdd(id, existStr);
                        processed = true;
                    }
                    if(!processed)
                    { }
                }
            }
            else
            {
                //last func
                if (_lastFuncByCtx.ContainsKey(id))
                    _lastFuncByCtx[id] = curSig;
                else
                    _lastFuncByCtx.TryAdd(id, curSig);
            }
            return true;
        }
    }
}
