using System;
using System.Collections;
using System.Collections.Generic;
using Drill4Net.Target.Common;
using Drill4Net.Target.Common.Another;
using Drill4Net.Target.Common.VB;
using static Drill4Net.Target.Tests.Common.SourceDataCore;

namespace Drill4Net.Target.Tests.Common
{
    public class CommonSourceData
    {
        public static InjectTarget Target { get; }

        #region CONSTs
        private const string CATEGORY_DYNAMIC = "Dynamic";
        private const string CATEGORY_MISC = "Misc";
        #endregion
        #region FIELDs
        private static readonly AnotherTarget _anotherTarget;
        private static readonly VBLibrary _vbTarget;
        private static readonly GenStr _genStr;
        private static readonly MyPoint _point;
        private static readonly NotEmptyStringEnumerator _strEnumerator;
        private static readonly Eventer _eventer;
        #endregion

        /************************************************************************/

        static CommonSourceData()
        {
            Target = new InjectTarget();
            _anotherTarget = new AnotherTarget();
            _vbTarget = new VBLibrary();

            _genStr = new GenStr("");
            _point = new MyPoint();
            _eventer = new Eventer();
            _strEnumerator = new NotEmptyStringEnumerator(null);
        }

        /************************************************************************/

        internal static IEnumerable Simple
        {
            get 
            {
                #region If/Else
                yield return GetCase(GetInfo(Target.IfElse_Half), new object[] { false }, new List<string>());
                yield return GetCase(GetInfo(Target.IfElse_Half), new object[] { true }, new List<string> { "If_6" });

                yield return GetCase(GetInfo(Target.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_7", "Anchor_10" });
                yield return GetCase(GetInfo(Target.IfElse_FullSimple), new object[] { true }, new List<string> { "If_4", "Anchor_10" });

                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_20", "Anchor_32", "Else_47" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { false, true }, new List<string> { "Else_20", "Anchor_32", "If_35" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_8", "Anchor_32", "Else_47" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_8", "Anchor_32", "If_35" });

                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_19", "Anchor_25" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_13", "Anchor_25" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_4", "Else_19", "Anchor_25" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_4", "If_13", "Anchor_25" });

                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string> { "Else_17", "Anchor_26" });
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_6", "Anchor_26" });
#if !NETFRAMEWORK
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string> { "Else_19", "Anchor_30" });
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_6", "Anchor_30" });
#endif
                yield return GetCase(GetInfo(Target.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Else_2", "Anchor_6" });
                yield return GetCase(GetInfo(Target.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "If_4", "Anchor_6" });

                yield return GetCase(GetInfo(Target.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Else_4", "Anchor_6" });
                yield return GetCase(GetInfo(Target.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "If_2", "Anchor_6" });

                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_22", "Else_33" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_22", "If_27" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_4", "Else_15" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_4", "If_9" });

                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_4", "Else_15" });
                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_4", "If_9" });

                yield return GetCase(GetInfo(Target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_4" });
                yield return GetCase(GetInfo(Target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_4", "If_9" });

                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_16" });
                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "If_4" });
                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "If_4", "If_9" });
                #endregion
                #region Switch
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { 0 }, new List<string> { "If_12", "Anchor_22" });
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { 1 }, new List<string> { "If_15", "Anchor_22" });
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { -1 }, new List<string> { "If_18", "Anchor_22" });

                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 0 }, new List<string> { "If_8", "Anchor_21" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 1 }, new List<string> { "If_11", "Anchor_21" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 2 }, new List<string> { "If_14", "Anchor_21" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { -1 }, new List<string> { "Switch_7", "Anchor_18", "Anchor_21" });

                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { -1 }, new List<string> { "If_11" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 0 }, new List<string> { "If_19", "Anchor_29" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 1 }, new List<string> { "If_22", "Anchor_29" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 2 }, new List<string> { "Switch_10", "Anchor_26", "Anchor_29" }); //default of switch statement

                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { -1 }, new List<string> { "If_11" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 0 }, new List<string> { "If_19", "Anchor_26" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 1 }, new List<string> { "If_22", "Anchor_26" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 2 }, new List<string> { "Switch_10", "Anchor_26" }); //default of switch statement

                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { -1 }, new List<string> { "If_9" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 0 }, new List<string> { "If_17" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 1 }, new List<string> { "If_25" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 2 }, new List<string> { "Switch_8", "Anchor_34" }); //place of Switch statement
