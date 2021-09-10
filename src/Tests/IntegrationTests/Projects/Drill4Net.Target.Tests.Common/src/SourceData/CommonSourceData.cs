using System;
using System.Collections;
using System.Collections.Generic;
using Drill4Net.Target.Common;
using Drill4Net.Target.Common.Another;
using Drill4Net.Target.Common.VB;
using static Drill4Net.Target.Tests.Common.SourceDataCore;

namespace Drill4Net.Target.Tests.Common
{
    /// <summary>
    /// Common source data for tests
    /// </summary>
    public class CommonSourceData
    {
        public static InjectTarget Target { get; }

        #region CONSTs
        private const string CATEGORY_DYNAMIC = "Dynamic";
        private const string CATEGORY_MISC = "Misc";
        //private const string IGNORE_REASON = "Due complexiti with multi-threaded";
        #endregion
        #region FIELDs
        private static readonly AnotherTarget _anotherTarget;
        private static readonly VBLibrary _vbTarget;
        private static readonly GenStr _genStr;
        private static readonly MyPoint _point;
        private static readonly NotEmptyStringEnumerator _strEnumerator;
        private static readonly Eventer _eventer;
        #endregion

        /***********************************************************************************/

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

        /***********************************************************************************/

        /// <summary>
        /// Data source for input data of Target tests which affect only one method
        /// </summary>
        internal static IEnumerable Simple
        {
            get
            {
                #region If/Else
                yield return GetCase(GetInfo(Target.IfElse_Half), new object[] { false }, new List<string> { "Branch_6", "Anchor_9" });
                yield return GetCase(GetInfo(Target.IfElse_Half), new object[] { true }, new List<string> { "Branch_6", "If_6", "Anchor_9" });

                yield return GetCase(GetInfo(Target.IfElse_FullSimple), new object[] { false }, new List<string> { "Branch_4", "Else_7", "Anchor_10" });
                yield return GetCase(GetInfo(Target.IfElse_FullSimple), new object[] { true }, new List<string> { "Branch_4", "If_4", "Branch_7" });

                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Branch_8", "Else_20", "Anchor_32", "Branch_35", "Else_47" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { false, true }, new List<string> { "Branch_8", "Else_20", "Anchor_32", "Branch_35", "If_35", "Branch_47" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "Branch_8", "If_8", "Branch_20", "Branch_35", "Else_47" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "Branch_8", "If_8", "Branch_20", "Branch_35", "If_35", "Branch_47" });

                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Branch_4", "Branch_13", "Else_19", "Anchor_25", "Branch_27" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "Branch_4", "Branch_13", "If_13", "Branch_19", "Branch_27" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "Branch_4", "If_4", "Branch_13", "Else_19", "Anchor_25", "Branch_27" });
                yield return GetCase(GetInfo(Target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "Branch_4", "If_4", "Branch_13", "If_13", "Branch_19", "Branch_27" });

                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Branch_4", "Else_16" });
                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "Branch_4", "If_4", "Branch_9", "Branch_16" });
                yield return GetCase(GetInfo(Target.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "Branch_4", "If_4", "Branch_9", "If_9", "Branch_16" });

                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Branch_4", "Else_22", "Branch_27", "Else_33", "Anchor_39" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Branch_4", "Else_22", "Branch_27", "If_27", "Branch_33" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "Branch_4", "If_4", "Branch_9", "Else_15", "Anchor_21", "Branch_22" });
                yield return GetCase(GetInfo(Target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "Branch_4", "If_4", "Branch_9", "If_9", "Branch_15", "Branch_22" });

                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string> { "Branch_6", "Else_17", "Branch_25" });
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "Branch_6", "If_6", "Branch_17" });
#if !NETFRAMEWORK
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string> { "Branch_6", "Else_19", "Branch_29" });
                yield return GetCase(GetInfo(Target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "Branch_6", "If_6", "Branch_19" });
#endif
                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string> { "Branch_4" });
                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "Branch_4", "If_4", "Branch_9", "Else_15", "Anchor_21" });
                yield return GetCase(GetInfo(Target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "Branch_4", "If_4", "Branch_9", "If_9", "Branch_15" });

                yield return GetCase(GetInfo(Target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "Branch_4", "If_4", "Branch_9" });
                yield return GetCase(GetInfo(Target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "Branch_4", "If_4", "Branch_9", "If_9", "Anchor_15" });

                yield return GetCase(GetInfo(Target.IfElse_Ternary_Negative), new object[] { false }, new List<string> { "Branch_2", "Else_4", "Anchor_6" });
                yield return GetCase(GetInfo(Target.IfElse_Ternary_Negative), new object[] { true }, new List<string> { "Branch_2", "If_2", "Branch_4" });

                yield return GetCase(GetInfo(Target.IfElse_Ternary_Positive), new object[] { false }, new List<string> { "Branch_2", "Else_2", "Branch_4" });
                yield return GetCase(GetInfo(Target.IfElse_Ternary_Positive), new object[] { true }, new List<string> { "Branch_2", "If_4", "Anchor_6" });
                #endregion
                #region Switch
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { 0 }, new List<string> { "Branch_4", "Branch_7", "If_12", "Branch_15", "Branch_23" });
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { 1 }, new List<string> { "Branch_4", "Branch_7", "Branch_8", "Branch_11", "If_15", "Branch_18", "Branch_23" });
                yield return GetCase(GetInfo(Target.Switch_TwoCases_Into_IfElse), new object[] { -1 }, new List<string> { "Branch_4", "Branch_7", "Branch_8", "Branch_11", "Branch_12", "Branch_21", "Branch_23" });

                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 0 }, new List<string> { "Branch_4", "Branch_7", "Branch_11", "Branch_22" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 1 }, new List<string> { "Branch_4", "Branch_7", "Branch_14", "Branch_22" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { 2 }, new List<string> { "Branch_4", "Branch_7", "Branch_17", "Branch_22" });
                yield return GetCase(GetInfo(Target.Switch_ThreeCases_Into_Switch), new object[] { -1 }, new List<string> { "Branch_4", "Branch_7", "Switch_7", "Branch_8", "Branch_20", "Branch_22" });

                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { -1 }, new List<string> { "Branch_10", "Branch_19" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 0 }, new List<string> { "Branch_10", "Branch_22" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 1 }, new List<string> { "Branch_10", "Branch_25" });
                yield return GetCase(GetInfo(Target.Switch_ExplicitDefault), new object[] { 2 }, new List<string> { "Branch_10", "Switch_10", "Branch_11", "Branch_28" }); //default of switch statement

                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { -1 }, new List<string> { "Branch_10", "Branch_19" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 0 }, new List<string> { "Branch_10", "Branch_22" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 1 }, new List<string> { "Branch_10", "Branch_25" });
                yield return GetCase(GetInfo(Target.Switch_ImplicitDefault), new object[] { 2 }, new List<string> { "Branch_10", "Switch_10", "Branch_11" }); //default of switch statement

                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { -1 }, new List<string> { "Branch_8", "Branch_17" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 0 }, new List<string> { "Branch_8", "Branch_25" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 1 }, new List<string> { "Branch_8", "Branch_33" });
                yield return GetCase(GetInfo(Target.Switch_WithoutDefault), new object[] { 2 }, new List<string> { "Branch_8", "Switch_8", "Branch_9" }); //place of Switch statement
