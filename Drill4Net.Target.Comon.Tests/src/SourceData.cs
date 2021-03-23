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
        #region CONSTs
        private const string INFLUENCE = "The passing of the test is affected by some other asynchronous tests. May be test will pass in Debug Test Mode.";

        private const string CATEGORY_DYNAMIC = "Dynamic";
        private const string CATEGORY_MISC = "Misc";
        #endregion
        #region FIELDs
        private static readonly InjectTarget _target;
        private static readonly GenStr _genStr;
        private static readonly Point _point;
        #endregion

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
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "If_13" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Generics_Call_Child), new List<string> { "Else_7" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Generics_Call_Child), new List<string> { "If_12" }), new TestInfo(GetInfo(_genStr.GetShortDesc), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Generics_Var), new List<string> { "Else_37" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Generics_Var), new List<string> { "If_20", "If_30" }));
                #endregion
                #region Anonymous
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(_target.Anonymous_Func), new List<string> { "If_6" }));

                yield return GetCase(Array.Empty<object>(), true, new TestInfo(GetInfo(_target.Anonymous_Func_WithLocalFunc), new List<string> { "If_8" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Anonymous_Type), new List<string> { "Else_5" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Anonymous_Type), new List<string> { "If_10" }));
                #endregion
                #region Async/await
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Async_Task), new List<string> { "Else_59" }), new TestInfo(GetInfo(_target.Delay100), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Async_Task), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_target.Async_Lambda), new List<string> { "Else_60" }));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_target.Async_Lambda), new List<string> { "If_18" }));

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(_target.Async_Linq_Blocking), new List<string>()), new TestInfo(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_target.ProcessElement), new List<string>()));
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(_target.Async_Linq_Blocking),  new List<string>()), new TestInfo(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_target.Async_Linq_NonBlocking), new List<string> { "Else_83", "Else_95" }), new TestInfo(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_target.ProcessElement), new List<string>())).Ignore(INFLUENCE);
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_target.Async_Linq_NonBlocking), new List<string> { "Else_83", "Else_95" }), new TestInfo(GetInfo(_target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));
                #endregion
                #region Parallel
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Parallel_Linq), new List<string> { "Else_16", "Else_16", "Else_16", "Else_16", "Else_16" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Parallel_Linq), new List<string> { "If_2", "If_2", "If_2", "If_2", "If_2", "If_7", "If_7", "If_7", "If_7", "If_7" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Parallel_For), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Parallel_For), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Parallel_Foreach), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Parallel_Foreach), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                //data migrates from one func to another depending on running other similar tests... See next option for execute them
                //yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "Else_11"}), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "Else_4" }));
                //yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "If_3" }), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "If_12" }));

                yield return GetCase(new object[] { false }, false, true, true, new TestInfo(GetInfo(_target.Parallel_Task_New), new List<string> { "Else_12", "Else_4" }, true));
                yield return GetCase(new object[] { true }, false, true, true, new TestInfo(GetInfo(_target.Parallel_Task_New), new List<string> { "If_13", "If_3" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(_target.GetStringListForThreadNew), new List<string> { "Else_4" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(_target.GetStringListForThreadNew), new List<string> { "If_13" }));
                #endregion
                #region Disposable
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Disposable_Using_SyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Disposable_Using_SyncRead), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_target.Disposable_Using_AsyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_target.Disposable_Using_AsyncRead), new List<string> { "If_34" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_target.Disposable_Using_AsyncTask), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_target.Disposable_Using_AsyncTask), new List<string> { "If_34" }));

                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(_target.Disposable_Using_Last_Exception), new List<string> { "Throw_10" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Disposable_Using_Exception), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Disposable_Using_Exception), new List<string> { "If_15", "Throw_20" }));

                //class::Finalize() is the thing-in-itself
                yield return GetCase(new object[] { (ushort)17 }, true,
                    new TestInfo(GetInfo(_target.Disposable_Finalizer), new List<string>()), 
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "If_31", "If_8" }, true));

                //still not work togeteher with previous call
                yield return GetCase(new object[] { (ushort)18 }, true,
                    new TestInfo(GetInfo(_target.Disposable_Finalizer), new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "Else_12", "If_30" }, true)).Ignore(INFLUENCE);
                #endregion
                #region Misc
                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(_target.Yield), new List<string>()), new TestInfo(GetInfo(_target.GetForYield), new List<string> { "Else_44" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(_target.Yield), new List<string>()), new TestInfo(GetInfo(_target.GetForYield), new List<string> { "If_49" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_target.Unsafe), new List<string> { "Else_9" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_target.Unsafe), new List<string> { "If_14" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
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

                yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { true }, new List<string> { "If_6" });

                yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_25", "Else_56" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, true },  new List<string> { "Else_25", "If_41" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_10", "Else_56" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_10", "If_41" });

                yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_26" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_17" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_26" });
                yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_17" });

                yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string> { "Else_22" });
                yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string> { "Else_24" });                                                 
                yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_target.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                yield return GetCase(GetInfo(_target.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "If_9" });

                yield return GetCase(GetInfo(_target.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Else_9" });
                yield return GetCase(GetInfo(_target.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

                yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_27", "Else_46" });
                yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_27", "If_37" });
                yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_21" });
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

                yield return GetCase(GetInfo(_target.Switch_When), new object[] { 0 }, new List<string> { "If_20", "If_28" });
                yield return GetCase(GetInfo(_target.Switch_When), new object[] { 1 }, new List<string> { "If_20", "Else_34" });
                yield return GetCase(GetInfo(_target.Switch_When), new object[] { -1 }, new List<string> { "If_14" });
                #endregion
                #region Elvis
                yield return GetCase(GetInfo(_target.Elvis_Property_NotNull), Array.Empty<object>(), new List<string> { "If_12" });
                yield return GetCase(GetInfo(_target.Elvis_Property_Null), Array.Empty<object>(), new List<string> { "Else_6" });

                yield return GetCase(GetInfo(_target.Elvis_Property_NotNull_Double), Array.Empty<object>(), new List<string> { "If_14", "If_27" });
                yield return GetCase(GetInfo(_target.Elvis_Property_Null_Double), Array.Empty<object>(), new List<string> { "Else_6" });
                #endregion
                #region Linq
                yield return GetCase(GetInfo(_target.Linq_Query), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(_target.Linq_Query), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });

                yield return GetCase(GetInfo(_target.Linq_Fluent), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(_target.Linq_Fluent), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });
                #endregion
                #region Lambda
                yield return GetCase(GetInfo(_target.Lambda), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_target.Lambda), new object[] { 10 }, new List<string> { "Else_2" });

                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Else_2" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Else_2", "If_22" });

                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Else_2", "If_29" });
                yield return GetCase(GetInfo(_target.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Else_2", "If_34" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(GetInfo(_target.Try_Exception_Conditional), new object[] { false }, new List<string>());
                yield return GetCase(GetInfo(_target.Try_Exception_Conditional), new object[] { true }, new List<string> { "If_20", "Throw_26" });

                yield return GetCase(GetInfo(_target.Try_Catch), new object[] { false }, new List<string> { "Throw_7", "Else_13" });
                yield return GetCase(GetInfo(_target.Try_Catch), new object[] { true }, new List<string> { "Throw_7", "If_18" });

                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_7", "CatchFilter_16", "Else_22" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_target.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_7", "CatchFilter_16", "If_27" });

                yield return GetCase(GetInfo(_target.Try_Finally), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(GetInfo(_target.Try_Finally), new object[] { true }, new List<string> { "If_17" });
                #endregion
                #region Dynamic
                yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(_target.ExpandoObject), new object[] { true }, new List<string> { "If_7" }).SetCategory(CATEGORY_DYNAMIC);

                yield return GetCase(GetInfo(_target.DynamicObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(_target.DynamicObject), new object[] { true }, new List<string> { "If_7" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(GetInfo(_target.Cycle_While), new object[] { -1 }, new List<string>());
                yield return GetCase(GetInfo(_target.Cycle_While), new object[] { 3 }, new List<string> { "While_20", "While_20", "While_20" });
                #endregion
                #region Misc
                yield return GetCase(GetInfo(_target.Goto_Statement), new object[] { false }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_target.Goto_Statement), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_target.Lock_Statement), new object[] { false }, new List<string> { "Else_14" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_target.Lock_Statement), new object[] { true }, new List<string> { "If_19" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_target.WinAPI), new object[] { false }, new List<string> { "Else_5" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_target.WinAPI), new object[] { true }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);

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
        internal delegate void OneString(string par);
        internal delegate Task OneIntFuncTask(int digit);
        internal delegate string FuncString();
        internal delegate Task OneBoolFuncTask(bool digit);
        internal delegate List<GenStr> FuncListGetStr();
        internal delegate Task<GenStr> ProcessElementDlg(Common.GenStr element, bool cond);
        internal delegate List<string> OneBoolFuncListStr(bool cond);
        internal delegate IEnumerable<string> OneBoolFuncIEnumerable(bool digit);
        internal delegate bool OneInt(int digit);
        #endregion
        #region Method info
        internal static MethodInfo GetInfo(OneString method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(OneIntFuncTask method)
        {
            return method.Method;
        }

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

        internal static TestCaseData GetCase(object[] pars, params TestInfo[] input)
        {
            return GetCase(pars, false, false, false, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(pars, false, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(pars, isAsync, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] input)
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
        #region Source
        internal static string GetSource(string shortSig)
        {
            return GetSourceFromFullSig(GetFullSignature(shortSig));
        }

        internal static string GetSourceFromFullSig(string fullSig)
        {
            var asmName = GetModuleName();
            return $"{asmName};{fullSig}";
        }

        internal static string GetFullSignature(string shortSig)
        {
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} Drill4Net.Target.Common.InjectTarget::{name}";
        }

        internal static string GetModuleName()
        {
            return "Drill4Net.Target.Common.dll";
        }

        internal static string GetNameFromSig(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
            return name;
        }
        #endregion
    }
}