#if !NETFRAMEWORK
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "English", "morning" }, new List<string> { "If_12", "If_30", "Anchor_46" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "English", "evening" }, new List<string> { "If_12", "Else_16", "If_33", "Anchor_46" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "German", "morning" }, new List<string> { "Else_7", "If_21", "If_36", "Anchor_46" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "German", "evening" }, new List<string> { "Else_7", "If_21", "If_39", "Anchor_46" });

                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { -1 }, new List<string> { "If_15", "Anchor_28" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 0 }, new List<string> { "If_18", "Anchor_28" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 1 }, new List<string> { "If_21", "Anchor_28" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 2 }, new List<string> { "Switch_14", "Anchor_25", "Anchor_28" });
#endif
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { 0 }, new List<string> { "If_16", "If_21", "Anchor_47" });
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { 1 }, new List<string> { "If_16", "Else_26", "If_32", "Anchor_47" });
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { -1 }, new List<string> { "If_11", "Anchor_47" });

                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { null }, new List<string> { "Else_25", "Anchor_30", "If_35", "If_57", "Call_60", "Anchor_81" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { false }, new List<string> { "If_3", "Else_15", "Anchor_30", "If_35", "Else_42", "If_51", "Call_53", "If_64", "Anchor_81" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { true }, new List<string> { "If_3", "If_20", "Anchor_30", "If_35", "Else_42", "Else_46", "If_67", "Call_70", "Anchor_81" });

                //C#9
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { -5 }, new List<string> { "Else_6", "If_14", "Anchor_33" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 5 }, new List<string> { "Else_6", "If_17", "Anchor_33" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 10 }, new List<string> { "If_10", "If_22", "Anchor_33" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 100 }, new List<string> { "If_10", "If_27", "Anchor_33" });

                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { -5 }, new List<string> { "If_10", "Anchor_20" });
                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { 5 }, new List<string> { "Else_6", "If_13", "Anchor_20" });
                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { 10 }, new List<string> { "Else_6", "If_16", "Anchor_20" });
                #endregion
                #region Elvis
                yield return GetCase(GetInfo(Target.Elvis_Null), Array.Empty<object>(), new List<string> { "Else_4", "Anchor_9" });
                yield return GetCase(GetInfo(Target.Elvis_Sequence_Null), Array.Empty<object>(), new List<string> { "Else_4", "Anchor_20" });
                yield return GetCase(GetInfo(Target.Elvis_Double_Null), Array.Empty<object>(), new List<string> { "Else_5" });
                #endregion
                #region Linq
                yield return GetCase(GetInfo(Target.Linq_Query), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(Target.Linq_Query), new object[] { true }, new List<string> { "If_6", "If_6", "If_6" });

                yield return GetCase(GetInfo(Target.Linq_Fluent), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(Target.Linq_Fluent), new object[] { true }, new List<string> { "If_6", "If_6", "If_6" });
                
                yield return GetCase(GetInfo(Target.Linq_Fluent_Double), new object[] { false }, new List<string> { "Else_2", "Else_2", "Else_2" });
                yield return GetCase(GetInfo(Target.Linq_Fluent_Double), new object[] { true }, new List<string> { "If_6", "If_6", "If_6" });
                #endregion
                #region Lambda
                yield return GetCase(GetInfo(Target.Lambda), new object[] { 5 }, new List<string> { "Call_13", "If_6" });
                yield return GetCase(GetInfo(Target.Lambda), new object[] { 10 }, new List<string> { "Call_13", "Else_2" });

                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "Call_13", "If_6" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Call_13", "Else_2" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Call_13", "Else_2", "If_20" });

                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "Call_13", "If_6" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Call_13", "Else_2", "If_26" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Call_13", "Else_2", "If_29" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(GetInfo(Target.Try_Catch), new object[] { false }, new List<string> { "Throw_5", "Else_9", "Anchor_13" });
                yield return GetCase(GetInfo(Target.Try_Catch), new object[] { true }, new List<string> { "Throw_5", "If_11", "Anchor_13" });

                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_5", "CatchFilter_12" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_5", "CatchFilter_12", "Else_16", "Anchor_20" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_5", "CatchFilter_12" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_5", "CatchFilter_12", "If_18", "Anchor_20" });

                yield return GetCase(GetInfo(Target.Try_Finally), new object[] { false }, new List<string> { "Else_10", "Anchor_14" });
                yield return GetCase(GetInfo(Target.Try_Finally), new object[] { true }, new List<string> { "If_12", "Anchor_14" });
                
                yield return GetCase(GetInfo(Target.Try_WithCondition), new object[] { false }, new List<string> { "Else_5", "Throw_8" });
                yield return GetCase(GetInfo(Target.Try_WithCondition), new object[] { true }, new List<string> { "If_9" });
                #endregion
                #region Dynamic
                yield return GetCase(GetInfo(Target.ExpandoObject), new object[] { false }, new List<string> { "Call_40", "Call_72", "Else_2", "Anchor_6" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(Target.ExpandoObject), new object[] { true }, new List<string> { "Call_40", "Call_72", "If_4", "Anchor_6" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(GetInfo(Target.Cycle_Do), Array.Empty<object>(), new List<string> { "Cycle_21", "Cycle_21", "CycleEnd_21" }); //IDs may match

                yield return GetCase(GetInfo(Target.Cycle_For), new object[] { -1 }, new List<string> { "Anchor_15", "CycleEnd_20" });
                yield return GetCase(GetInfo(Target.Cycle_For), new object[] { 3 }, new List<string> { "Anchor_15", "Cycle_20", "Anchor_15", "Cycle_20", "Anchor_15", "Cycle_20", "Anchor_15", "CycleEnd_20" });

                yield return GetCase(GetInfo(Target.Cycle_For_Break), new object[] { 2 }, new List<string> { "Anchor_24", "If_13", "Cycle_29", "Anchor_24", "If_13", "Cycle_29", "Anchor_24" });
                yield return GetCase(GetInfo(Target.Cycle_For_Break), new object[] { 3 }, new List<string> { "Anchor_24", "If_13", "Cycle_29", "Anchor_24", "If_13", "Cycle_29", "Anchor_24", "If_13", "Cycle_29", "Anchor_24", "CycleEnd_29" });

                yield return GetCase(GetInfo(Target.Cycle_Foreach), Array.Empty<object>(), new List<string> { "Anchor_36", "Cycle_40", "Anchor_36", "Cycle_40", "Anchor_36", "Cycle_40", "Anchor_36", "CycleEnd_40" });

                yield return GetCase(GetInfo(Target.Cycle_While), new object[] { -1 }, new List<string> { "Anchor_13", "CycleEnd_18" });
                yield return GetCase(GetInfo(Target.Cycle_While), new object[] { 3 }, new List<string> { "Anchor_13", "Cycle_18", "Anchor_13", "Cycle_18", "Anchor_13", "Cycle_18", "Anchor_13", "CycleEnd_18" });
                #endregion
                #region Misc
                yield return GetCase(GetInfo(Target.Goto_Statement), new object[] { false }, new List<string> { "If_7" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Goto_Statement), new object[] { true }, new List<string>()).SetCategory(CATEGORY_MISC);
                
                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Backward), Array.Empty<object>(), new List<string> { "Else_20", "Anchor_21", "Else_20", "Anchor_21", "If_19", "Anchor_24" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Forward), new object[] { false }, new List<string> { "Anchor_26", "If_16", "Cycle_31", "Anchor_26", "If_16", "Cycle_31", "Anchor_26", "CycleEnd_31" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Forward), new object[] { true }, new List<string> { "Anchor_26" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.Lock_Statement), new object[] { false }, new List<string> { "Else_12", "Anchor_16" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Lock_Statement), new object[] { true }, new List<string> { "If_14", "Anchor_16" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.WinAPI), new object[] { false }, new List<string> { "Else_3", "Call_7" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.WinAPI), new object[] { true }, new List<string> { "If_5", "Call_7" }).SetCategory(CATEGORY_MISC);

#if NETFRAMEWORK
                yield return GetCase(GetInfo(Target.ContextBound), new object[] { false }, new List<string> { "Else_5" });
                yield return GetCase(GetInfo(Target.ContextBound), new object[] { true }, new List<string> { "If_9" });
#endif
                #endregion
            }
        }

        internal static IEnumerable Parented
        {
            get
            {
                #region Elvis
                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_NotNull),  new List<string> { "If_7", "Call_9", "Anchor_10" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>())
                    );

                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_Sequence_NotNull),  new List<string> { "If_9", "Call_11", "If_18", "Anchor_21" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>())
                    );

                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_Double_NotNull), new List<string>())
                    );
                #endregion
                #region Generics
                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Generics_Call_Base), new List<string> { "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_6", "Anchor_12" })
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Generics_Call_Base), new List<string> { "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "If_8", "Anchor_12" })
                    );

                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Generics_Call_Child), new List<string> { "Else_5", "Anchor_10" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Generics_Call_Child), new List<string> { "If_7", "Call_9", "Anchor_10" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(_genStr.GetShortDesc), new List<string> { "Call_3" }), 
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Else_6", "Anchor_12" })
                    );

                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Generics_Var), new List<string> { "Else_32", "Anchor_38" })
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Generics_Var), new List<string> { "If_18", "If_26", "Anchor_38" })
                    );
                #endregion
                #region Anonymous
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func), new List<string> { "Call_13", "If_6" }));

                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func_Invoke), new List<string> { "Call_13", "If_6" }));

                //at the moment, we decided not to consider local functions as separate entities 
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func_WithLocalFunc), new List<string> { "Call_7", "If_6" }));

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(Target.Anonymous_Type), new List<string> { "Else_3", "Anchor_7", "Call_14" }));
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(Target.Anonymous_Type), new List<string> { "If_5", "Anchor_7", "Call_14" }));
                #endregion
                #region Async/await
                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(Target.Async_Lambda), new List<string> { "Call_3", "Call_3", "Else_57" }));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(Target.Async_Lambda), new List<string> { "Call_3", "Call_3", "If_18" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Async_Task), new List<string> { "Call_3", "Else_56" }), new TestInfo(GetInfo(Target.Delay100), new List<string>()));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Async_Task), new List<string> { "Call_3", "If_17" }));
