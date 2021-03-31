using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.NetCore.Tests
{
    internal class SourceData
    {
        #region CONSTs
        private const string INFLUENCE = "The passing of the test is affected by some other asynchronous tests. May be test will pass in Debug Test Mode.";

        private const string CATEGORY_DYNAMIC = "Dynamic";
        private const string CATEGORY_MISC = "Misc";
        #endregion
        #region FIELDs
        //only for getting method's signatures
        private static readonly InjectTarget _targetCommon;
        private static readonly Net50.InjectTarget _target50;
        private static readonly GenStr _genStr;
        private static readonly Point _point;
        private static readonly NotEmptyStringEnumerator _strEnumerator;
        private static readonly Eventer _eventer;
        #endregion

        /************************************************************************/

        static SourceData()
        {
            //only for getting method's signatures
            _targetCommon = new InjectTarget();
            _target50 = new Net50.InjectTarget();

            _genStr = new GenStr("");
            _point = new Point();
            _eventer = new Eventer();
            _strEnumerator = new NotEmptyStringEnumerator(null);
        }

        /************************************************************************/

        internal static IEnumerable Simple
        {
            get
            {
                #region If/Else
                yield return GetCase(GetInfo(_targetCommon.IfElse_Half), new object[] { false }, new List<string>());
                yield return GetCase(GetInfo(_targetCommon.IfElse_Half), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullSimple), new object[] { true }, new List<string> { "If_6" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_25", "Else_56" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_Full), new object[] { false, true }, new List<string> { "Else_25", "If_41" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_10", "Else_56" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_10", "If_41" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_26" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_17" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_26" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_17" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string> { "Else_22" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string> { "Else_24" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "If_9" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Else_9" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_27", "Else_46" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_27", "If_37" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                yield return GetCase(GetInfo(_targetCommon.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(GetInfo(_targetCommon.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_21" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                yield return GetCase(GetInfo(_targetCommon.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });
                #endregion
                #region Switch
                yield return GetCase(GetInfo(_targetCommon.Switch_ExplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ExplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ExplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ExplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                yield return GetCase(GetInfo(_targetCommon.Switch_ImplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ImplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ImplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                yield return GetCase(GetInfo(_targetCommon.Switch_ImplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                yield return GetCase(GetInfo(_targetCommon.Switch_WithoutDefault), new object[] { -1 }, new List<string> { "If_13" });
                yield return GetCase(GetInfo(_targetCommon.Switch_WithoutDefault), new object[] { 0 }, new List<string> { "If_23" });
                yield return GetCase(GetInfo(_targetCommon.Switch_WithoutDefault), new object[] { 1 }, new List<string> { "If_33" });
                yield return GetCase(GetInfo(_targetCommon.Switch_WithoutDefault), new object[] { 2 }, new List<string> { "Switch_10" }); //place of Switch statement

                yield return GetCase(GetInfo(_targetCommon.Switch_AsReturn), new object[] { -1 }, new List<string> { "If_19" });
                yield return GetCase(GetInfo(_targetCommon.Switch_AsReturn), new object[] { 0 }, new List<string> { "If_24" });
                yield return GetCase(GetInfo(_targetCommon.Switch_AsReturn), new object[] { 1 }, new List<string> { "If_29" });
                yield return GetCase(GetInfo(_targetCommon.Switch_AsReturn), new object[] { 2 }, new List<string> { "Switch_16" });

                yield return GetCase(GetInfo(_targetCommon.Switch_When), new object[] { 0 }, new List<string> { "If_20", "If_28" });
                yield return GetCase(GetInfo(_targetCommon.Switch_When), new object[] { 1 }, new List<string> { "If_20", "Else_34" });
                yield return GetCase(GetInfo(_targetCommon.Switch_When), new object[] { -1 }, new List<string> { "If_14" });

                yield return GetCase(GetInfo((OneNullBoolFuncStr)_targetCommon.Switch_Property), new object[] { null }, new List<string> { "Else_30", "If_45", "If_72" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)_targetCommon.Switch_Property), new object[] { false }, new List<string> { "If_5", "Else_19", "If_45", "Else_54", "If_68", "If_85" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)_targetCommon.Switch_Property), new object[] { true }, new List<string> { "If_5", "If_27", "If_45", "Else_54", "Else_60", "If_88" });

                yield return GetCase(GetInfo(_targetCommon.Switch_Tuple), new object[] { "English", "morning" }, new List<string> { "If_17", "If_41" });
                yield return GetCase(GetInfo(_targetCommon.Switch_Tuple), new object[] { "English", "evening" }, new List<string> { "If_17", "Else_22", "If_46" });
                yield return GetCase(GetInfo(_targetCommon.Switch_Tuple), new object[] { "German", "morning" }, new List<string> { "Else_9", "If_28", "If_53" });
                yield return GetCase(GetInfo(_targetCommon.Switch_Tuple), new object[] { "German", "evening" }, new List<string> { "Else_9", "If_28", "Else_35", "If_58" });

                yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { -5 }, new List<string> { "Else_8", "If_21" });
                yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 5 }, new List<string> { "Else_8", "If_25" });
                yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 10 }, new List<string> { "If_15", "If_31" });
                yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 100 }, new List<string> { "If_15", "If_39" });

                yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { -5 }, new List<string> { "If_15" });
                yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { 5 }, new List<string> { "Else_8", "If_20" });
                yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { 10 }, new List<string> { "Else_8", "If_24" });
                #endregion
                #region Elvis
                yield return GetCase(GetInfo(_targetCommon.Elvis_NotNull), Array.Empty<object>(), new List<string> { "If_12" });
                yield return GetCase(GetInfo(_targetCommon.Elvis_Null), Array.Empty<object>(), new List<string> { "Else_6" });

                yield return GetCase(GetInfo(_targetCommon.Elvis_Sequence_NotNull), Array.Empty<object>(), new List<string> { "If_14", "If_27" });
                yield return GetCase(GetInfo(_targetCommon.Elvis_Sequence_Null), Array.Empty<object>(), new List<string> { "Else_6" });

                yield return GetCase(GetInfo(_targetCommon.Elvis_Double_NotNull), Array.Empty<object>(), new List<string>());
                yield return GetCase(GetInfo(_targetCommon.Elvis_Double_Null), Array.Empty<object>(), new List<string> { "Else_7" });
                #endregion
                #region Linq
                yield return GetCase(GetInfo(_targetCommon.Linq_Query), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(_targetCommon.Linq_Query), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });

                yield return GetCase(GetInfo(_targetCommon.Linq_Fluent), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(_targetCommon.Linq_Fluent), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });
                #endregion
                #region Lambda
                yield return GetCase(GetInfo(_targetCommon.Lambda), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_targetCommon.Lambda), new object[] { 10 }, new List<string> { "Else_2" });

                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Else_2" });
                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Else_2", "If_22" });

                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Else_2", "If_29" });
                yield return GetCase(GetInfo(_targetCommon.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Else_2", "If_34" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(GetInfo(_targetCommon.Try_Exception_Conditional), new object[] { false }, new List<string>());
                yield return GetCase(GetInfo(_targetCommon.Try_Exception_Conditional), new object[] { true }, new List<string> { "If_20", "Throw_26" });

                yield return GetCase(GetInfo(_targetCommon.Try_Catch), new object[] { false }, new List<string> { "Throw_7", "Else_13" });
                yield return GetCase(GetInfo(_targetCommon.Try_Catch), new object[] { true }, new List<string> { "Throw_7", "If_18" });

                yield return GetCase(GetInfo(_targetCommon.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_targetCommon.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_7", "CatchFilter_16", "Else_22" });
                yield return GetCase(GetInfo(_targetCommon.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(GetInfo(_targetCommon.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_7", "CatchFilter_16", "If_27" });

                yield return GetCase(GetInfo(_targetCommon.Try_Finally), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(GetInfo(_targetCommon.Try_Finally), new object[] { true }, new List<string> { "If_17" });
                #endregion
                #region Dynamic
                yield return GetCase(GetInfo(_targetCommon.ExpandoObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(_targetCommon.ExpandoObject), new object[] { true }, new List<string> { "If_7" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(GetInfo(_targetCommon.Cycle_Do), Array.Empty<object>(), new List<string> { "If_10" });

                yield return GetCase(GetInfo(_targetCommon.Cycle_For), new object[] { -1 }, new List<string>());
                yield return GetCase(GetInfo(_targetCommon.Cycle_For), new object[] { 3 }, new List<string> { "Cycle_22", "Cycle_22", "Cycle_22" });

                yield return GetCase(GetInfo(_targetCommon.Cycle_While), new object[] { -1 }, new List<string>());
                yield return GetCase(GetInfo(_targetCommon.Cycle_While), new object[] { 3 }, new List<string> { "Cycle_20", "Cycle_20", "Cycle_20" });


                #endregion
                #region Misc
                yield return GetCase(GetInfo(_targetCommon.Goto_Statement), new object[] { false }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_targetCommon.Goto_Statement), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_targetCommon.Goto_Statement_Cycle_Out), new object[] { false }, new List<string> { "If_19", "Cycle_35", "If_19", "Cycle_35" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_targetCommon.Goto_Statement_Cycle_Out), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_targetCommon.Lock_Statement), new object[] { false }, new List<string> { "Else_14" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_targetCommon.Lock_Statement), new object[] { true }, new List<string> { "If_19" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(_targetCommon.WinAPI), new object[] { false }, new List<string> { "Else_5" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(_targetCommon.WinAPI), new object[] { true }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);

                //only for NetFramework?
                //yield return GetCase(GetInfo(_target.ContextBound), new object[] { false }, new List<string> { "Else_5" });
                //yield return GetCase(GetInfo(_target.ContextBound), new object[] { true }, new List<string> { "If_9" });
                #endregion
            }
        }

        internal static IEnumerable ParentChild
        {
            get
            {
                #region Generics
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "If_13" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Generics_Call_Child), new List<string> { "Else_7" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Generics_Call_Child), new List<string> { "If_12" }), new TestInfo(GetInfo(_genStr.GetShortDesc), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Generics_Var), new List<string> { "Else_37" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Generics_Var), new List<string> { "If_20", "If_30" }));
                #endregion
                #region Anonymous
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(_targetCommon.Anonymous_Func), new List<string> { "If_6" }));

                //at the moment, we decided not to consider local functions as separate entities 
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(_targetCommon.Anonymous_Func_WithLocalFunc), new List<string> { "If_6" }));

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(_targetCommon.Anonymous_Type), new List<string> { "Else_5" }));
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(_targetCommon.Anonymous_Type), new List<string> { "If_10" }));
                #endregion
                #region Async/await
                yield return GetCase(Array.Empty<object>(), true, true, 
                    new TestInfo(GetInfo(_targetCommon.Async_Stream), new List<string> { "If_54" }), 
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceAsync()"), false, new List<string> { "Else_24", "Else_24", "Else_24", "If_36", "If_36" }, true));

                yield return GetCase(Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(_targetCommon.Async_Stream_Cancellation), new List<string> { "If_76" }),
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceWithCancellationAsync(System.Threading.CancellationToken)"), false, new List<string> { "Else_24", "Else_24", "Else_24" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Async_Task), new List<string> { "Else_59" }), new TestInfo(GetInfo(_targetCommon.Delay100), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Async_Task), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_targetCommon.Async_Lambda), new List<string> { "Else_60" }));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_targetCommon.Async_Lambda), new List<string> { "If_18" }));

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(_targetCommon.Async_Linq_Blocking), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_targetCommon.ProcessElement), new List<string>()));
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(_targetCommon.Async_Linq_Blocking),  new List<string>()), new TestInfo(GetInfo(_targetCommon.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_targetCommon.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_targetCommon.Async_Linq_NonBlocking), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_targetCommon.ProcessElement), new List<string>())).Ignore(INFLUENCE);
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_targetCommon.Async_Linq_NonBlocking), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(_targetCommon.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));
                #endregion
                #region Parallel
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Parallel_Linq), new List<string> { "Else_16", "Else_16", "Else_16", "Else_16", "Else_16" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Parallel_Linq), new List<string> { "If_2", "If_2", "If_2", "If_2", "If_2", "If_7", "If_7", "If_7", "If_7", "If_7" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Parallel_For), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Parallel_For), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Parallel_Foreach), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Parallel_Foreach), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                //data migrates from one func to another depending on running other similar tests... See next option for execute them
                //yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "Else_11"}), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "Else_4" }));
                //yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "If_3" }), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "If_12" }));

                yield return GetCase(new object[] { false }, false, true, true, new TestInfo(GetInfo(_targetCommon.Parallel_Task_New), new List<string> { "Else_12", "Else_4" }, true));
                yield return GetCase(new object[] { true }, false, true, true, new TestInfo(GetInfo(_targetCommon.Parallel_Task_New), new List<string> { "If_13", "If_3" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetStringListForThreadNew), new List<string> { "Else_4" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetStringListForThreadNew), new List<string> { "If_13" }));
                #endregion
                #region Dynamic
                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(_targetCommon.DynamicObject), new List<string> { "Else_2" }),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));
                
                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(_targetCommon.DynamicObject), new List<string> { "If_7" }),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));
                #endregion
                #region Disposable
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Disposable_Using_SyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Disposable_Using_SyncRead), new List<string> { "If_17" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_targetCommon.Disposable_Using_AsyncRead), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_targetCommon.Disposable_Using_AsyncRead), new List<string> { "If_34" }));

                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(_targetCommon.Disposable_Using_AsyncTask), new List<string>()));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(_targetCommon.Disposable_Using_AsyncTask), new List<string> { "If_34" }));

                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(_targetCommon.Disposable_Using_Last_Exception), new List<string> { "Throw_10" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Disposable_Using_Exception), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Disposable_Using_Exception), new List<string> { "If_15", "Throw_20" }));

                //class::Finalize() is the thing-in-itself
                yield return GetCase(new object[] { (ushort)17 }, true,
                    new TestInfo(GetInfo(_targetCommon.Disposable_Finalizer), new List<string>()), 
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "If_31", "If_8" }, true));

                //still not work togeteher with previous call
                yield return GetCase(new object[] { (ushort)18 }, true,
                    new TestInfo(GetInfo(_targetCommon.Disposable_Finalizer), new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "Else_12", "If_30" }, true)).Ignore(INFLUENCE);
                #endregion
                #region Misc
                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(_targetCommon.Yield), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetForYield), new List<string> { "Else_44" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(_targetCommon.Yield), new List<string>()), new TestInfo(GetInfo(_targetCommon.GetForYield), new List<string> { "If_49" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Extension), new List<string>()), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "Else_4" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Extension), new List<string>()), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "If_9" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(Array.Empty<object>(), true, 
                    new TestInfo(GetInfo(_targetCommon.Event), new List<string>()),
                    new TestInfo(GetInfo(_eventer.NotifyAbout), new List<string> { "If_11" }))
                    .SetCategory(CATEGORY_MISC);

                yield return GetCase(Array.Empty<object>(), true,
                    new TestInfo(GetInfo(_targetCommon.Enumerator_Implementation), new List<string> { "Cycle_21", "Cycle_21", "Cycle_21", "Cycle_21" }),
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IEnumerator`1<System.String> Drill4Net.Target.Common.StringEnumerable::GetEnumerator()"), false, new List<string>()),
                    new TestInfo(GetInfo(_strEnumerator.MoveNext), new List<string> { "Else_22", "Else_22", "Else_22", "Else_22", "If_16" }),
                    new TestInfo(GetSourceFromFullSig("System.String Drill4Net.Target.Common.NotEmptyStringEnumerator::get_Current()"), false, new List<string> { "Else_6", "Else_6", "Else_6", "Else_6" }),
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.NotEmptyStringEnumerator::Dispose()"), false, new List<string>()))
                    .SetCategory(CATEGORY_MISC);

                //we dont't take into account local func as separate entity
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.LocalFunc), new List<string> { "Else_2" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.LocalFunc), new List<string> { "If_7" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(_targetCommon.Unsafe), new List<string> { "Else_9" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(_targetCommon.Unsafe), new List<string> { "If_14" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                #endregion
            }
        }

        /******************************************************************/

        #region Delegates
        internal delegate void EmptySig();
        internal delegate void OneBoolMethod(bool cond);
        internal delegate void TwoBoolMethod(bool cond, bool cond2);

        internal delegate bool OneBool();
        internal delegate bool OneBoolFunc(bool cond);
        internal delegate bool TwoBoolFunc(bool cond, bool cond2);

        internal delegate (bool, bool) OneBoolTupleFunc(bool cond);

        internal delegate void OneIntMethod(int digit);
        internal delegate string OneIntFuncStr(int digit);
        internal delegate double OneDoubleFuncDouble(double digit);

        internal delegate string OneBoolFuncStr(bool cond);
        internal delegate string OneNullBoolFuncStr(bool? cond);
        internal delegate string TwoStringFuncStr(string a, string b);
        internal delegate Task FuncTask();
        internal delegate IAsyncEnumerable<int> FuncAsyncEnum();
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
        internal static MethodInfo GetInfo(OneDoubleFuncDouble method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(TwoStringFuncStr method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(OneNullBoolFuncStr method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(OneBool method)
        {
            return method.Method;
        }

        internal static MethodInfo GetInfo(FuncAsyncEnum method)
        {
            return method.Method;
        }

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
        internal static TestCaseData GetCase(MethodInfo mi, object[] pars, List<string> checks)
        {
            return GetCase(null, mi, pars, checks);
        }

        internal static TestCaseData GetCase(object target, MethodInfo mi, object[] pars, List<string> checks)
        {
            var name = mi.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(target, mi, pars, checks)
                .SetCategory(category)
                .SetName(caption);
        }

        internal static TestCaseData GetCase(object[] pars, params TestInfo[] input)
        {
            return GetCase(null, pars, false, false, false, input);
        }

        internal static TestCaseData GetCase(object target, object[] pars, params TestInfo[] input)
        {
            return GetCase(target, pars, false, false, false, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(null, pars, false, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object target, object[] pars, bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(target, pars, false, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool ignoreEnterReturns,
            params TestInfo[] input)
        {
            return GetCase(null, pars, isAsync, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object target, object[] pars, bool isAsync, bool ignoreEnterReturns, 
            params TestInfo[] input)
        {
            return GetCase(target, pars, isAsync, false, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object[] pars, bool isAsync, bool isBunch,
            bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(null, pars, isAsync, isBunch, ignoreEnterReturns, input);
        }

        internal static TestCaseData GetCase(object target, object[] pars, bool isAsync, bool isBunch, 
            bool ignoreEnterReturns, params TestInfo[] input)
        {
            Assert.IsNotNull(input);
            Assert.True(input.Length > 0);

            var name = input[0].Info.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(target, pars, isAsync, isBunch, ignoreEnterReturns, input)
                .SetCategory(category).
                SetName(caption);
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
                name += par ?? "null";
                if (i < lastInd)
                    name += ",";
            }
            return name;
        }
        #endregion
        #region Source
        internal static string GetSource(string shortSig, object target)
        {
            return GetSourceFromFullSig(GetFullSignature(shortSig, target), target);
        }

        internal static string GetSourceFromFullSig(string fullSig, object target = null)
        {
            var asmName = GetModuleName(target);
            return $"{asmName};{fullSig}";
        }

        internal static string GetFullSignature(string shortSig, object target)
        {
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} {GetAssembly(target).GetName().Name}.InjectTarget::{name}";
        }

        internal static string GetModuleName(object target)
        {
            return GetAssembly(target).ManifestModule.Name;
        }

        internal static Assembly GetAssembly(object target)
        {
            if (target == null)
                target = _targetCommon;
            return target.GetType().Assembly;
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