#if !NETFRAMEWORK
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "English", "morning" }, new List<string> { "Branch_2", "Branch_7", "If_12", "Branch_16", "If_30", "Branch_33", "Branch_47", "Branch_58" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "English", "evening" }, new List<string> { "Branch_2", "Branch_7", "If_12", "Branch_16", "Else_16", "Branch_20", "If_33", "Branch_36", "Branch_47", "Branch_58" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "German", "morning" }, new List<string> { "Branch_2", "Branch_7", "Else_7", "Branch_11", "If_21", "Branch_25", "If_36", "Branch_39", "Branch_47", "Branch_58" });
                yield return GetCase(GetInfo(Target.Switch_Tuple), new object[] { "German", "evening" }, new List<string> { "Branch_2", "Branch_7", "Else_7", "Branch_11", "If_21", "Branch_25", "Else_25", "Branch_29", "If_39", "Branch_42", "Branch_47", "Branch_58" });

                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { -1 }, new List<string> { "Branch_9", "Branch_14", "Branch_18", "Branch_29", "Branch_33" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 0 }, new List<string> { "Branch_9", "Branch_14", "Branch_21", "Branch_29", "Branch_33" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 1 }, new List<string> { "Branch_9", "Branch_14", "Branch_24", "Branch_29", "Branch_33" });
                yield return GetCase(GetInfo(Target.Switch_AsReturn), new object[] { 2 }, new List<string> { "Branch_9", "Branch_14", "Switch_14", "Branch_15", "Branch_27", "Branch_29", "Branch_33" });
