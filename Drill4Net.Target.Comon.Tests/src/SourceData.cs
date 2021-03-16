using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Comon.Tests
{
    internal class SourceData
    {
        private static readonly InjectTarget _target;
        private static readonly GenStr _genStr;
        private static readonly Point _point;
        private static string INFLUENCE = "The passing of the test is affected by some other asynchronous tests. May be test will pass in Debug Test Mode.";

        private static string CATEGORY_DYNAMIC = "Dynamic";
        private static string CATEGORY_MISC = "Misc";

        /************************************************************************/

        static SourceData()
        {
            _target = new InjectTarget();
            _genStr = new GenStr("");
            _point = new Point();
        }

        /************************************************************************/

        internal static IEnumerable ParentChild
        {
            get
            {
                #region Generics
                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Generics_Call_Child), new List<string> { "Else_7" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Generics_Call_Child), new List<string> { "If_11" }), new TestData(GetInfo(_genStr.GetShortDesc), new List<string>()), new TestData(GetInfo(_genStr.GetDesc), new List<string> { "Else_12" }));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Generics_Var), new List<string> { "Else_38" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Generics_Var), new List<string> { "If_20", "If_30" }));
                #endregion
                #region Anonymous
                yield return GetCase(Array.Empty<object>(), new TestData(GetInfo(_target.Anonymous_Func), new List<string> { "If_6" }));

                yield return GetCase(Array.Empty<object>(), true, new TestData(GetInfo(_target.Anonymous_Func_WithLocalFunc), new List<string> { "If_8" }));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Anonymous_Type), new List<string> { "Else_5" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Anonymous_Type), new List<string> { "If_9" }));
                #endregion
                #region Async/await
                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Async_Task), new List<string> { "Else_58" }), new TestData(GetInfo(_target.Delay100), new List<string>()));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Async_Task), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestData(GetInfo(_target.Async_Lambda), new List<string> { "Else_59" }));
                yield return GetCase(new object[] { true }, true, true, new TestData(GetInfo(_target.Async_Lambda), new List<string> { "If_18" }));

                yield return GetCase(new object[] { false }, true, new TestData(GetInfo(_target.Async_Linq_Blocking), new List<string>()), new TestData(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestData(GetInfo(_target.ProcessElement), new List<string>()));
                yield return GetCase(new object[] { true }, true, new TestData(GetInfo(_target.Async_Linq_Blocking),  new List<string>()), new TestData(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestData(GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));

                yield return GetCase(new object[] { false }, true, true, new TestData(GetInfo(_target.Async_Linq_NonBlocking), new List<string> { "Else_83", "Else_95" }), new TestData(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestData(GetInfo(_target.ProcessElement), new List<string>())).Ignore(INFLUENCE);
                yield return GetCase(new object[] { true }, true, true, new TestData(GetInfo(_target.Async_Linq_NonBlocking), new List<string> { "Else_83", "Else_95" }), new TestData(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestData(GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));
                #endregion
                #region Parallel
                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Parallel_Linq), new List<string> { "Else_19", "Else_19", "Else_19", "Else_19", "Else_19" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Parallel_Linq), new List<string> { "If_2", "If_2", "If_2", "If_2", "If_2", "If_7", "If_7", "If_7", "If_7", "If_7" }, true));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Parallel_For), new List<string> { "Else_20", "Else_20", "Else_20", "Else_20", "Else_20", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Parallel_For), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Parallel_Foreach), new List<string> { "Else_20", "Else_20", "Else_20", "Else_20", "Else_20", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Parallel_Foreach), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                //data migrates from one func to another depending on running other similar tests... See next option for execute them
                //yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "Else_11"}), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "Else_4" }));
                //yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "If_3" }), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "If_12" }));

                yield return GetCase(new object[] { false }, false, true, true, new TestData(GetInfo(_target.Parallel_Task_New), new List<string> { "Else_11", "Else_4" }, true));
                yield return GetCase(new object[] { true }, false, true, true, new TestData(GetInfo(_target.Parallel_Task_New), new List<string> { "If_12", "If_3" }, true));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Parallel_Thread_New), new List<string>()), new TestData(GetInfo(_target.GetStringListForThreadNew), new List<string> { "Else_4" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Parallel_Thread_New), new List<string>()), new TestData(GetInfo(_target.GetStringListForThreadNew), new List<string> { "If_12" }));
                #endregion
                #region IDisposable
                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Disposable_Using_SyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Disposable_Using_SyncRead), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestData(GetInfo(_target.Disposable_Using_AsyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestData(GetInfo(_target.Disposable_Using_AsyncRead), new List<string> { "If_34" }));

                yield return GetCase(new object[] { false }, true, true, new TestData(GetInfo(_target.Disposable_Using_AsyncTask), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestData(GetInfo(_target.Disposable_Using_AsyncTask), new List<string> { "If_34" }));

                //data will be located in different locations...
                yield return GetCase(new object[] { (ushort)17 }, new TestData(GetInfo(_target.Disposable_Finalizer), new List<string> { "If_8", "If_30" }));
                yield return GetCase(new object[] { (ushort)18 }, new TestData(GetInfo(_target.Disposable_Finalizer), new List<string> { "Else_12", "If_30" }));
                #endregion
                #region Misc
                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Generics_Call_Base), new List<string>()), new TestData(GetInfo(_genStr.GetDesc), new List<string> { "Else_12" }));
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Generics_Call_Base), new List<string>()), new TestData(GetInfo(_genStr.GetDesc), new List<string> { "If_16" }));

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Yield), new List<string> { "Else_44" }), new TestData(GetInfo(_target.GetForYield), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Yield), new List<string> { "If_48" }), new TestData(GetInfo(_target.GetForYield), new List<string>())).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.Unsafe), new List<string> { "Else_9" }), new TestData(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.Unsafe), new List<string> { "If_13" }), new TestData(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
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

                yield return GetCase(GetInfo(_target.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                yield return GetCase(GetInfo(_target.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_target.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Else_8" });
                yield return GetCase(GetInfo(_target.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

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
                yield return GetCase(GetInfo(_target.Lambda), new object[] { 5 }, new List<string> { "If_8" });
                yield return GetCase(GetInfo(_target.Lambda), new object[] { 10 }, new List<string> { "Else_2" });

                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "If_8" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Else_2" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Else_2", "If_22" });

                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "If_8", "Else_23", "Else_29" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Else_2", "If_32" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Else_2", "Else_23", "If_37" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(GetInfo(_target.Try_Exception_Conditional), new object[] { false }, new List<string>());
                yield return GetCase(GetInfo(_target.Try_Exception_Conditional), new object[] { true }, new List<string> { "If_20", "Throw_26" });

                yield return GetCase(GetInfo(_target.Try_Catch), new object[] { false }, new List<string> { "Throw_7", "Else_13" });
                yield return GetCase(GetInfo(_target.Try_Catch), new object[] { true }, new List<string> { "Throw_7", "If_17" });

                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_7", "CatchFilter_16", "Else_22" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_7", "CatchFilter_16", "If_26" });

                yield return GetCase(GetInfo(_target.Try_Finally), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(GetInfo(_target.Try_Finally), new object[] { true }, new List<string> { "If_16" });
                #endregion
                #region Dynamic
                yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { true }, new List<string> { "If_6" }).SetCategory(CATEGORY_DYNAMIC);

                yield return GetCase(GetInfo(_target.DynamicObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(_target.DynamicObject), new object[] { true }, new List<string> { "If_6" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(GetInfo(_target.Cycle_While), new object[] { -1 }, new List<string>());
                yield return GetCase(GetInfo(_target.Cycle_While), new object[] { 3 }, new List<string> { "While_20", "While_20", "While_20" });
                #endregion
                #region Misc
                yield return GetCase(GetInfo(_target.Lock_Statement), new object[] { false }, new List<string> { "Else_14" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_target.Lock_Statement), new object[] { true }, new List<string> { "If_18" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_target.WinAPI), new object[] { false }, new List<string> { "Else_5" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_target.WinAPI), new object[] { true }, new List<string> { "If_9" }).SetCategory(CATEGORY_MISC);

                //only for NetFramework?
                //yield return GetCase(GetInfo(_target.ContextBound), new object[] { false }, new List<string> { "Else_5" });
                //yield return GetCase(GetInfo(_target.ContextBound), new object[] { true }, new List<string> { "If_9" });
                #endregion
            }
        }

        /******************************************************************/

        #region Delegates
        internal delegate void EmptySig();
        internal delegate void OneBoolMethod(bool cond);
        internal delegate void TwoBoolMethod(bool cond, bool cond2);

        internal delegate bool OneBoolFunc(bool cond);
        internal delegate bool TwoBoolFunc(bool cond, bool cond2);

        internal delegate (bool, bool) OneBoolTupleFunc(bool cond);

        internal delegate void OneIntMethod(int digit);
        internal delegate string OneIntFuncStr(int digit);

        internal delegate string OneBoolFuncStr(bool digit);
        internal delegate Task FuncTask();
        internal delegate string FuncString();
        internal delegate Task OneBoolFuncTask(bool digit);
        internal delegate List<Common.GenStr> FuncListGetStr();
        internal delegate Task<Common.GenStr> ProcessElementDlg(Common.GenStr element, bool cond);
        internal delegate List<string> OneBoolFuncListStr(bool cond);
        internal delegate IEnumerable<string> OneBoolFuncIEnumerable(bool digit);
        internal delegate bool OneInt(int digit);
        #endregion
        #region Method info
        internal static MethodInfo GetInfo(OneBoolFuncIEnumerable method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(EmptySig method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(OneInt method)
        {
            return method.Method;
        }

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

        internal static MethodInfo GetInfo(OneBoolFuncListStr method)
        {
            return method.Method;
        }
        #endregion
        #region GetCase
        internal static TestCaseData GetCase(MethodInfo mi, object[] pars, List<string> checks = null)
        {
            var name = mi.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(mi, pars, checks).SetCategory(category).SetName(caption);
        }

        internal static TestCaseData GetCase(object[] pars, params TestData[] input)
        {
            return GetCase(pars, false, false, false, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool ignoreEnterReturns, params TestData[] input)
        {
            return GetCase(pars, false, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool ignoreEnterReturns, params TestData[] input)
        {
            return GetCase(pars, isAsync, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestData[] input)
        {
            Assert.IsNotNull(input);
            Assert.True(input.Length > 0);

            var name = input[0].Info.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(pars, isAsync, isBunch, ignoreEnterReturns, input).SetCategory(category).SetName(caption);
        }

        private static string GetCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return name.Split("_")[0];
        }

        private static string GetCaption(string name, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return name;
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
        #endregion
    }
}