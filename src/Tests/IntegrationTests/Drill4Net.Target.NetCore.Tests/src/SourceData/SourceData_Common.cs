using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Target.Common;
using static Drill4Net.Target.NetCore.Tests.SourceDataCore;

namespace Drill4Net.Target.NetCore.Tests
{
    internal class SourceData_Common
    {
        #region CONSTs
        private const string CATEGORY_DYNAMIC = "Dynamic";
        private const string CATEGORY_MISC = "Misc";
        #endregion
        #region FIELDs
        private static readonly Common.InjectTarget _targetCommon;
        private static readonly Net50.InjectTarget _target31;

        private static readonly GenStr _genStr;
        private static readonly Point _point;
        private static readonly NotEmptyStringEnumerator _strEnumerator;
        private static readonly Eventer _eventer;
        #endregion

        /************************************************************************/

        static SourceData_Common()
        {
            _targetCommon = new InjectTarget();
            _target31 = new Core31.InjectTarget();

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
                var a = GetSimple(_targetCommon).ToList();
                var b = GetSimple(_target31).ToList();
                a.AddRange(b);
                return a;
            }
        }

        private static IEnumerable<TestCaseData> GetSimple(IInjectTarget target)
        {
            //get
            //{
                #region If/Else
                yield return GetCase(target, GetInfo(target.IfElse_Half), new object[] { false }, new List<string>());
                yield return GetCase(target, GetInfo(target.IfElse_Half), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(target, GetInfo(target.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(target, GetInfo(target.IfElse_FullSimple), new object[] { true }, new List<string> { "If_6" });

                yield return GetCase(target, GetInfo(target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_25", "Else_56" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_Full), new object[] { false, true }, new List<string> { "Else_25", "If_41" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_10", "Else_56" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_10", "If_41" });

                yield return GetCase(target, GetInfo(target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_26" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_17" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_26" });
                yield return GetCase(target, GetInfo(target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_17" });

                yield return GetCase(target, GetInfo(target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string> { "Else_22" });
                yield return GetCase(target, GetInfo(target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(target, GetInfo(target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string> { "Else_24" });
                yield return GetCase(target, GetInfo(target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_8" });

                yield return GetCase(target, GetInfo(target.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                yield return GetCase(target, GetInfo(target.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "If_9" });

                yield return GetCase(target, GetInfo(target.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Else_9" });
                yield return GetCase(target, GetInfo(target.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

                yield return GetCase(target, GetInfo(target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_27", "Else_46" });
                yield return GetCase(target, GetInfo(target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_27", "If_37" });
                yield return GetCase(target, GetInfo(target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(target, GetInfo(target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(target, GetInfo(target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                yield return GetCase(target, GetInfo(target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_22" });
                yield return GetCase(target, GetInfo(target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(target, GetInfo(target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                yield return GetCase(target, GetInfo(target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                yield return GetCase(target, GetInfo(target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_21" });
                yield return GetCase(target, GetInfo(target.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                yield return GetCase(target, GetInfo(target.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });
                #endregion
                #region Switch
                yield return GetCase(target, GetInfo(target.Switch_ExplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                yield return GetCase(target, GetInfo(target.Switch_ExplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                yield return GetCase(target, GetInfo(target.Switch_ExplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                yield return GetCase(target, GetInfo(target.Switch_ExplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                yield return GetCase(target, GetInfo(target.Switch_ImplicitDefault), new object[] { -1 }, new List<string> { "If_15" });
                yield return GetCase(target, GetInfo(target.Switch_ImplicitDefault), new object[] { 0 }, new List<string> { "If_25" });
                yield return GetCase(target, GetInfo(target.Switch_ImplicitDefault), new object[] { 1 }, new List<string> { "If_30" });
                yield return GetCase(target, GetInfo(target.Switch_ImplicitDefault), new object[] { 2 }, new List<string> { "Switch_12" }); //default of switch statement

                yield return GetCase(target, GetInfo(target.Switch_WithoutDefault), new object[] { -1 }, new List<string> { "If_13" });
                yield return GetCase(target, GetInfo(target.Switch_WithoutDefault), new object[] { 0 }, new List<string> { "If_23" });
                yield return GetCase(target, GetInfo(target.Switch_WithoutDefault), new object[] { 1 }, new List<string> { "If_33" });
                yield return GetCase(target, GetInfo(target.Switch_WithoutDefault), new object[] { 2 }, new List<string> { "Switch_10" }); //place of Switch statement

                yield return GetCase(target, GetInfo(target.Switch_AsReturn), new object[] { -1 }, new List<string> { "If_19" });
                yield return GetCase(target, GetInfo(target.Switch_AsReturn), new object[] { 0 }, new List<string> { "If_24" });
                yield return GetCase(target, GetInfo(target.Switch_AsReturn), new object[] { 1 }, new List<string> { "If_29" });
                yield return GetCase(target, GetInfo(target.Switch_AsReturn), new object[] { 2 }, new List<string> { "Switch_16" });

                yield return GetCase(target, GetInfo(target.Switch_When), new object[] { 0 }, new List<string> { "If_20", "If_28" });
                yield return GetCase(target, GetInfo(target.Switch_When), new object[] { 1 }, new List<string> { "If_20", "Else_34" });
                yield return GetCase(target, GetInfo(target.Switch_When), new object[] { -1 }, new List<string> { "If_14" });

                yield return GetCase(target, GetInfo((OneNullBoolFuncStr)target.Switch_Property), new object[] { null }, new List<string> { "Else_30", "If_45", "If_72" });
                yield return GetCase(target, GetInfo((OneNullBoolFuncStr)target.Switch_Property), new object[] { false }, new List<string> { "If_5", "Else_19", "If_45", "Else_54", "If_68", "If_85" });
                yield return GetCase(target, GetInfo((OneNullBoolFuncStr)target.Switch_Property), new object[] { true }, new List<string> { "If_5", "If_27", "If_45", "Else_54", "Else_60", "If_88" });

                yield return GetCase(target, GetInfo(target.Switch_Tuple), new object[] { "English", "morning" }, new List<string> { "If_17", "If_41" });
                yield return GetCase(target, GetInfo(target.Switch_Tuple), new object[] { "English", "evening" }, new List<string> { "If_17", "Else_22", "If_46" });
                yield return GetCase(target, GetInfo(target.Switch_Tuple), new object[] { "German", "morning" }, new List<string> { "Else_9", "If_28", "If_53" });
                yield return GetCase(target, GetInfo(target.Switch_Tuple), new object[] { "German", "evening" }, new List<string> { "Else_9", "If_28", "Else_35", "If_58" });
                #endregion
                #region Elvis
                yield return GetCase(target, GetInfo(target.Elvis_NotNull), Array.Empty<object>(), new List<string> { "If_12" });
                yield return GetCase(target, GetInfo(target.Elvis_Null), Array.Empty<object>(), new List<string> { "Else_6" });

                yield return GetCase(target, GetInfo(target.Elvis_Sequence_NotNull), Array.Empty<object>(), new List<string> { "If_14", "If_27" });
                yield return GetCase(target, GetInfo(target.Elvis_Sequence_Null), Array.Empty<object>(), new List<string> { "Else_6" });

                yield return GetCase(target, GetInfo(target.Elvis_Double_NotNull), Array.Empty<object>(), new List<string>());
                yield return GetCase(target, GetInfo(target.Elvis_Double_Null), Array.Empty<object>(), new List<string> { "Else_7" });
                #endregion
                #region Linq
                yield return GetCase(target, GetInfo(target.Linq_Query), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(target, GetInfo(target.Linq_Query), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });

                yield return GetCase(target, GetInfo(target.Linq_Fluent), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(target, GetInfo(target.Linq_Fluent), new object[] { true }, new List<string> { "If_9", "If_9", "If_9" });
                #endregion
                #region Lambda
                yield return GetCase(target, GetInfo(target.Lambda), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(target, GetInfo(target.Lambda), new object[] { 10 }, new List<string> { "Else_2" });

                yield return GetCase(target, GetInfo(target.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(target, GetInfo(target.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Else_2" });
                yield return GetCase(target, GetInfo(target.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Else_2", "If_22" });

                yield return GetCase(target, GetInfo(target.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "If_9" });
                yield return GetCase(target, GetInfo(target.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Else_2", "If_29" });
                yield return GetCase(target, GetInfo(target.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Else_2", "If_34" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(target, GetInfo(target.Try_Exception_Conditional), new object[] { false }, new List<string>());
                yield return GetCase(target, GetInfo(target.Try_Exception_Conditional), new object[] { true }, new List<string> { "If_20", "Throw_26" });

                yield return GetCase(target, GetInfo(target.Try_Catch), new object[] { false }, new List<string> { "Throw_7", "Else_13" });
                yield return GetCase(target, GetInfo(target.Try_Catch), new object[] { true }, new List<string> { "Throw_7", "If_18" });

                yield return GetCase(target, GetInfo(target.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(target, GetInfo(target.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_7", "CatchFilter_16", "Else_22" });
                yield return GetCase(target, GetInfo(target.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_7", "CatchFilter_16" });
                yield return GetCase(target, GetInfo(target.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_7", "CatchFilter_16", "If_27" });

                yield return GetCase(target, GetInfo(target.Try_Finally), new object[] { false }, new List<string> { "Else_12" });
                yield return GetCase(target, GetInfo(target.Try_Finally), new object[] { true }, new List<string> { "If_17" });
                #endregion
                #region Dynamic
                yield return GetCase(target, GetInfo(target.ExpandoObject), new object[] { false }, new List<string> { "Else_2" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(target, GetInfo(target.ExpandoObject), new object[] { true }, new List<string> { "If_7" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(target, GetInfo(target.Cycle_Do), Array.Empty<object>(), new List<string> { "If_10" });

                yield return GetCase(target, GetInfo(target.Cycle_For), new object[] { -1 }, new List<string>());
                yield return GetCase(target, GetInfo(target.Cycle_For), new object[] { 3 }, new List<string> { "Cycle_22", "Cycle_22", "Cycle_22" });

                yield return GetCase(target, GetInfo(target.Cycle_While), new object[] { -1 }, new List<string>());
                yield return GetCase(target, GetInfo(target.Cycle_While), new object[] { 3 }, new List<string> { "Cycle_20", "Cycle_20", "Cycle_20" });


                #endregion
                #region Misc
                yield return GetCase(target, GetInfo(target.Goto_Statement), new object[] { false }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, GetInfo(target.Goto_Statement), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, GetInfo(target.Goto_Statement_Cycle_Out), new object[] { false }, new List<string> { "If_19", "Cycle_35", "If_19", "Cycle_35" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, GetInfo(target.Goto_Statement_Cycle_Out), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, GetInfo(target.Lock_Statement), new object[] { false }, new List<string> { "Else_14" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, GetInfo(target.Lock_Statement), new object[] { true }, new List<string> { "If_19" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, GetInfo(target.WinAPI), new object[] { false }, new List<string> { "Else_5" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, GetInfo(target.WinAPI), new object[] { true }, new List<string> { "If_10" }).SetCategory(CATEGORY_MISC);

                //only for NetFramework?
                //yield return GetCase(target, GetInfo(_target.ContextBound), new object[] { false }, new List<string> { "Else_5" });
                //yield return GetCase(target, GetInfo(_target.ContextBound), new object[] { true }, new List<string> { "If_9" });
                #endregion
            //}
        }

        internal static IEnumerable ParentChild
        {
            get
            {
                var a = GetParentChild(_targetCommon).ToList();
                var b = GetParentChild(_target31).ToList();
                a.AddRange(b);
                return a;
            }
        }

        internal static IEnumerable<TestCaseData> GetParentChild(InjectTarget target)
        {

                #region Generics
                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Generics_Call_Base), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "If_13" }));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Generics_Call_Child), new List<string> { "Else_7" }));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Generics_Call_Child), new List<string> { "If_12" }), new TestInfo(GetInfo(_genStr.GetShortDesc), new List<string>()), new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_8" }));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Generics_Var), new List<string> { "Else_37" }));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Generics_Var), new List<string> { "If_20", "If_30" }));
                #endregion
                #region Anonymous
                yield return GetCase(target, Array.Empty<object>(), new TestInfo(GetInfo(target.Anonymous_Func), new List<string> { "If_6" }));

                //at the moment, we decided not to consider local functions as separate entities 
                yield return GetCase(target, Array.Empty<object>(), new TestInfo(GetInfo(target.Anonymous_Func_WithLocalFunc), new List<string> { "If_6" }));

                yield return GetCase(target, new object[] { false }, true, new TestInfo(GetInfo(target.Anonymous_Type), new List<string> { "Else_5" }));
                yield return GetCase(target, new object[] { true }, true, new TestInfo(GetInfo(target.Anonymous_Type), new List<string> { "If_10" }));
                #endregion
                #region Async/await
                yield return GetCase(target, Array.Empty<object>(), true, true, 
                    new TestInfo(GetInfo(target.Async_Stream), new List<string> { "If_54" }), 
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceAsync()"), false, new List<string> { "Else_24", "Else_24", "Else_24", "If_36", "If_36" }, true));

                yield return GetCase(target, Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(target.Async_Stream_Cancellation), new List<string> { "If_76" }),
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceWithCancellationAsync(System.Threading.CancellationToken)"), false, new List<string> { "Else_24", "Else_24", "Else_24" }, true));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Async_Task), new List<string> { "Else_59" }), new TestInfo(GetInfo(target.Delay100), new List<string>()));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Async_Task), new List<string> { "If_17" }));

                yield return GetCase(target, new object[] { false }, true, true, new TestInfo(GetInfo(target.Async_Lambda), new List<string> { "Else_60" }));
                yield return GetCase(target, new object[] { true }, true, true, new TestInfo(GetInfo(target.Async_Lambda), new List<string> { "If_18" }));

                //yield return GetCase(target, new object[] { false }, true, new TestInfo(GetInfo(target.Async_Linq_Blocking), new List<string>()), new TestInfo(GetInfo(target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(target.ProcessElement), new List<string>()));
                //yield return GetCase(target, new object[] { true }, true, new TestInfo(GetInfo(target.Async_Linq_Blocking),  new List<string>()), new TestInfo(GetInfo(target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));

                //yield return GetCase(target, new object[] { false }, true, true, new TestInfo(GetInfo(target.Async_Linq_NonBlocking), new List<string>()), new TestInfo(GetInfo(target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(target.ProcessElement), new List<string>())).Ignore(TestConstants.INFLUENCE);
                //yield return GetCase(target, new object[] { true }, true, true, new TestInfo(GetInfo(target.Async_Linq_NonBlocking), new List<string>()), new TestInfo(GetInfo(target.GetDataForAsyncLinq), new List<string>()), new TestInfo(GetInfo(target.ProcessElement), new List<string> { "If_5", "If_5", "If_5" }));
                #endregion
                #region Parallel
                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Parallel_Linq), new List<string> { "Else_16", "Else_16", "Else_16", "Else_16", "Else_16" }));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Parallel_Linq), new List<string> { "If_2", "If_2", "If_2", "If_2", "If_2", "If_7", "If_7", "If_7", "If_7", "If_7" }, true));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Parallel_For), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Parallel_For), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Parallel_Foreach), new List<string> { "Else_17", "Else_17", "Else_17", "Else_17", "Else_17", "If_26", "If_26", "If_26", "If_26", "If_26" }, true));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Parallel_Foreach), new List<string> { "If_26", "If_26", "If_26", "If_3", "If_3", "If_3", "If_3", "If_3", "If_8", "If_8", "If_8", "If_8", "If_8" }, true));

                //data migrates from one func to another depending on running other similar tests... See next option for execute them
                //yield return GetCase(target, new object[] { false }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "Else_11"}), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "Else_4" }));
                //yield return GetCase(target, new object[] { true }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "If_3" }), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "If_12" }));

                yield return GetCase(target, new object[] { false }, false, true, true, new TestInfo(GetInfo(target.Parallel_Task_New), new List<string> { "Else_12", "Else_4" }, true));
                yield return GetCase(target, new object[] { true }, false, true, true, new TestInfo(GetInfo(target.Parallel_Task_New), new List<string> { "If_13", "If_3" }, true));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(target.GetStringListForThreadNew), new List<string> { "Else_4" }));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Parallel_Thread_New), new List<string>()), new TestInfo(GetInfo(target.GetStringListForThreadNew), new List<string> { "If_13" }));
                #endregion
                #region Dynamic
                yield return GetCase(target, new object[] { false }, 
                    new TestInfo(GetInfo(target.DynamicObject), new List<string> { "Else_2" }),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));
                
                yield return GetCase(target, new object[] { true }, 
                    new TestInfo(GetInfo(target.DynamicObject), new List<string> { "If_7" }),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));
                #endregion
                #region Disposable
                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Disposable_Using_SyncRead), new List<string>()));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Disposable_Using_SyncRead), new List<string> { "If_17" }));

                yield return GetCase(target, new object[] { false }, true, true, new TestInfo(GetInfo(target.Disposable_Using_AsyncRead), new List<string>()));
                yield return GetCase(target, new object[] { true }, true, true, new TestInfo(GetInfo(target.Disposable_Using_AsyncRead), new List<string> { "If_34" }));

                yield return GetCase(target, new object[] { false }, true, true, new TestInfo(GetInfo(target.Disposable_Using_AsyncTask), new List<string>()));
                yield return GetCase(target, new object[] { true }, true, true, new TestInfo(GetInfo(target.Disposable_Using_AsyncTask), new List<string> { "If_34" }));

                yield return GetCase(target, Array.Empty<object>(), new TestInfo(GetInfo(target.Disposable_Using_Last_Exception), new List<string> { "Throw_10" }));

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Disposable_Using_Exception), new List<string>()));
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Disposable_Using_Exception), new List<string> { "If_15", "Throw_20" }));

                //class::Finalize() is the thing-in-itself
                yield return GetCase(target, new object[] { (ushort)17 }, true,
                    new TestInfo(GetInfo(target.Disposable_Finalizer), new List<string>()), 
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "If_31", "If_8" }, true));

                //still not work togeteher with previous call
                yield return GetCase(target, new object[] { (ushort)18 }, true,
                    new TestInfo(GetInfo(target.Disposable_Finalizer), new List<string>()),
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "Else_12", "If_30" }, true)).Ignore(TestConstants.INFLUENCE);
                #endregion
                #region Misc
                yield return GetCase(target, new object[] { false }, true, new TestInfo(GetInfo(target.Yield), new List<string>()), new TestInfo(GetInfo(target.GetForYield), new List<string> { "Else_44" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, new object[] { true }, true, new TestInfo(GetInfo(target.Yield), new List<string>()), new TestInfo(GetInfo(target.GetForYield), new List<string> { "If_49" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Extension), new List<string>()), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "Else_4" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Extension), new List<string>()), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "If_9" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, Array.Empty<object>(), true, 
                    new TestInfo(GetInfo(target.Event), new List<string>()),
                    new TestInfo(GetInfo(_eventer.NotifyAbout), new List<string> { "If_11" }))
                    .SetCategory(CATEGORY_MISC);

                yield return GetCase(target, Array.Empty<object>(), true,
                    new TestInfo(GetInfo(target.Enumerator_Implementation), new List<string> { "Cycle_21", "Cycle_21", "Cycle_21", "Cycle_21" }),
                    new TestInfo(GetSourceFromFullSig("System.Collections.Generic.IEnumerator`1<System.String> Drill4Net.Target.Common.StringEnumerable::GetEnumerator()"), false, new List<string>()),
                    new TestInfo(GetInfo(_strEnumerator.MoveNext), new List<string> { "Else_22", "Else_22", "Else_22", "Else_22", "If_16" }),
                    new TestInfo(GetSourceFromFullSig("System.String Drill4Net.Target.Common.NotEmptyStringEnumerator::get_Current()"), false, new List<string> { "Else_6", "Else_6", "Else_6", "Else_6" }),
                    new TestInfo(GetSourceFromFullSig("System.Void Drill4Net.Target.Common.NotEmptyStringEnumerator::Dispose()"), false, new List<string>()))
                    .SetCategory(CATEGORY_MISC);

                //we dont't take into account local func as separate entity
                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.LocalFunc), new List<string> { "Else_2" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.LocalFunc), new List<string> { "If_7" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(target, new object[] { false }, new TestInfo(GetInfo(target.Unsafe), new List<string> { "Else_9" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(target, new object[] { true }, new TestInfo(GetInfo(target.Unsafe), new List<string> { "If_14" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                #endregion
        }
    }
}