#endif
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { 0 }, new List<string> { "Branch_7", "Branch_10", "Branch_11", "Branch_20", "If_21", "Branch_26" });
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { 1 }, new List<string> { "Branch_7", "Branch_10", "Branch_11", "Branch_20", "Branch_21", "Branch_31", "If_32", "Branch_37" });
                yield return GetCase(GetInfo(Target.Switch_When), new object[] { -1 }, new List<string> { "Branch_7", "Branch_10", "If_11", "Branch_16" });

                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { null }, new List<string> { "Branch_3", "Else_25", "Anchor_30", "Branch_32", "Branch_35", "If_35", "Branch_42", "If_57", "Branch_64", "Branch_82", "Branch_95" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { false }, new List<string> { "Branch_3", "If_3", "Branch_15", "Else_15", "Branch_20", "Branch_32", "Branch_35", "If_35", "Branch_42", "Else_42", "Branch_46", "If_51", "Branch_56", "If_64", "Branch_67", "Branch_82", "Branch_95" });
                yield return GetCase(GetInfo((OneNullBoolFuncStr)Target.Switch_Property), new object[] { true }, new List<string> { "Branch_3", "If_3", "Branch_15", "If_20", "Branch_25", "Branch_32", "Branch_35", "If_35", "Branch_42", "Else_42", "Branch_46", "Else_46", "Branch_50", "If_67", "Branch_74", "Branch_82", "Branch_95" });

                //C#9
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { -5 }, new List<string> { "Branch_2", "Branch_6", "Else_6", "Branch_9", "If_14", "Branch_17", "Branch_34", "Branch_47" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 5 }, new List<string> { "Branch_2", "Branch_6", "Else_6", "Branch_9", "Branch_10", "Branch_22", "Branch_34", "Branch_47" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 10 }, new List<string> { "Branch_2", "Branch_6", "If_10", "Branch_13", "If_22", "Branch_27", "Branch_34", "Branch_47" });
                yield return GetCase(GetInfo(Target.Switch_Relational), new object[] { 100 }, new List<string> { "Branch_2", "Branch_6", "If_10", "Branch_13", "Branch_14", "Branch_32", "Branch_34", "Branch_47" });

                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { -5 }, new List<string> { "Branch_2", "Branch_6", "If_10", "Branch_13", "Branch_21", "Branch_32" });
                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { 5 }, new List<string> { "Branch_2", "Branch_6", "Else_6", "Branch_9", "If_13", "Branch_16", "Branch_21", "Branch_32" });
                yield return GetCase(GetInfo(Target.Switch_Logical), new object[] { 10 }, new List<string> { "Branch_2", "Branch_6", "Else_6", "Branch_9", "Branch_10", "Branch_19", "Branch_21", "Branch_32" });
                #endregion
                #region Elvis
                yield return GetCase(GetInfo(Target.Elvis_Null), Array.Empty<object>(), new List<string> { "Branch_4", "Else_4", "Branch_6" });
                yield return GetCase(GetInfo(Target.Elvis_Sequence_Null), Array.Empty<object>(), new List<string> { "Branch_4", "Else_4", "Branch_8" });
                yield return GetCase(GetInfo(Target.Elvis_Double_Null), Array.Empty<object>(), new List<string> { "Branch_5", "Else_5", "Anchor_8" });
                #endregion
                #region Linq
                yield return GetCase(GetInfo(Target.Linq_Query), new object[] { false }, new List<string> { "Call_24", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6" });
                yield return GetCase(GetInfo(Target.Linq_Query), new object[] { true }, new List<string> { "Call_24", "Branch_2", "If_6", "Branch_2", "If_6", "Branch_2", "If_6" });

                yield return GetCase(GetInfo(Target.Linq_Fluent), new object[] { false }, new List<string> { "Call_24", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6" });
                yield return GetCase(GetInfo(Target.Linq_Fluent), new object[] { true }, new List<string> { "Call_24", "Branch_2", "If_6", "Branch_2", "If_6", "Branch_2", "If_6" });
                
                yield return GetCase(GetInfo(Target.Linq_Fluent_Double), new object[] { false }, new List<string> { "Call_24", "Branch_43", "Call_50", "Call_55", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6", "Branch_2", "Else_2", "Branch_6" });
                yield return GetCase(GetInfo(Target.Linq_Fluent_Double), new object[] { true }, new List<string> { "Call_24", "Branch_43", "Call_50", "Call_55", "Branch_2", "If_6", "Branch_2", "If_6", "Branch_2", "If_6" });
                #endregion
                #region Lambda
                yield return GetCase(GetInfo(Target.Lambda), new object[] { 5 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "If_6" });
                yield return GetCase(GetInfo(Target.Lambda), new object[] { 10 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "Else_2", "Branch_6" });

                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 5 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "If_6", "Branch_20" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 10 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "Else_2", "Branch_6", "Branch_20" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalBranch), new object[] { 12 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "Else_2", "Branch_6", "Branch_20", "If_20", "Anchor_25" });

                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 5 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "If_6", "Branch_21", "Branch_22", "Branch_25", "Branch_26" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 10 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "Else_2", "Branch_6", "Branch_21", "If_26", "Branch_29" });
                yield return GetCase(GetInfo(Target.Lambda_AdditionalSwitch), new object[] { 12 }, new List<string> { "Branch_3", "Anchor_10|", "Call_13", "Branch_2", "Else_2", "Branch_6", "Branch_21", "Branch_22", "Branch_25", "If_29", "Branch_32" });
                #endregion
                #region Try/cath/finally
                yield return GetCase(GetInfo(Target.Try_Catch), new object[] { false }, new List<string> { "Throw_5", "Branch_9", "Else_9", "Branch_11" });
                yield return GetCase(GetInfo(Target.Try_Catch), new object[] { true }, new List<string> { "Throw_5", "Branch_9", "If_11", "Anchor_13" });

                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { false, false }, new List<string> { "Throw_5", "CatchFilter_12", "Branch_42" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { false, true }, new List<string> { "Throw_5", "CatchFilter_12", "Branch_16", "Else_16", "Branch_18" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { true, false }, new List<string> { "Throw_5", "CatchFilter_12", "Branch_42" });
                yield return GetCase(GetInfo(Target.Try_CatchWhen), new object[] { true, true }, new List<string> { "Throw_5", "CatchFilter_12", "Branch_16", "If_18", "Anchor_20" });

                yield return GetCase(GetInfo(Target.Try_Finally), new object[] { false }, new List<string> { "Branch_10", "Else_10", "Branch_12" });
                yield return GetCase(GetInfo(Target.Try_Finally), new object[] { true }, new List<string> { "Branch_10", "If_12", "Anchor_14" });
                
                yield return GetCase(GetInfo(Target.Try_WithCondition), new object[] { false }, new List<string> { "Branch_5", "Else_5", "Throw_8", "Branch_29" });
                yield return GetCase(GetInfo(Target.Try_WithCondition), new object[] { true }, new List<string> { "Branch_5", "If_9", "Branch_20" });
                #endregion
                #region Dynamic
                yield return GetCase(GetInfo(Target.ExpandoObject), new object[] { false }, new List<string> { "Branch_4", "Branch_5|Anchor_27", "Branch_33", "Call_40", "Branch_43", "Branch_44|Anchor_67", "Call_72", "Branch_1", "Else_1", "Branch_3" }).SetCategory(CATEGORY_DYNAMIC);
                yield return GetCase(GetInfo(Target.ExpandoObject), new object[] { true }, new List<string> { "Branch_4", "Branch_5|Anchor_27", "Branch_33", "Call_40", "Branch_43", "Branch_44|Anchor_67", "Call_72", "Branch_1", "If_3" }).SetCategory(CATEGORY_DYNAMIC);
                #endregion
                #region Cycle
                yield return GetCase(GetInfo(Target.Cycle_Do), Array.Empty<object>(), new List<string> { "Anchor_10", "Cycle_21", "Branch_21", "Cycle_21", "Branch_21", "Cycle_21", "Branch_21", "CycleEnd_21" }); //local IDs may match

                //Cycle_For
                yield return GetCase(GetInfo(Target.Cycle_For), new object[] { -1 }, new List<string> { "Branch_5", "Branch_20", "CycleEnd_20", "Branch_28" });
                
                yield return GetCase(GetInfo(Target.Cycle_For), new object[] { 3 },
                    new List<string> { "Branch_5",
                                       "Branch_20", "Cycle_20", "Anchor_6", "Anchor_15",
                                       "Branch_20", "Cycle_20", "Anchor_6", "Anchor_15",
                                       "Branch_20", "Cycle_20", "Anchor_6", "Anchor_15",
                                       "Branch_20", "CycleEnd_20", "Branch_28" });

                //Cycle_For_Break
                yield return GetCase(GetInfo(Target.Cycle_For_Break), new object[] { 2 },
                    new List<string> { "Branch_5", 
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12", "If_13", "Anchor_24",
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12", "If_13", "Anchor_24",
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12",
                                       "Branch_13", "Branch_37", "Return_39" });

                yield return GetCase(GetInfo(Target.Cycle_For_Break), new object[] { 3 },
                    new List<string> { "Branch_5", 
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12", "If_13", "Anchor_24",
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12", "If_13", "Anchor_24",
                                       "Branch_29", "Cycle_29", "Anchor_6", "Branch_12", "If_13", "Anchor_24",
                                       "Branch_29", "CycleEnd_29", "Anchor_30", "Branch_37" });

                //Cycle_Foreach
                yield return GetCase(GetInfo(Target.Cycle_Foreach), Array.Empty<object>(),
                    new List<string> { "Branch_23",
                                       "Branch_40", "Cycle_40", "Anchor_24", "Anchor_36",
                                       "Branch_40", "Cycle_40", "Anchor_24", "Anchor_36",
                                       "Branch_40", "Cycle_40", "Anchor_24", "Anchor_36",
                                       "Branch_40", "CycleEnd_40" });

                //Cycle_While
                yield return GetCase(GetInfo(Target.Cycle_While), new object[] { -1 }, new List<string> { "Branch_8", "Branch_18", "CycleEnd_18", "Branch_21" });
               
                yield return GetCase(GetInfo(Target.Cycle_While), new object[] { 3 },
                    new List<string> { "Branch_8",
                                       "Branch_18", "Cycle_18", "Anchor_9", "Anchor_13",
                                       "Branch_18", "Cycle_18", "Anchor_9", "Anchor_13",
                                       "Branch_18", "Cycle_18", "Anchor_9", "Anchor_13",
                                       "Branch_18", "CycleEnd_18", "Branch_21" });
                #endregion
                #region Misc
                yield return GetCase(GetInfo(Target.Goto_Statement), new object[] { false }, new List<string> { "Branch_6", "If_7", "Anchor_10" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Goto_Statement), new object[] { true }, new List<string> { "Branch_6", "Branch_7" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Backward), Array.Empty<object>(), new List<string> { "Anchor_3", "Branch_11", "Branch_23", "Branch_18", "Branch_19", "Branch_11", "Branch_23", "Branch_18", "If_19", "Branch_20" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Forward), new object[] { false }, new List<string> { "Branch_5", "Branch_31", "Cycle_31", "Anchor_6", "Branch_15", "If_16", "Anchor_26", "Branch_31", "Cycle_31", "Anchor_6", "Branch_15", "If_16", "Anchor_26", "Branch_31", "CycleEnd_31", "Anchor_32" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Goto_Statement_Cycle_Forward), new object[] { true }, new List<string> { "Branch_5", "Branch_31", "Cycle_31", "Anchor_6", "Branch_15", "Branch_16" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.Lock_Statement), new object[] { false }, new List<string> { "Branch_12", "Else_12", "Branch_14", "Branch_20", "Anchor_24" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.Lock_Statement), new object[] { true }, new List<string> { "Branch_12", "If_14", "Anchor_16", "Branch_20", "Anchor_24" }).SetCategory(CATEGORY_MISC);

                yield return GetCase(GetInfo(Target.WinAPI), new object[] { false }, new List<string> { "Branch_3", "Else_3", "Branch_5", "Call_7" }).SetCategory(CATEGORY_MISC);
                yield return GetCase(GetInfo(Target.WinAPI), new object[] { true }, new List<string> { "Branch_3", "If_5", "Call_7" }).SetCategory(CATEGORY_MISC);
                #endregion
            }
        }

        /// <summary>
        /// Data source for input data of Target tests which affect several methods
        /// </summary>
        internal static IEnumerable Parented
        {
            get
            {
                #region Elvis
                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_NotNull),  new List<string> { "Branch_5", "If_7", "Call_9", "Anchor_10" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    );

                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_Sequence_NotNull),  new List<string> { "Branch_5", "If_9", "Call_11", "Branch_13", "If_18", "Anchor_21" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    );

                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.Elvis_Double_NotNull), new List<string> { "Branch_5", "Anchor_8" })
                    );
                #endregion
                #region Generics
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Generics_Call_Base), new List<string> { "Call_6" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Branch_6", "Else_6", "Branch_8", "Branch_13" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Generics_Call_Base), new List<string> { "Call_6" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Branch_6", "If_8", "Anchor_12", "Branch_13" })
                    );

                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Generics_Call_Child), new List<string> { "Branch_5", "Else_5", "Branch_7" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Generics_Call_Child), new List<string> { "Branch_5", "If_7", "Call_9", "Anchor_10" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6" }),
                    new TestInfo(GetInfo(_genStr.GetShortDesc), new List<string> { "Call_3", "Branch_5" }),
                    new TestInfo(GetInfo(_genStr.GetDesc), new List<string> { "Branch_6", "Else_6", "Branch_8", "Branch_13" })
                    );

                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Generics_Var), new List<string> { "Branch_18", "Else_32", "Anchor_38" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Generics_Var), new List<string> { "Branch_18", "If_18", "Branch_26", "If_26", "Branch_32" })
                    );
                #endregion
                #region Anonymous
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func), new List<string> { "Call_13", "Branch_6", "If_6", "Branch_18" }));

                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func_Invoke), new List<string> { "Call_13", "Branch_6", "If_6", "Branch_18" }));

                //at the moment, we decided not to consider local functions as separate entities 
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Anonymous_Func_WithLocalFunc), new List<string> { "Call_7", "Branch_6", "If_6", "Branch_18" }));

                yield return GetCase(new object[] { false }, true, new TestInfo(GetInfo(Target.Anonymous_Type), new List<string> { "Branch_3", "Else_3", "Branch_5" }));
                yield return GetCase(new object[] { true }, true, new TestInfo(GetInfo(Target.Anonymous_Type), new List<string> { "Branch_3", "If_5", "Anchor_7" }));
                #endregion
                #region Async/await
                yield return GetCase(new object[] { false }, true, true, new TestInfo(GetInfo(Target.Async_Lambda), new List<string> {"Call_9", "Branch_11", "Call_14", "Branch_18", "Else_57" }));
                yield return GetCase(new object[] { true }, true, true, new TestInfo(GetInfo(Target.Async_Lambda), new List<string> { "Call_9", "Branch_11", "Call_14", "Branch_18", "If_18" }));

                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Async_Task), new List<string> { "Call_17", "Branch_17", "Else_56", "Call_59" }), new TestInfo(GetInfo(Target.Delay100), new List<string> { "Branch_4" }));
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Async_Task), new List<string> { "Call_17", "Branch_17", "If_17" }));
#if !NETFRAMEWORK
                #region Async_Linq_Blocking
                yield return GetCase(new object[] { false }, true,
                    new TestInfo(GetInfo(Target.Async_Linq_Blocking), new List<string> { "Call_10", "Call_16", "Branch_19", "Call_26", "Branch_29", "Call_36", "Call_37", "Call_17", "Call_15", "Call_5", "Call_17", "Call_15", "Call_5", "Call_17", "Call_15", "Call_5" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string> { "Branch_18" }),
                    new TestInfo(GetInfo(Target.ProcessElement),
                        new List<string> { "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24"
                        }, true));

                yield return GetCase(new object[] { true }, true,
                    new TestInfo(GetInfo(Target.Async_Linq_Blocking), new List<string> { "Call_10", "Call_16", "Branch_19", "Call_26", "Branch_29", "Call_36", "Call_37", "Call_17", "Call_15", "Call_5", "Call_17", "Call_15", "Call_5", "Call_17", "Call_15", "Call_5" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string> { "Branch_18" }),
                    new TestInfo(GetInfo(Target.ProcessElement),
                        new List<string> { "Call_20", "Call_25", "Branch_5", "If_5", "Call_9", "Call_12", "Anchor_14", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "If_5", "Call_9", "Call_12", "Anchor_14", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "If_5", "Call_9", "Call_12", "Anchor_14", "Branch_17", "Return_24"
                        }, true));
                #endregion
                #region Async_Linq_NonBlocking
                yield return GetCase(new object[] { false }, true, true,
                    new TestInfo(GetInfo(Target.Async_Linq_NonBlocking), new List<string> { "Call_17", "Call_24", "Call_32", "Call_5", "Call_5", "Call_5", "Call_90", "Branch_93", "Else_93", "Call_100", "Call_101" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string> { "Branch_18" } ),
                    new TestInfo(GetInfo(Target.ProcessElement),
                        new List<string> { "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Branch_17", "Return_24"
                        }, true)
                    ); //.Ignore(TestConstants.INFLUENCE);

                yield return GetCase(new object[] { true }, true, true,
                    new TestInfo(GetInfo(Target.Async_Linq_NonBlocking), new List<string> { "Call_17", "Call_24", "Call_32", "Call_5", "Call_5", "Call_5", "Call_90", "Branch_93", "Else_93", "Call_100", "Call_101" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.AbstractGen`1::.ctor(T)"), false, new List<string> { "Call_6", "Call_6", "Call_6" }),
                    new TestInfo(GetInfo(Target.GetDataForAsyncLinq), new List<string> { "Branch_18" }),
                    new TestInfo(GetInfo(Target.ProcessElement),
                        new List<string> { "Call_20", "Call_25", "Branch_5", "If_5", "Call_9", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Call_12", "Anchor_14", "Branch_17", "If_5", "Return_24",
                                           "Call_20", "Call_25", "Branch_5", "Call_12",  "Anchor_14", "Branch_17", "If_5", "Call_9", "Return_24",
                                           "Call_12", "Call_9", "Anchor_14", "Branch_17"
                        }, true));
                #endregion
#endif
#if !NET461
                //Async_Stream
                yield return GetCase(Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(Target.Async_Stream), new List<string> { "Call_14", "Call_16", "Anchor_36", "Return_18", "Branch_54", "Anchor_64", "Branch_54", "If_54", "Anchor_63", "Anchor_64", "Branch_109", "Branch_117", "If_117", "Branch_127", "Anchor_159", "Branch_163" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceAsync()"), false,
                        new List<string> { "Return_5", "Branch_13", "Branch_24", "Branch_111", "Branch_34", "If_34", "Branch_91", "Anchor_94", "Anchor_103", "Branch_111", "Branch_34", "Anchor_103", "Branch_111", "Branch_34", "If_34", "Branch_91", "Anchor_94", "Anchor_103", "Branch_111" }, true));

                //Async_Stream_Cancellation
                yield return GetCase(Array.Empty<object>(), true, true,
                    new TestInfo(GetInfo(Target.Async_Stream_Cancellation), new List<string> { "Call_14", "Call_22", "Anchor_32", "Branch_39", "Anchor_58", "Return_18", "Branch_76", "Anchor_92", "Branch_76", "If_76", "Anchor_91", "Anchor_92", "Branch_145", "If_145", "Branch_155", "Anchor_187", "Branch_191", "If_191", "Branch_196", "If_199" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IAsyncEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GenerateSequenceWithCancellationAsync(System.Threading.CancellationToken)"), false,
                        new List<string> { "Return_8", "Branch_13", "Branch_24", "Branch_104", "Branch_85", "Anchor_96", "Branch_104", "Branch_85", "Anchor_96", "Branch_104", "Branch_112" }, true));
#endif
                #endregion
                #region Parallel
                //Parallel_Linq
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Parallel_Linq), new List<string> { "Call_8", "Call_11", "Call_15", "Call_16", "Branch_2", "Branch_2", "Else_13", "Branch_2", "Branch_2", "Else_13", "Branch_2", "Else_13", "Else_13", "Else_13" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Parallel_Linq), new List<string> { "Call_8", "Call_11", "Call_15", "Call_16", "Branch_2", "Branch_2", "Branch_2", "Branch_2", "If_2", "If_2", "If_2", "If_2", "Branch_5", "Branch_5", "Branch_5", "Branch_5", "If_5", "If_5", "If_5", "If_5", "Branch_11", "Branch_11", "Branch_11", "Branch_11", "Branch_2", "If_2", "Branch_5", "If_5", "Branch_11" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                //Parallel_For
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Parallel_For), new List<string> { "Call_8", "Call_15", "Call_19", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "Else_14", "Else_14", "Else_14", "Else_14", "Anchor_16", "Anchor_16", "Anchor_16", "Else_14", "Anchor_16", "Branch_18", "Branch_18", "Branch_18", "Anchor_16", "Branch_18", "If_18", "If_18", "If_18", "Branch_18", "If_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Parallel_For), new List<string> { "Call_8", "Call_15", "Call_19", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "If_3", "If_3", "If_3", "If_3", "If_3", "Branch_6", "Branch_6", "If_6", "Branch_6", "Branch_6", "If_6", "Branch_6", "Branch_12", "If_6", "If_6", "Branch_12", "Branch_18", "Branch_18", "Branch_12", "Branch_12", "If_6", "If_18", "If_18", "Branch_18", "Branch_18", "Branch_12", "Branch_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                //Parallel_Foreach
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Parallel_Foreach), new List<string> { "Call_8", "Call_17", "Branch_3", "Branch_3", "Else_14", "Branch_3", "Branch_3", "Else_14", "Branch_3", "Anchor_16", "Else_14", "Else_14", "Anchor_16", "Else_14", "Branch_18", "If_18", "Anchor_16", "Branch_18", "Anchor_16", "Anchor_16", "Branch_18", "If_18", "Branch_18", "Branch_18", "If_18", "If_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Parallel_Foreach), new List<string> { "Call_8", "Call_17", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "Branch_3", "If_3", "If_3", "If_3", "If_3", "If_3", "Branch_6", "Branch_6", "Branch_6", "Branch_6", "Branch_6", "If_6", "If_6", "If_6", "If_6", "If_6", "Branch_12", "Branch_12", "Branch_12", "Branch_12", "Branch_12", "Branch_18", "Branch_18", "Branch_18", "Branch_18", "Branch_18", "If_18", "If_18", "If_18" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerable`1<System.Int32> Drill4Net.Target.Common.InjectTarget::GetDataForParallel(System.Int32)"), false, new List<string> { "Call_3", "Branch_5" })
                    );

                //Parallel_Task_New
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Parallel_Task_New), new List<string> { "Call_24", "Call_32", "Call_5", "Branch_3", "Else_9", "Anchor_15" }, true),
                    new TestInfo(GetInfo(Target.GetStringListForTaskNew), new List<string> { "Branch_2", "Else_2", "Branch_8", "Branch_15" }, true));

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Parallel_Task_New), new List<string> { "Call_24", "Call_32", "Call_5", "Branch_3", "If_3", "Branch_9" }, true),
                    new TestInfo(GetInfo(Target.GetStringListForTaskNew), new List<string> { "Branch_2", "If_8", "Anchor_14", "Branch_15" }, true));

                //Parallel_Thread_New
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Parallel_Thread_New), new List<string> { "Call_15", "Call_9" }),
                    new TestInfo(GetInfo(Target.GetStringListForThreadNew), new List<string> { "Branch_2", "Else_2", "Branch_8" }));

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Parallel_Thread_New), new List<string> { "Call_15", "Call_9" }),
                    new TestInfo(GetInfo(Target.GetStringListForThreadNew), new List<string> { "Branch_2", "If_8", "Anchor_14" }));
                #endregion
                #region Dynamic
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.DynamicObject), new List<string> { "Branch_4", "Branch_5|Anchor_27", "Branch_33", "Call_40", "Branch_43", "Branch_44|Anchor_67", "Call_72", "Branch_1", "Else_1", "Branch_3" }), //"Anchor_6|Branch_7",
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.DynamicDictionary::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string> { "Branch_11" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string> { "Branch_11" }));

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.DynamicObject), new List<string> { "Branch_4", "Branch_5|Anchor_27", "Branch_33", "Call_40", "Branch_43", "Branch_44|Anchor_67", "Call_72", "Branch_1", "If_3" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.DynamicDictionary::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TrySetMember(System.Dynamic.SetMemberBinder,System.Object)"), false, new List<string> { "Branch_11" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Boolean Drill4Net.Target.Common.DynamicDictionary::TryGetMember(System.Dynamic.GetMemberBinder,System.Object&)"), false, new List<string> { "Branch_11" }));
                #endregion
                #region Disposable
                //Disposable_Using_SyncRead
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Disposable_Using_SyncRead), new List<string> { "Call_8", "Branch_15", "Branch_27", "Anchor_31" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Disposable_Using_SyncRead), new List<string> { "Call_8", "Branch_15", "If_15", "Anchor_24", "Branch_27", "Anchor_31" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" })
                    );

                //Disposable_Using_AsyncRead
                yield return GetCase(new object[] { false }, true, true,
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncRead), new List<string> { "Call_17", "Call_21", "Anchor_24", "Branch_34", "Branch_85", "Else_85", "Branch_88", "If_88", "Anchor_93" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6", 
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                          "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" })
                    );

                yield return GetCase(new object[] { true }, true, true,
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncRead), new List<string> { "Call_17", "Call_21", "Anchor_24", "Branch_34", "If_34", "Branch_85", "Else_85", "Branch_88", "If_88", "Anchor_93" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6", 
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" })
                    );

                //Disposable_Using_AsyncTask
                yield return GetCase(new object[] { false }, true, true,
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncTask), new List<string> { "Call_17", "Call_21", "Anchor_24", "Branch_34", "Branch_78", "Else_78", "Branch_81", "If_81", "Anchor_86" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6", 
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" })
                    );

                yield return GetCase(new object[] { true }, true, true,
                    new TestInfo(GetInfo(Target.Disposable_Using_AsyncTask), new List<string> { "Call_17", "Call_21", "Anchor_24", "Branch_34", "If_34", "Call_37", "Branch_78", "Branch_78", "Else_78", "Branch_81", "If_81", "Anchor_86" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Byte[] Drill4Net.Target.Common.InjectTarget::GetBytes(System.Byte)"), false,
                        new List<string> { "Branch_6", 
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "Cycle_21", "Anchor_7", "Anchor_16",
                                           "Branch_21", "CycleEnd_21", "Branch_24", "Return_26" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Threading.Tasks.Task Drill4Net.Target.Common.InjectTarget::AsyncWait()"), false,
                        new List<string> { "Branch_3", "Call_10", "Branch_12" })
                    );

                //Disposable_Using_Last_Exception
                yield return GetCase(Array.Empty<object>(), new TestInfo(GetInfo(Target.Disposable_Using_Last_Exception), new List<string> { "Throw_9", "Branch_11" }));

                //Disposable_Using_Exception
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.Disposable_Using_Exception),
                    new List<string> { "Branch_14", "Else_18", "Branch_21", "Anchor_25" }));

                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.Disposable_Using_Exception),
                    new List<string> { "Branch_14", "If_14", "Throw_17", "Branch_21", "Anchor_25" }));

                //Disposable_Finalizer
                //class::Finalize() is the thing-in-itself
                yield return GetCase(new object[] { (ushort)17 }, true,
                        new TestInfo(GetInfo(Target.Disposable_Finalizer), new List<string> { "Call_3" }),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.InjectTarget::CreateDisposable(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::.ctor(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true,
                            new List<string> { "Branch_8", "If_8", "Branch_10", "Branch_22", "If_24" }, true));

                yield return GetCase(new object[] { (ushort)18 }, true,
                        new TestInfo(GetInfo(Target.Disposable_Finalizer), new List<string> { "Call_3" }),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.InjectTarget::CreateDisposable(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::.ctor(System.Int32)"), false, new List<string>()),
                        new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.Finalizer::Finalize()"), true,
                            new List<string> { "Branch_8", "Else_10", "Anchor_12", "Branch_22", "If_24" }, true)); //.Ignore(TestConstants.INFLUENCE);
                #endregion
                #region VB.NET
                yield return GetCase(new object[] { false }, true,
                    new TestInfo(GetInfo(Target.Try_Catch_VB), new List<string> { "Call_5" }),
                    new TestInfo(GetInfo(_vbTarget.VB_Try_Catch), new List<string> { "Throw_5", "Branch_9", "Else_9", "Branch_11" })
                    );

                yield return GetCase(new object[] { true }, true,
                    new TestInfo(GetInfo(Target.Try_Catch_VB), new List<string> { "Call_5" }),
                    new TestInfo(GetInfo(_vbTarget.VB_Try_Catch), new List<string> { "Throw_5", "Branch_9", "If_11", "Anchor_13" })
                    );

                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Try_Finally_VB), new List<string> { "Call_5" }),
                    new TestInfo(GetInfo(_vbTarget.VB_Try_Finally), new List<string> { "Branch_9", "Else_9", "Branch_11" })
                    );

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Try_Finally_VB), new List<string> { "Call_5" }),
                    new TestInfo(GetInfo(_vbTarget.VB_Try_Finally), new List<string> { "Branch_9", "If_11", "Anchor_13" })
                    );
                #endregion
                #region Misc
                //CallAnotherTarget
                yield return GetCase(Array.Empty<object>(), false,
                    new TestInfo(GetInfo(Target.CallAnotherTarget), new List<string> { "Call_2" }),
                    new TestInfo(GetInfo(_anotherTarget.WhoAreU), new List<string> { "Branch_3" })
                    ).SetCategory(CATEGORY_MISC);

                //Yield
                yield return GetCase(new object[] { false }, true,
                    new TestInfo(GetInfo(Target.Yield), new List<string> { "Call_3" }),
                    new TestInfo(GetInfo(Target.GetForYield), new List<string> { "Branch_35", "Branch_65", "Branch_44", "Else_44", "Branch_46", "Anchor_62", "Branch_65", "Call_67" })
                    ).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { true }, true,
                    new TestInfo(GetInfo(Target.Yield), new List<string> { "Call_3" }),
                    new TestInfo(GetInfo(Target.GetForYield), new List<string> { "Branch_35", "Branch_65", "Branch_44", "If_46", "Anchor_49", "Anchor_62", "Branch_65", "Call_67" })
                    ).SetCategory(CATEGORY_MISC);

                //Extension
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Extension), new List<string> { "Call_3" }),
                    new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "Branch_2", "Else_2", "Branch_4", "Branch_7" })
                    ).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Extension), new List<string> { "Call_3" }),
                    new TestInfo(GetInfo(Extensions.ToWord), new List<string> { "Branch_2", "If_4", "Anchor_6", "Branch_7" })
                    ).SetCategory(CATEGORY_MISC);

                //Event
                yield return GetCase(Array.Empty<object>(), true,
                    new TestInfo(GetInfo(Target.Event), new List<string> { "Branch_6", "Call_18", "Call_22", "Call_26" }),
                    new TestInfo(GetInfo(_eventer.NotifyAbout), new List<string> { "Branch_4", "If_6", "Call_8" })
                    ).SetCategory(CATEGORY_MISC);

                #region Enumerator_Implementation
                yield return GetCase(Array.Empty<object>(), true,
                    new TestInfo(GetInfo(Target.Enumerator_Implementation), new List<string> { "Call_7", "Branch_9", "Branch_19", "Anchor_17", "Branch_19", "Anchor_17", "Branch_19", "Anchor_17", "Branch_19", "Anchor_17", "Branch_19", "Branch_22", "Anchor_26" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.StringEnumerable::.ctor()"), false, new List<string>()),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Collections.Generic.IEnumerator`1<System.String> Drill4Net.Target.Common.StringEnumerable::GetEnumerator()"), false, new List<string> { "Branch_5" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.NotEmptyStringEnumerator::.ctor(System.String[])"), false, new List<string>()),
                    new TestInfo(GetInfo(_strEnumerator.MoveNext),
                        new List<string> { "Branch_14", "Else_17", "Call_20", "Branch_24", "Return_26",
                                           "Branch_14", "Else_17", "Call_20", "Branch_24", "Return_26",
                                           "Branch_14", "Else_17", "Call_20", "Branch_24", "Return_26",
                                           "Branch_14", "Else_17", "Call_20", "Branch_24", "Return_26",
                                           "Branch_14", "If_14", "Branch_17", "Return_26" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Int32 Drill4Net.Target.Common.NotEmptyStringEnumerator::GetPosition()"), false,
                        new List<string> { "Branch_4", "Branch_13", "Else_13", "Branch_23", "Branch_27", "Cycle_27", "Anchor_5", "Anchor_6",
                                           "Branch_13", "Else_13", "Branch_23", "Branch_27", "CycleEnd_27", "Branch_30", "Return_32", "Branch_4",
                                           "Branch_13", "Else_13", "Branch_23", "Branch_27", "CycleEnd_27", "Branch_30", "Return_32", "Branch_4",
                                           "Branch_13", "Else_13", "Branch_23", "Branch_27", "Cycle_27", "Anchor_5", "Anchor_6",
                                           "Branch_13", "Else_13", "Branch_23", "Branch_27", "CycleEnd_27", "Branch_30", "Return_32", "Branch_4",
                                           "Branch_13", "Else_13", "Branch_23", "Branch_27", "Cycle_27", "Anchor_5", "Anchor_6",
                                           "Branch_13", "If_23", "Anchor_25", "Branch_27", "CycleEnd_27", "Branch_30" }, true),
                    new TestInfo(GetSourceFromFullSig(Target, "System.String Drill4Net.Target.Common.NotEmptyStringEnumerator::get_Current()"), false,
                        new List<string> { "Branch_4", 
                                           "Else_4", "Branch_14", "Branch_18", "Else_21", "Branch_27", "Return_29", "Branch_4",
                                           "Else_4", "Branch_14", "Branch_18", "Else_21", "Branch_27", "Return_29", "Branch_4",
                                           "Else_4", "Branch_14", "Branch_18", "Else_21", "Branch_27", "Return_29", "Branch_4",
                                           "Else_4", "Branch_14", "Branch_18", "Else_21", "Branch_27" }, true)
                    ).SetCategory(CATEGORY_MISC);
                #endregion

                //LocalFunc
                //Drill doesnt't take into account local func as separate entity
                yield return GetCase(new object[] { false }, new TestInfo(GetInfo(Target.LocalFunc), new List<string> { "Call_3", "Branch_2", "Else_2", "Branch_4", "Branch_7" })).SetCategory(CATEGORY_MISC);
                yield return GetCase(new object[] { true }, new TestInfo(GetInfo(Target.LocalFunc), new List<string> { "Call_3", "Branch_2", "If_4", "Anchor_6", "Branch_7" })).SetCategory(CATEGORY_MISC);

                //Unsafe
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.Unsafe), new List<string> { "Branch_7", "Else_7", "Branch_9", "Branch_25" }),
                    new TestInfo(GetInfo(_point.ToString), new List<string> { "Branch_29" })
                    ).SetCategory(CATEGORY_MISC);

                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.Unsafe), new List<string> { "Branch_7", "If_9", "Anchor_11", "Branch_25" }),
                    new TestInfo(GetInfo(_point.ToString), new List<string> { "Branch_29" })
                    ).SetCategory(CATEGORY_MISC);

#if NETFRAMEWORK
                //ContextBound
                yield return GetCase(new object[] { false },
                    new TestInfo(GetInfo(Target.ContextBound), new List<string> { "Branch_13" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.ContextBound::.ctor(System.Boolean)"), false, 
                        new List<string> { "Branch_6", "Else_6", "Branch_12", "Call_10" })
                    ).SetCategory(CATEGORY_MISC);


                yield return GetCase(new object[] { true },
                    new TestInfo(GetInfo(Target.ContextBound), new List<string> { "Branch_13" }),
                    new TestInfo(GetSourceFromFullSig(Target, "System.Void Drill4Net.Target.Common.ContextBound::.ctor(System.Boolean)"), false, 
                        new List<string> { "Branch_6", "If_8", "Call_10" })
                    ).SetCategory(CATEGORY_MISC);
#endif
                #endregion
            }
        }
    }
}