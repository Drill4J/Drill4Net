using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Plugins.Testing;
using System.Threading.Tasks;
using System.Threading;

namespace Drill4Net.Target.Comon.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    public class InjectTargetTests
    {
        private IInjectorRepository _rep;
        private MainOptions _opts;
        private Type _type;
        private object _target;

        /****************************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            var cfg_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CoreConstants.CONFIG_TESTS_NAME);
            _rep = new InjectorRepository(cfg_path);
            //this is done on the post-build event of the Injector project
            //var injector = new InjectorEngine(_rep);
            //injector.Process();

            var _opts = _rep.Options;
            var profPath = Path.Combine(_opts.Destination.Directory, _opts.Tests.AssemblyName);
            var asm = Assembly.LoadFrom(profPath);
            _type = asm.GetType($"{_opts.Tests.Namespace}.{_opts.Tests.Class}");
            _target = Activator.CreateInstance(_type);
        }

        /****************************************************************************/

        [TestCaseSource(typeof(SourceData), "Simple"/*, Category = "Simple"*/)]
        public void Simple_Ok(MethodInfo mi, object[] args, List<string> checks)
        {
            //arrange
            Assert.NotNull(mi, $"MethodInfo is empty for: {mi}");

            //act
            try
            {
                mi.Invoke(_target, args);
            }
            catch { } //it's normal for business throws

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = GetFullSignature(mi);
            var source = GetSource(sig);
            Assert.True(funcs.ContainsKey(source));
            var points = funcs[source];

            CheckEnterAndLastReturnOrThrow(points);
            RemoveEnterAndLastReturn(points);
            Check(points, checks);
        }

        [TestCaseSource(typeof(SourceData), "ParentChild", Category = "ParentChild")]
        public void Parent_Child_Ok(object[] args, bool wait, bool ignoreEnterReturns, 
                                    params(MethodInfo Info, List<string> Checks)[] inputs)
        {
            //arrange
            var parentData = inputs[0];
            var mi = parentData.Info;
            Assert.NotNull(mi, $"MethodInfo is empty for: {mi}");

            //act
            mi.Invoke(_target, args);
            
            if (wait) //for some async
            {
                for (var i = 0; i < 10; i++) 
                    Thread.Sleep(50);
            }

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == inputs.Length);

            for (var i = 0; i < inputs.Length; i++)
            {
                var data = inputs[i];
                var source = GetSource(GetFullSignature(data.Info));
                Assert.True(funcs.ContainsKey(source));
                var childFunc = funcs[source];
                if (ignoreEnterReturns)
                {
                    var forDelete = childFunc.Where(a => a.StartsWith("Enter_") || a.StartsWith("Return_")).ToArray();
                    for(var j=0; j<forDelete.Length; j++)
                        childFunc.Remove(forDelete[j]);
                }
                else
                {
                    CheckEnterAndLastReturnOrThrow(childFunc);
                    RemoveEnterAndLastReturn(childFunc);
                }

                Check(childFunc, data.Checks);
            }
        }

        private void RemoveEnterAndLastReturn(List<string> points)
        {
            //Check() not checks Enter and last Return
            if (points[0].StartsWith("Enter_"))
                points.RemoveAt(0);
            var lastInd = points.Count - 1;
            if (points[lastInd].StartsWith("Return_"))
                points.RemoveAt(lastInd);
        }

        #region Auxiliary funcs
        private List<string> GetPoints(string shortSig)
        {
            var funcSig = GetFullSignature(shortSig);
            var asmName = GetModuleName();
            return TestProfiler.GetPoints(asmName, funcSig, false);
        }

        private string GetSource(string sig)
        {
            var funcSig = sig;
            var asmName = GetModuleName();
            return $"{asmName};{funcSig}";
        }

        private string GetModuleName()
        {
            return "Drill4Net.Target.Common.dll";
        }

        private string GetFullSignature(string shortSig)
        {
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} Drill4Net.Target.Common.InjectTarget::{name}";
        }

        internal static string GetFullSignature(MethodInfo mi)
        {
            var func = mi.Name;
            var type = mi.DeclaringType;
            var className = $"{type.Namespace}.{type.Name}";

            //parameters
            var pars = mi.GetParameters();
            var parNames = string.Empty;
            var lastInd = pars.Length - 1;
            for (var j = 0; j <= lastInd; j++)
            {
                var p = pars[j];
                parNames += p.ParameterType.FullName;
                if (j < lastInd)
                    parNames += ",";
            }

            //return type
            var retType = mi.ReturnType.FullName;
            if (retType.Contains("Version=")) //need simplify strong named type
            {
                retType = mi.ReturnParameter.ToString()
                    .Replace("[", "<").Replace("]", ">").Replace(" ", null);
            }
            var sig = $"{retType} {className}::{func}({parNames})";

            return sig;
        }

        private Dictionary<string, List<string>> GetFunctions()
        {
            return TestProfiler.GetFunctions(false);
        }

        private MethodInfo GetMethod(string shortSig)
        {
            var name = GetNameFromSig(shortSig);
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private string GetNameFromSig(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
            return name;
        }

        private void CheckEnterAndLastReturnOrThrow(List<string> points)
        {
            Assert.IsNotNull(points.FirstOrDefault(a => a == "Enter_0"), "No Enter");
            Assert.IsNotNull(points.Last(a => a.StartsWith("Return_") || a.StartsWith("Throw_")), "No last Return/Throw");
        }

        private void Check(List<string> points, List<string> checks)
        {
            Assert.IsTrue(points.Count == checks.Count);
            for (var i = 0; i < checks.Count; i++)
            {
                if (points[i] != checks[i])
                    Assert.Fail();
            }
        }
        #endregion

        internal class SourceData
        {
            #region Delegates
            internal delegate void OneBoolMethod(bool cond);
            internal delegate void TwoBoolMethod(bool cond, bool cond2);

            internal delegate bool OneBoolFunc(bool cond);
            internal delegate bool TwoBoolFunc(bool cond, bool cond2);

            internal delegate (bool,bool) OneBoolTupleFunc(bool cond);

            internal delegate void OneIntMethod(int digit);
            internal delegate string OneIntFuncStr(int digit);

            internal delegate string OneBoolFuncStr(bool digit);
            internal delegate Task FuncTask();
            internal delegate string FuncString();
            internal delegate Task OneBoolFuncTask(bool digit);
            internal delegate List<Common.GenStr> FuncListGetStr();
            internal delegate Task<Common.GenStr> ProcessElementDlg(Common.GenStr element, bool cond);
            #endregion

            private static readonly Common.InjectTarget _target;
            private static readonly Common.GenStr _genStr;

            /*****************************************************/

            static SourceData()
            {
                _target = new Common.InjectTarget();
                _genStr = new Common.GenStr("");
            }

            /******************************************************************/

            internal static IEnumerable ParentChild
            {
                get
                {
                    yield return GetCase(new object[] { false }, (GetInfo(_target.ThreadNew), new List<string>()), (GetInfo(_target.GetStringListForThreadNew), new List<string> { "Else_4" }));
                    yield return GetCase(new object[] { true }, (GetInfo(_target.ThreadNew), new List<string>()), (GetInfo(_target.GetStringListForThreadNew), new List<string> { "If_12" }));

                    yield return GetCase(new object[] { false }, (GetInfo(_target.Generic_Call_Base), new List<string>()), (GetInfo(_genStr.GetDesc), new List<string> { "Else_12" }));
                    yield return GetCase(new object[] { true }, (GetInfo(_target.Generic_Call_Base), new List<string>()), (GetInfo(_genStr.GetDesc), new List<string> { "If_16" }));

                    //paired test locates in the Simple category
                    yield return GetCase(new object[] { true }, (GetInfo(_target.Generic_Call_Child), new List<string> { "If_11"}), (GetInfo(_genStr.GetShortDesc), new List<string>()), (GetInfo(_genStr.GetDesc), new List<string> { "Else_12" }));

                    #region Async/await
                    //paired test locates in the Simple category
                    yield return GetCase(new object[] { false }, (GetInfo(_target.AsyncTask), new List<string> { "Else_58" }), (GetInfo(_target.Delay100), new List<string>()));

                    // how it test in NUnit?
                    //yield return GetCase(new object[] { false }, (GetInfo(_target.AsyncLambdaRunner), new List<string>()), (GetInfo(_target.AsyncLambda), new List<string> { "Else_59" }));
                    //yield return GetCase(new object[] { true }, (GetInfo(_target.AsyncLambdaRunner), new List<string> { "If_17" }), (GetInfo(_target.AsyncLambda), new List<string> { "Else_58" }));

                    //If both tests run together, one of them will crash
                    yield return GetCase(new object[] { false }, true, (GetInfo(_target.AsyncLinq_Blocking), new List<string>()), (GetInfo(_target.GetDataForAsyncLinq), new List<string>()), (GetInfo(_target.ProcessElement), new List<string>()));
                    yield return GetCase(new object[] { true }, true, (GetInfo(_target.AsyncLinq_Blocking),  new List<string>()), (GetInfo(_target.GetDataForAsyncLinq), new List<string>()), (GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));

                    //If both tests run together, one of them will crash
                    //yield return GetCase(new object[] { false }, true, true, (GetInfo(_target.AsyncLinq_NonBlocking), new List<string>()), (GetInfo(_target.GetDataForAsyncLinq), new List<string>()), (GetInfo(_target.ProcessElement), new List<string> { "Else_83", "Else_95" }));
                    //yield return GetCase(new object[] { true }, true, true, (GetInfo(_target.AsyncLinq_NonBlockingRunner), new List<string>()), (GetInfo(_target.AsyncLinq_NonBlocking), new List<string> { "Else_83", "Else_95" }), (GetInfo(_target.GetDataForAsyncLinq), new List<string>()), (GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));
                    #endregion
                }
            }

            internal static IEnumerable Simple
            {
                get
                {
                    #region If/Else
                    yield return GetCase(GetInfo(_target.IfElse_Half), new object[] { false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_Half), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_11" });
                    yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { true }, new List<string> { "If_6" });

                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_24", "Else_55" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, true },  new List<string> { "Else_24", "If_41" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_10", "Else_55" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_10", "If_41" });

                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_25" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_17" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_25" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_17" });

                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string>());                                                 
                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                    yield return GetCase(GetInfo(_target.Ternary_Positive), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.Ternary_Negative), new object[] { false }, new List<string> { "Else_8" });
                    yield return GetCase(GetInfo(_target.Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_30", "Else_45" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_30", "If_37" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_6", "Else_21" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_21" });
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_22" });
                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "If_6" });                    
                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });
                    #endregion
                    #region Switch
                    yield return GetCase(GetInfo(_target.Switch_ExplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                    yield return GetCase(GetInfo(_target.Switch_ExplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                    yield return GetCase(GetInfo(_target.Switch_ExplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                    yield return GetCase(GetInfo(_target.Switch_ExplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                    yield return GetCase(GetInfo(_target.Switch_ImplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                    yield return GetCase(GetInfo(_target.Switch_ImplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                    yield return GetCase(GetInfo(_target.Switch_ImplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                    yield return GetCase(GetInfo(_target.Switch_ImplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                    yield return GetCase(GetInfo(_target.Switch_WithoutDefault), new object[] { -1 }, new List<string> { "If_13" });
                    yield return GetCase(GetInfo(_target.Switch_WithoutDefault), new object[] { 0 }, new List<string> { "If_23" });
                    yield return GetCase(GetInfo(_target.Switch_WithoutDefault), new object[] { 1 }, new List<string> { "If_33" });
                    yield return GetCase(GetInfo(_target.Switch_WithoutDefault), new object[] { 2 }, new List<string> { "Switch_10" }); //place of Switch statement

                    yield return GetCase(GetInfo(_target.Switch_AsReturn), new object[] { -1 }, new List<string> { "If_19" });
                    yield return GetCase(GetInfo(_target.Switch_AsReturn), new object[] { 0 }, new List<string> { "If_24" });
                    yield return GetCase(GetInfo(_target.Switch_AsReturn), new object[] { 1 }, new List<string> { "If_29" });
                    yield return GetCase(GetInfo(_target.Switch_AsReturn), new object[] { 2 }, new List<string> { "Switch_16" });
                    #endregion
                    #region Linq
                    yield return GetCase(GetInfo(_target.Linq_Query), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                    yield return GetCase(GetInfo(_target.Linq_Query), new object[] { true }, new List<string> { "If_8", "If_8", "If_8" });

                    yield return GetCase(GetInfo(_target.Linq_Fluent), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                    yield return GetCase(GetInfo(_target.Linq_Fluent), new object[] { true }, new List<string> { "If_8", "If_8", "If_8" });
                    #endregion
                    #region Lambda
                    yield return GetCase(GetInfo(_target.Lambda10), new object[] { 5 }, new List<string> { "If_8" });
                    yield return GetCase(GetInfo(_target.Lambda10), new object[] { 10 }, new List<string> { "Else_2" });

                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalBranch), new object[] { 5 }, new List<string> { "If_8" });
                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalBranch), new object[] { 10 }, new List<string> { "Else_2" });
                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalBranch), new object[] { 12 }, new List<string> { "Else_2", "If_22" });

                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalSwitch), new object[] { 5 }, new List<string> { "If_8", "Else_23", "Else_29" });
                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalSwitch), new object[] { 10 }, new List<string> { "Else_2", "If_32" });
                    yield return GetCase(GetInfo(_target.Lambda10_AdditionalSwitch), new object[] { 12 }, new List<string> { "Else_2", "Else_23", "If_37" });
                    #endregion
                    #region Generics
                    yield return GetCase(GetInfo(_target.GenericVar), new object[] { false }, new List<string> { "Else_38" });
                    yield return GetCase(GetInfo(_target.GenericVar), new object[] { true }, new List<string> { "If_20", "If_30" });

                    //paired test locates in the ParentChild category
                    yield return GetCase(GetInfo(_target.Generic_Call_Child), new object[] { false }, new List<string> { "Else_7" });
                    #endregion
                    #region Try/cath/finally
                    yield return GetCase(GetInfo(_target.Exception_Conditional), new object[] { false }, new List<string>());
                    yield return GetCase(GetInfo(_target.Exception_Conditional), new object[] { true }, new List<string> { "If_20", "Throw_26" });

                    yield return GetCase(GetInfo(_target.Catch_Statement), new object[] { false }, new List<string> { "Throw_7", "Else_13" });
                    yield return GetCase(GetInfo(_target.Catch_Statement), new object[] { true }, new List<string> { "Throw_7", "If_17" });

                    yield return GetCase(GetInfo(_target.Catch_When_Statement), new object[] { false, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                    yield return GetCase(GetInfo(_target.Catch_When_Statement), new object[] { false, true }, new List<string> { "Throw_7", "CatchFilter_16", "Else_22" });
                    yield return GetCase(GetInfo(_target.Catch_When_Statement), new object[] { true, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                    yield return GetCase(GetInfo(_target.Catch_When_Statement), new object[] { true, true }, new List<string> { "Throw_7", "CatchFilter_16", "If_26" });

                    yield return GetCase(GetInfo(_target.Finally_Statement), new object[] { false }, new List<string> { "Else_12" });
                    yield return GetCase(GetInfo(_target.Finally_Statement), new object[] { true }, new List<string> { "If_16" });
                    #endregion
                    #region Dynamic
                    yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { false }, new List<string> { "Else_2" });
                    yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { true }, new List<string> { "If_6" });

                    yield return GetCase(GetInfo(_target.DynamicObject), new object[] { false }, new List<string> { "Else_2" });
                    yield return GetCase(GetInfo(_target.DynamicObject), new object[] { true }, new List<string> { "If_6" });
                    #endregion
                    #region Async/await
                    //paired test locates in the Simple category
                    yield return GetCase(GetInfo(_target.AsyncTask), new object[] { true }, new List<string> { "If_17" });


                    #endregion
                }
            }

            /******************************************************************/

            #region Method info
            internal static MethodInfo GetInfo(OneBoolMethod method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(TwoBoolMethod method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolFunc method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(TwoBoolFunc method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolTupleFunc method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneIntMethod method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneIntFuncStr method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolFuncStr method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolFuncTask method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(FuncTask method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(FuncListGetStr method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(ProcessElementDlg method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(FuncString method)
            {
                return method.Method;
            }
            #endregion

            internal static TestCaseData GetCase(MethodInfo mi, object[] pars, List<string> checks)
            {
                var name = mi.Name;
                var caption = GetCaption(name, pars);
                return new TestCaseData(mi, pars, checks).SetName(caption);
            }

            internal static TestCaseData GetCase(object[] pars, bool ignoreEnterReturns, params (MethodInfo Info, List<string> Checks)[] input)
            {
                return GetCase(pars, false, ignoreEnterReturns, input);
            }

            internal static TestCaseData GetCase(object[] pars, params (MethodInfo Info, List<string> Checks)[] input)
            {
                return GetCase(pars, false, false, input);
            }

            internal static TestCaseData GetCase(object[] pars, bool wait, bool ignoreEnterReturns, params (MethodInfo Info, List<string> Checks)[] input)
            {
                Assert.IsNotNull(input);
                Assert.True(input.Length > 0);

                string caption = null;
                var data = new (MethodInfo Info, List<string> Checks)[input.Length];
                for (var i = 0; i < data.Length; i++)
                {
                    if(i == 0)
                        caption = GetCaption(input[0].Info.Name, pars);
                    data[i] = input[i];
                }
                return new TestCaseData(pars, wait, ignoreEnterReturns, data).SetName(caption);
            }

            private static string GetCaption(string name, object[] parameters)
            {
                name += ": ";
                var lastInd = parameters.Length - 1;
                for (int i = 0; i <= lastInd; i++)
                {
                    var par = parameters[i];
                    name += par;
                    if (i < lastInd)
                        name += ",";
                }
                return name;
            }
        }
    }
}