#if !NETFRAMEWORK
                #region Async_Linq_Blocking
                yield return GetCase(new object[] { false }, true, 
                    new TestInfo(GetInfo(Target.Async_Linq_Blocking), new List<string> { "Call_10", "Call_15", "Call_5", "Call_15", "Call_5", "Call_15", "Call_5" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2", "Call_2", "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.ProcessElement), new List<string>()));

                yield return GetCase(new object[] { true }, true, 
                    new TestInfo(GetInfo(Target.Async_Linq_Blocking), new List<string> { "Call_10", "Call_15", "Call_5", "Call_15", "Call_5", "Call_15", "Call_5" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2", "Call_2", "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.ProcessElement), new List<string> { "If_5", "Call_9", "Call_12", "If_5", "Call_9", "Call_12", "If_5", "Call_9", "Call_12" }));
                #endregion
                #region Async_Linq_NonBlocking
                yield return GetCase(new object[] { false }, true, true,
                    new TestInfo(GetInfo(Target.Async_Linq_NonBlocking), new List<string> { "Call_24", "Call_3", "Call_5", "Call_5", "Call_5" }, true),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2", "Call_2", "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.ProcessElement), new List<string>())); //.Ignore(TestConstants.INFLUENCE);

                yield return GetCase(new object[] { true }, true, true, 
                    new TestInfo(GetInfo(Target.Async_Linq_NonBlocking), new List<string> { "Call_24", "Call_3", "Call_5", "Call_5", "Call_5" }, true),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.GenStr::.ctor(System.String)"), false, new List<string> { "Call_2", "Call_2", "Call_2" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::set_Prop(T)"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string>()),
                    //new TestInfo(GetSourceFromFullSig(Target, "T Drill4Net.Target.Common.AbstractGen`1::get_Prop()"), false, new List<string>()),
                    new TestInfo(GetInfo(Target.ProcessElement), new List<string> { "If_5", "Call_9", "Call_12", "If_5", "Call_9", "Call_12", "If_5", "Call_9", "Call_12" }, true));
                #endregion
#endif
#if !NET461
                yield return GetCase(Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(Target.Async_Stream), new List<string> { "Call_3", "Call_16", "Anchor_64", "Anchor_64", "If_54", "Anchor_64" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceAsync()"), false, 
                        new List<string> { "Else_24", "If_34", "Else_24", "Else_24", "If_34" }, true));

                yield return GetCase(Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(Target.Async_Stream_Cancellation), new List<string> { "Call_3", "Call_22", "Anchor_92", "Anchor_92", "If_76", "Anchor_92" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceWithCancellationAsync(System.Threading.CancellationToken)"), false, 
                        new List<string> { "Else_24", "Else_24", "Else_24" }, true));
#endif
                #endregion
                #region Parallel
                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Parallel_Linq), new List<string> { "Call_8", "Else_13", "Else_13", "Else_13", "Else_13", "Else_13" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Parallel_Linq), new List<string> { "Call_8", "If_2", "If_2", "If_2", "If_2", "If_2", "If_5", "If_5", "If_5", "If_5", "If_5" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Parallel_For), new List<string> { "Call_8", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Else_14", "Else_14", "Else_14", "Else_14", "Else_14", "If_18", "If_18", "If_18", "If_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Parallel_For), new List<string> { "Call_8", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "If_18", "If_18", "If_18", "If_3", "If_3", "If_3", "If_3", "If_3", "If_6", "If_6", "If_6", "If_6", "If_6" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Parallel_Foreach), new List<string> { "Call_8", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Else_14", "Else_14", "Else_14", "Else_14", "Else_14", "If_18", "If_18", "If_18", "If_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Parallel_Foreach), new List<string> { "Call_8", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "Anchor_16", "If_18", "If_18", "If_18", "If_3", "If_3", "If_3", "If_3", "If_3", "If_6", "If_6", "If_6", "If_6", "If_6" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string>())
                    );

                //data migrates from one func to another depending on running other similar tests... See next option for execute them
                //yield return GetCase(new object[] { false }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "Else_11"}), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "Else_4" }));
                //yield return GetCase(new object[] { true }, new TestData(GetInfo(_target.TaskNewWait), new List<string> { "If_3" }), new TestData(GetInfo(_target.GetStringListForTaskNewWait), new List<string> { "If_12" }));

                yield return GetCase(new object[] { false }, false, true, true, 
                    new TestInfo(GetInfo(Target.Parallel_Task_New), new List<string> { "Call_5", "Else_9", "Anchor_15" }, true),
                    new TestInfo(GetInfo(Target.GetStringListForTaskNew), new List<string> { "Else_2", "Anchor_14" }, true));

                yield return GetCase(new object[] { true }, false, true, true, 
                    new TestInfo(GetInfo(Target.Parallel_Task_New), new List<string> { "Call_5", "If_3", "Anchor_15" }, true),
                    new TestInfo(GetInfo(Target.GetStringListForTaskNew), new List<string> { "If_8", "Anchor_14" }, true));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Parallel_Thread_New), new List<string> { "Call_9" }), new TestInfo(GetInfo(Target.GetStringListForThreadNew), new List<string> { "Else_2", "Anchor_14" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Parallel_Thread_New), new List<string> { "Call_9" }), new TestInfo(GetInfo(Target.GetStringListForThreadNew), new List<string> { "If_8", "Anchor_14" }));
                #endregion
                #region Dynamic
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.DynamicObject), new List<string> { "Call_40", "Call_72", "Else_2", "Anchor_6" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.DynamicDictionary::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.DynamicObject), new List<string> { "Call_40", "Call_72", "If_4", "Anchor_6" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.DynamicDictionary::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string>()));
                #endregion
                #region Disposable
                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Disposable_Using_SyncRead), new List<string> { "Call_8" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" })
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Disposable_Using_SyncRead), new List<string> { "Call_8", "If_15" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" })
                    );

                yield return GetCase(new object[] { false }, true, true, 
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncRead), new List<string> { "Call_3", "Call_21" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" })
                    );

                yield return GetCase(new object[] { true }, true, true, 
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncRead), new List<string> { "Call_3", "Call_21", "If_34" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" })
                    );

                yield return GetCase(new object[] { false }, true, true, 
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncTask), new List<string> { "Call_3", "Call_21" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" })
                    );

                yield return GetCase(new object[] { true }, true, true, 
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncTask), new List<string> { "Call_3", "Call_21", "If_34", "Call_37" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false, new List<string> { "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "Cycle_21", "Anchor_16", "CycleEnd_21" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Threading.Tasks.Task Drill4Net.Target.Common.InjectTarget::AsyncWait()"), false, new List<string>())
                    );

                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Disposable_Using_Last_Exception), new List<string> { "Throw_9" })); //in net50 was "Throw_10"

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Disposable_Using_Exception), new List<string> { "Else_18" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Disposable_Using_Exception), new List<string> { "If_14", "Throw_17" })); //in net50 was "If_15", "Throw_20"

                //class::Finalize() is the thing-in-itself
                yield return GetCase(new object[] { (ushort)17 }, true,
                        new TestInfo(GetInfo(Target.Disposable_Finalizer), new List<string> {"Call_3" }),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.InjectTarget::CreateDisposable(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::.ctor(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "Anchor_12", "If_8", "If_24" }, true)); //in net5 was "If_31", "If_8"

                yield return GetCase(new object[] { (ushort)18 }, true,
                        new TestInfo(GetInfo(Target.Disposable_Finalizer), new List<string> { "Call_3" }),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.InjectTarget::CreateDisposable(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::.ctor(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true, new List<string> { "Anchor_12", "Else_10", "If_24" }, true)); //.Ignore(TestConstants.INFLUENCE);
                #endregion
                #region VB.NET
                yield return GetCase(new object[] { false }, true, 
                    new TestInfo(GetInfo(Target.Try_Catch_VB), new List<string> { "Call_5" }), 
                    new TestInfo(GetInfo(_vbTarget.Try_Catch_VB), new List<string> { "Throw_5", "Else_9", "Anchor_13" })
                    //new TestInfo(GetSourceFromFullSig(Target, "Drill4Net.Target.Common.VB.dll;System.Void Drill4Net.Target.Common.VB.VBLibrary::.ctor()"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, true, 
                    new TestInfo(GetInfo(Target.Try_Catch_VB), new List<string> { "Call_5" }), 
                    new TestInfo(GetInfo(_vbTarget.Try_Catch_VB), new List<string> { "Throw_5", "If_11", "Anchor_13" })
                    //new TestInfo(GetSourceFromFullSig(Target, "Drill4Net.Target.Common.VB.dll;System.Void Drill4Net.Target.Common.VB.VBLibrary::.ctor()"), false, new List<string>())
                    );

                yield return GetCase(new object[] { false }, 
                    new TestInfo(GetInfo(Target.Try_Finally_VB), new List<string> { "Call_5" }), 
                    new TestInfo(GetInfo(_vbTarget.Try_Finally_VB), new List<string> { "Else_9", "Anchor_13" })
                    //new TestInfo(GetSourceFromFullSig(Target, "Drill4Net.Target.Common.VB.dll;System.Void Drill4Net.Target.Common.VB.VBLibrary::.ctor()"), false, new List<string>())
                    );

                yield return GetCase(new object[] { true }, 
                    new TestInfo(GetInfo(Target.Try_Finally_VB), new List<string> { "Call_5" }), 
                    new TestInfo(GetInfo(_vbTarget.Try_Finally_VB), new List<string> { "If_11", "Anchor_13" })
                    //new TestInfo(GetSourceFromFullSig(Target, "Drill4Net.Target.Common.VB.dll;System.Void Drill4Net.Target.Common.VB.VBLibrary::.ctor()"), false, new List<string>())
                    );
                #endregion
                #region Misc
                yield return GetCase(Array.Empty<object>(), false, 
                    new TestInfo(GetInfo(Target.CallAnotherTarget), new List<string> { "Call_2" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "Drill4Net.Target.Common.Another.dll;System.Void Drill4Net.Target.Common.Another.AnotherTarget::.ctor()"), false, new List<string>()),
                    new TestInfo(GetInfo(_anotherTarget.WhoAreU), new List<string>())
                    ).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(Target.Yield), new List<string> { "Call_3" }), new TestInfo(GetInfo(Target.GetForYield), new List<string> { "Anchor_62", "Else_44", "Anchor_49", "If_10", "Anchor_62", "Call_67" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(Target.Yield), new List<string> { "Call_3" }), new TestInfo(GetInfo(Target.GetForYield), new List<string> { "Anchor_62", "If_46", "Anchor_49", "If_10", "Anchor_62", "Call_67" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Extension), new List<string> { "Call_3" }), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "Else_2", "Anchor_6" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Extension), new List<string> { "Call_3" }), new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "If_4", "Anchor_6" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(Array.Empty<object>(), true, 
                    new TestInfo(GetInfo(Target.Event), new List<string> { "Call_18", "Call_22", "Call_26" }),
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Eventer::.ctor()"), false, new List<string>()),
                    new TestInfo(GetInfo(_eventer.NotifyAbout), new List<string> { "If_6", "Call_8" })
                    ).SetCategory(CATEGORY_MISC);

                yield return GetCase(Array.Empty<object>(), true,
                    new TestInfo(GetInfo(Target.Enumerator_Implementation), new List<string> { "Call_7", "Anchor_17", "Anchor_17",  "Anchor_17",  "Anchor_17", "Anchor_17" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.StringEnumerable::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerator`1<System.String> Drill4Net.Target.Common.StringEnumerable::GetEnumerator()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.NotEmptyStringEnumerator::.ctor(System.String[])"), false, new List<string>()),
                    new TestInfo(GetInfo(_strEnumerator.MoveNext), new List<string> { "Else_17", "Call_20", "Anchor_25", "Else_17", "Call_20", "Anchor_25", "Else_17", "Call_20", "Anchor_25", "Else_17", "Call_20", "Anchor_25", "If_14", "Anchor_25"}),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Int32 Drill4Net.Target.Common.NotEmptyStringEnumerator::GetPosition()"), false, new List<string> { "Anchor_6", "Else_13", "Anchor_25", "Cycle_27", "Anchor_6", "Else_13", "Anchor_25", "CycleEnd_27", "Anchor_6", "Else_13", "Anchor_25", "CycleEnd_27", "Anchor_6", "Else_13", "Anchor_25", "Cycle_27", "Anchor_6", "Else_13", "Anchor_25", "CycleEnd_27", "Anchor_6", "Else_13", "Anchor_25", "Cycle_27", "Anchor_6", "If_23", "Anchor_25", "CycleEnd_27" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.String Drill4Net.Target.Common.NotEmptyStringEnumerator::get_Current()"), false, new List<string> { "Else_4", "Anchor_16", "Else_21", "Else_4", "Anchor_16", "Else_21", "Else_4", "Anchor_16", "Else_21", "Else_4", "Anchor_16", "Else_21" })
                    //new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.NotEmptyStringEnumerator::Dispose()"), false, new List<string>())
                    ).SetCategory(CATEGORY_MISC);

                //we dont't take into account local func as separate entity
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.LocalFunc), new List<string> { "Call_3", "Else_2", "Anchor_6" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.LocalFunc), new List<string> { "Call_3", "If_4", "Anchor_6" })).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Unsafe), new List<string> { "Else_7", "Anchor_11" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Unsafe), new List<string> { "If_9", "Anchor_11" }), new TestInfo(GetInfo(_point.ToString), new List<string>())).SetCategory(CATEGORY_MISC);
                #endregion
            }
        }
    }
}