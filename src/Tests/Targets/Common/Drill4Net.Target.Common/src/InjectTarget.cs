using System;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drill4Net.Target.Common.VB;

[assembly: InternalsVisibleTo("Drill4Net.Target.Tests.Common")]
//[assembly: InternalsVisibleTo("Drill4Net.Target.Tests.Net50")]

//add this in project's csproj file: 
//<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

                                             /*                                             *
                                              *   DON'T OPTIMIZE CODE BY REFACTORING !!!!   *
                                              *   It's needed AS it IS !!!                  *
                                              *                                             */

namespace Drill4Net.Target.Common
{
    #region SuppressMessages
    [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Evident")]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition")]
    [SuppressMessage("ReSharper", "PatternAlwaysMatches")]
    [SuppressMessage("ReSharper", "NotAccessedVariable")]
    [SuppressMessage("ReSharper", "MergeIntoLogicalPattern")]
    [SuppressMessage("ReSharper", "UnreachableSwitchCaseDueToIntegerAnalysis")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
    [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
    [SuppressMessage("ReSharper", "ConvertToLambdaExpression")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UseCancellationTokenForIAsyncEnumerable")]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
    [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    #endregion
    public class InjectTarget
    {
        public async Task RunTests()
        {
            PrintInfo();

            #region If/Else
            IfElse_Half(false);
            IfElse_Half(true);

            IfElse_FullSimple(false);
            IfElse_FullSimple(true);

            IfElse_Ternary_Positive(false);
            IfElse_Ternary_Positive(true);

            IfElse_Ternary_Negative(false);
            IfElse_Ternary_Negative(true);

            IfElse_FullCompound(false, false);
            IfElse_FullCompound(false, true);
            IfElse_FullCompound(true, false);
            IfElse_FullCompound(true, true);

            IfElse_HalfA_FullB(true, false);
            IfElse_HalfA_FullB(true, true);

            IfElse_HalfA_HalfB(true, true);

            IfElse_FullA_HalfB(true, true);
            IfElse_FullA_HalfB(false, true);

            IfElse_Consec_Full(false, false);
            IfElse_Consec_Full(false, true);
            IfElse_Consec_Full(true, false);
            IfElse_Consec_Full(true, true);

            IfElse_Consec_HalfA_FullB(true, false);
            IfElse_Consec_HalfA_FullB(true, true);

            IfElse_Half_EarlyReturn_Bool(false);
            IfElse_Half_EarlyReturn_Bool(true);
#if NETCOREAPP
            IfElse_Half_EarlyReturn_Tuple(false);
            IfElse_Half_EarlyReturn_Tuple(true);
#endif
            #endregion
            #region Switch
            Switch_TwoCases_Into_IfElse(-1);
            Switch_TwoCases_Into_IfElse(0);
            Switch_TwoCases_Into_IfElse(1);

            Switch_ThreeCases_Into_Switch(-1);
            Switch_ThreeCases_Into_Switch(0);
            Switch_ThreeCases_Into_Switch(1);
            Switch_ThreeCases_Into_Switch(2);

            Switch_ExplicitDefault(-1);
            Switch_ExplicitDefault(0);
            Switch_ExplicitDefault(1);
            Switch_ExplicitDefault(2);

            Switch_ImplicitDefault(-1);
            Switch_ImplicitDefault(0);
            Switch_ImplicitDefault(1);
            Switch_ImplicitDefault(2);

            Switch_WithoutDefault(-1);
            Switch_WithoutDefault(0);
            Switch_WithoutDefault(1);
            Switch_WithoutDefault(2);

            Switch_When(-1);
            Switch_When(0);
            Switch_When(1);

            Switch_Property(null);
            Switch_Property(false);
            Switch_Property(true);

#if NETCOREAPP
            Switch_AsReturn(-1);
            Switch_AsReturn(0);
            Switch_AsReturn(1);
            Switch_AsReturn(2);

            Switch_Tuple("English", "morning");
            Switch_Tuple("English", "evening");
            Switch_Tuple("German", "morning");
            Switch_Tuple("German", "evening");
#endif
//#if NET5_0
            Switch_Relational(-5);
            Switch_Relational(5);
            Switch_Relational(10);
            Switch_Relational(100);

            Switch_Logical(-5);
            Switch_Logical(5);
            Switch_Logical(10);
//#endif
            #endregion
            #region Elvis
            Elvis_NotNull();
            Elvis_Null();

            Elvis_Sequence_NotNull();
            Elvis_Sequence_Null();

            Elvis_Double_NotNull();
            Elvis_Double_Null();
            #endregion
            #region Linq
            Linq_Query(false);
            Linq_Query(true);

            Linq_Fluent(false);
            Linq_Fluent(true);

            Linq_Fluent_Double(false);
            Linq_Fluent_Double(true);
            #endregion
            #region Lambda
            Lambda(5);
            Lambda(10);

            Lambda_AdditionalBranch(10);

            Lambda_AdditionalSwitch(5);
            Lambda_AdditionalSwitch(10);
            Lambda_AdditionalSwitch(12);
            #endregion
            #region Generics
            Generics_Var(false);
            Generics_Var(true);

            Generics_Call_Base(false);
            Generics_Call_Base(true);

            Generics_Call_Child(false);
            Generics_Call_Child(true);
            #endregion
            #region Anonymous, Expression
            Anonymous_Func();
            Anonymous_Func_Invoke();
            Anonymous_Func_WithLocalFunc();

            Anonymous_Type(false);
            Anonymous_Type(true);

            Expression(5);
            Expression(10);
            #endregion
            #region Try/cath/finally
            Try_Catch(false);
            Try_Catch(true);

            Try_Catch_VB(false);
            Try_Catch_VB(true);

            Try_CatchWhen(false, false);
            Try_CatchWhen(false, true);
            Try_CatchWhen(true, false);
            Try_CatchWhen(true, true);

            Try_WithCondition(false);
            Try_WithCondition(true);

            Try_Finally(false);
            Try_Finally(true);

            Try_Finally_VB(false);
            Try_Finally_VB(true);
            #endregion
            #region Async
#if !NET461 && !NETSTANDARD2_0
            await Async_Stream();
            await Async_Stream_Cancellation();
#endif
            await Async_Task(false);
            await Async_Task(true);

            await Async_Lambda(false);
            await Async_Lambda(true);

            Async_Linq_Blocking(false);
            Async_Linq_Blocking(true);

            await Async_Linq_NonBlocking(false);
            await Async_Linq_NonBlocking(true);
            #endregion
            #region Parallel
            Parallel_Linq(false);
            Parallel_Linq(true);

            Parallel_For(false);
            Parallel_For(true);

            Parallel_Foreach(false);
            Parallel_Foreach(true);

            Parallel_Task_New(false);
            Parallel_Task_New(true);

            Parallel_Thread_New(false);
            Parallel_Thread_New(true);
            #endregion
            #region Disposable
            try
            {
                Disposable_Using_Exception(false);
                Disposable_Using_Exception(true);

                Disposable_Using_Last_Exception();
            }
            catch
            { }

            Disposable_Using_SyncRead(false);
            Disposable_Using_SyncRead(true);

            await Disposable_Using_AsyncRead(false);
            await Disposable_Using_AsyncRead(true);

            await Disposable_Using_AsyncTask(false);
            await Disposable_Using_AsyncTask(true);

            Disposable_Finalizer(17);
            Disposable_Finalizer(18);
            #endregion
            #region Cycle
            Cycle_For(-1);
            Cycle_For(3);

            Cycle_For_Break(3); //no break;
            Cycle_For_Break(2); //with break;
            
            Cycle_Foreach();

            Cycle_While(-1);
            Cycle_While(3);

            Cycle_Do();
            #endregion
            #region Misc
            Lock_Statement(false);
            Lock_Statement(true);

            Yield(false);
            Yield(true);

            Goto_Statement(false);
            Goto_Statement(true);
            
            Goto_Statement_Cycle_Backward();

            Goto_Statement_Cycle_Forward(false);
            Goto_Statement_Cycle_Forward(true);

            Extension(false);
            Extension(true);

            LocalFunc(false);
            LocalFunc(true);

            Enumerator_Implementation();
            Event();

#if NETFRAMEWORK
            ContextBound(false);
            ContextBound(true);
#endif
            ExpandoObject(false);
            ExpandoObject(true);

            DynamicObject(false);
            DynamicObject(true);

            Unsafe(false);
            Unsafe(true);

            WinAPI(false);
            WinAPI(true);

            CallAnotherTarget();
            #endregion
        }

        #region IF/ELSE
        public void IfElse_Half(bool cond)
        {
            var type = "no";
            if (cond)
                type = "yes";

            Console.WriteLine($"{nameof(IfElse_Half)}: {type}");
        }

        public void IfElse_FullSimple(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple)}: {type}");
        }

        public void IfElse_Consec_Full(bool a, bool b)
        {
            var info = new bool?[2, 2];
            if (a)
            {
                info[0, 0] = true;
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: YES1");
            }
            else
            {
                info[0, 1] = false;
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: NO1");
            }
            //
            if (b)
            {
                info[1, 0] = true;
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: YES2");
            }
            else
            {
                info[0, 1] = false;
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: NO2");
            }
        }

        public bool IfElse_Consec_HalfA_FullB(bool a, bool b)
        {
            if (a)
            {
                Console.WriteLine($"{nameof(IfElse_Consec_HalfA_FullB)}: YES1");
            }
            //
            if (b)
            {
                Console.WriteLine($"{nameof(IfElse_Consec_HalfA_FullB)}: YES2");
            }
            else
            {
                Console.WriteLine($"{nameof(IfElse_Consec_HalfA_FullB)}: NO2");
            }
            return false;
        }

        public bool IfElse_Half_EarlyReturn_Bool(bool cond)
        {
            var type = "no"; //let it be... Let it beeee!...
            if (cond)
            {
                type = "yes";
                Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Bool)}: {type}");
                return true;
            }
            Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Bool)}: {type}");

            return false; //don't change to void
        }

#if NETCOREAPP
        public (bool, bool) IfElse_Half_EarlyReturn_Tuple(bool cond)
        {
            var type = "no"; //let it be... Let it beeee!...
            if (cond)
            {
                type = "yes";
                Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Tuple)}: {type}");
                return (true, true);
            }
            Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Tuple)}: {type}");

            return (false, false);
        }

#endif
        public void IfElse_Ternary_Positive(bool cond)
        {
            var type = cond ? "yes" : "no";
            Console.WriteLine($"{nameof(IfElse_Ternary_Positive)}: {type}");
        }

        public void IfElse_Ternary_Negative(bool cond)
        {
            var type = !cond ? "no" : "yes";
            Console.WriteLine($"{nameof(IfElse_Ternary_Negative)}: {type}");
        }

        public void IfElse_FullCompound(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_FullCompound)}: ab");
                }
                else
                {
                    Console.WriteLine($"{nameof(IfElse_FullCompound)}: a!b");
                }
            }
            else
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_FullCompound)}: !ab");
                }
                else
                {
                    Console.WriteLine($"{nameof(IfElse_FullCompound)}: !a!b");
                }
            }
        }

        public void IfElse_HalfA_FullB(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_HalfA_FullB)}: ab");
                }
                else
                {
                    Console.WriteLine($"{nameof(IfElse_HalfA_FullB)}: a!b");
                }
            }
        }

        public void IfElse_HalfA_HalfB(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_HalfA_FullB)}: ab");
                }
            }
        }

        public void IfElse_FullA_HalfB(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_FullA_HalfB)}: ab");
                }
            }
            else
            {
                Console.WriteLine($"{nameof(IfElse_FullA_HalfB)}: !a*b");
            }
        }
        #endregion
        #region Elvis
        public void Elvis_NotNull()
        {
            var obj = new GenStr("aaa");
            var prop = obj?.Prop; //need Elvis !
            Console.WriteLine($"{nameof(Elvis_NotNull)}: {prop}");
        }

        public void Elvis_Null()
        {
            GenStr obj = null;
            var prop = obj?.Prop; //need Elvis !
            Console.WriteLine($"{nameof(Elvis_NotNull)}: {prop}");
        }

        public void Elvis_Sequence_NotNull()
        {
            var obj = new GenStr("aaa");
            var len = obj?.Prop?.Length;
            Console.WriteLine($"{nameof(Elvis_Sequence_NotNull)}: {len}");
        }

        public void Elvis_Sequence_Null()
        {
            GenStr obj = null;
            var len = obj?.Prop?.Length;
            Console.WriteLine($"{nameof(Elvis_Sequence_Null)}: {len}");
        }

        public void Elvis_Double_NotNull()
        {
            var obj = "aaa";
            var prop = obj ?? "bbb";
            Console.WriteLine($"{nameof(Elvis_Double_NotNull)}: {prop}");
        }

        public void Elvis_Double_Null()
        {
            string obj = null;
            var prop = obj ?? "bbb";
            Console.WriteLine($"{nameof(Elvis_Double_Null)}: {prop}");
        }
        #endregion
        #region Switch
        public void Switch_TwoCases_Into_IfElse(int a)
        {
            var s = "";
            s = a switch
            {
                0 => "A",
                1 => "B",
                _ => "explicit default",
            };
            Console.WriteLine($"{nameof(Switch_TwoCases_Into_IfElse)}: {a} -> {s}");
        }

        public void Switch_ThreeCases_Into_Switch(int a)
        {
            var s = "";
            s = a switch
            {
                0 => "A",
                1 => "B",
                2 => "C",
                _ => "explicit default",
            };
            Console.WriteLine($"{nameof(Switch_ThreeCases_Into_Switch)}: {a} -> {s}");
        }

        public void Switch_ExplicitDefault(int a)
        {
            var s = "";
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_ExplicitDefault)}: {a} -> return"); return;
                case 0: s = "A"; break;
                case 1: s = "B"; break;
                default: s = "explicit default"; break;
            }
            Console.WriteLine($"{nameof(Switch_ExplicitDefault)}: {a} -> {s}");
        }

        public void Switch_ImplicitDefault(int a)
        {
            var s = "implicit default";
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_ImplicitDefault)}: {a} -> return"); return;
                case 0: s = "A"; break;
                case 1: s = "B"; break;
            }
            Console.WriteLine($"{nameof(Switch_ImplicitDefault)}: {a} -> {s}");
        }

        public void Switch_WithoutDefault(int a)
        {
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
                case 0: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
                case 1: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
            }
            Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a} -> no default");
        }

        public void Switch_When(int a)
        {
            //in IL code it presents as set of simple 'if/else', not as 'switch'
            //statement in any cases (not depend from count of these 'cases')
            var s = "default";
            switch (a)
            {
                case int x when a < 0: s = "A"; x = 6; break;
                case int x when a == 0: s = "B"; x = 3; break;
                case int x when a > 0: s = "C"; x = 3; break;
                default: Console.WriteLine($"{nameof(Switch_When)}: {a} -> {s}"); return;
            }
            Console.WriteLine($"{nameof(Switch_When)}: {a} -> {s}");
        }

        public string Switch_Property(bool? cond)
        {
            var p = cond == null ?
                new { Name = "John", IsAdmin = false, Language = "English" } :
                (cond == true ?
                    new { Name = "Андрей", IsAdmin = false, Language = "Russian" } :
                    new { Name = "Woldemar", IsAdmin = true, Language = "German" }
                );
            //
            var s = p switch
            {
                { Language: "English" } => $"Hello, {p.Name}!",
                { Language: "German", IsAdmin: true } => "Hallo, Geheimagent!",
                { Language: "Russian" } => $"Привет, {p.Name}!",
                { } => "undefined",
                null => "null"
            };
            Console.WriteLine($"{nameof(Switch_Property)}: {p} -> {s}");
            return s;
        }

#if NETCOREAPP
        public string Switch_AsReturn(int a)
        {
            Console.WriteLine($"{nameof(Switch_AsReturn)}: {a}");

            return a switch
            {
                -1 => "",
                0 => "A",
                1 => "B",
                _ => "default",
            };
        }

        public string Switch_Tuple(string lang, string daytime)
        {
            var s = (lang, daytime) switch
            {
                ("English", "morning") => "Good morning",
                ("English", "evening") => "Good evening",
                ("German", "morning") => "Guten Morgen",
                ("German", "evening") => "Guten Abend",
                _ => "Доброго времени суток!"
            };
            Console.WriteLine($"{nameof(Switch_Tuple)}: {s}");
            return s;
        }
#endif
        //C#9
        public double Switch_Relational(double sum)
        {
            var newSum = sum switch
            {
                <= 0 => 0,
                < 10 => sum * 1.05,
                < 100 => sum * 1.1,
                _ => sum * 1.15
            };
            Console.WriteLine($"{nameof(Switch_Relational)}: {newSum:F2}");
            return sum;
        }

        //C#9
        public string Switch_Logical(int a)
        {
            var s = a switch
            {
                <= 0 => "zero",
                > 0 and < 10 => "small",
                _ => "big"
            };
            Console.WriteLine($"{nameof(Switch_Logical)}: {s}");
            return s;
        }
        #endregion
        #region Cycle
        public bool Cycle_For(int count)
        {
            var s = "";
            for (var i = 0; i < count; i++)
                s += i;
            Console.WriteLine($"{nameof(Cycle_For)} -> {s}");
            return true;
        }

        public bool Cycle_For_Break(int breakInd)
        {
            var s = "";
            for (var i = 0; i < 3; i++)
            {
                if (i == breakInd)
                    break;
                s += i;
            }
            Console.WriteLine($"{nameof(Cycle_For_Break)} -> {s}");
            return true;
        }

        public void Cycle_Foreach()
        {
            var ar = new string[] { "a", "b", "c" };
            string str = null;
            foreach (var s in ar)
                str += s;
            Console.WriteLine($"{nameof(Cycle_Foreach)} -> {str}");
        }

        public bool Cycle_While(int count)
        {
            Console.WriteLine($"{nameof(Cycle_While)} -> {count}");
            while (count > 0)
                count--;
            return true;
        }

        public void Cycle_Do()
        {
            var i = 3;
            Console.WriteLine($"{nameof(Cycle_Do)} -> {i}");
            do { i--; } while (i > 0);
        }
        #endregion
        #region Linq
        public void Linq_Query(bool all)
        {
            var cities = new List<string> { "Paris", "London", "Moscow" };
            var res = from c in cities where all || c == "London" select c;
            Console.WriteLine($"{nameof(Linq_Query)}: {string.Join(",", res)}");
        }

        public void Linq_Fluent(bool all)
        {
            var cities = new List<string> { "Paris", "London", "Moscow" };
            var res = cities.Where(c => all ? c != null : c == "London");
            Console.WriteLine($"{nameof(Linq_Fluent)}: {string.Join(",", res)}");
        }
        
        public void Linq_Fluent_Double(bool all)
        {
            var cities = new List<string> { "Paris", "London", "Moscow" };
            var res = cities.Where(c => all ? c != null : c == "London");
            var customers = new List<string> { "Microsoft", "IBM", "Google" };
            var res2 = customers.Where(a => a.StartsWith("G", StringComparison.CurrentCultureIgnoreCase));
            Console.WriteLine($"{nameof(Linq_Fluent)}: {string.Join(",", res)} -> {string.Join(",", res2)}");
        }
        #endregion
        #region Lambda
        public void Lambda(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            var d = square(x);
            Console.WriteLine($"{nameof(Lambda)}: {d}");
        }

        public void Lambda_AdditionalBranch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            var d = square(x);
            if (d > 100)
                d /= 2;
            Console.WriteLine($"{nameof(Lambda_AdditionalBranch)}: {d}");
        }

        public void Lambda_AdditionalSwitch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            var d = square(x);
            switch (d)
            {
                case 100: d = 50; break;
                case 144: d = 75; break;
            }
            Console.WriteLine($"{nameof(Lambda_AdditionalSwitch)}: {d}");
        }
        #endregion
        #region Generics
        public void Generics_Var(bool cond)
        {
            var list = new List<string> { "a", "b", "c" };
            if (cond)
            {
                if (list.Count > 0)
                    list.RemoveAt(0);
            }
            else
            {
                list.Reverse();
            }
            Console.WriteLine($"{nameof(Generics_Var)}: {cond} -> {string.Join(",", list)}");
        }

        public void Generics_Call_Base(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = gen.GetDesc(cond);
            Console.WriteLine($"{nameof(Generics_Call_Base)}: {s}");
        }

        public void Generics_Call_Child(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = cond ? gen.GetShortDesc() : "no desc";
            Console.WriteLine($"{nameof(Generics_Call_Child)}: {s}");
        }
        #endregion
        #region Try/cath/finally
        public void Try_Catch(bool cond)
        {
            var s = "none";
            try
            {
                throw new Exception();
            }
            catch
            {
                s = cond ? "YES" : "NO";
            }
            Console.WriteLine($"{nameof(Try_Catch)}: {s}");
        }

        public void Try_CatchWhen(bool cond, bool cond2)
        {
            var s = "none";
            try
            {
                throw new ArgumentException();
            }
            catch when (cond2)
            {
                s = cond ? "YES" : "NO";
            }
            catch { }
            Console.WriteLine($"{nameof(Try_CatchWhen)}: {s}");
        }

        public void Try_Finally(bool cond)
        {
            string s = null;
            try
            {
                s = "A";
            }
            finally
            {
                s = $"{(cond ? "YES" : "NO")}/{s}";
            }
            Console.WriteLine($"{nameof(Try_Finally)}: {s}");
        }
        
        public void Try_WithCondition(bool cond)
        {
            string s = null;
            try
            {
                s = cond ? "YES" : throw new Exception("Thrown exception");
            }
            catch(Exception ex)
            {
                s = ex.Message;
            }
            Console.WriteLine($"{nameof(Try_Finally)}: {s}");
        }
        #endregion
        #region Dynamic
        public void ExpandoObject(bool cond)
        {
            dynamic exp = new ExpandoObject();
            exp.Act = (Func<bool, string>)((a) => { return a ? "yes" : "false"; });
            exp.Act(cond);
            Console.WriteLine($"{nameof(ExpandoObject)}: {cond}");
        }

        public void DynamicObject(bool cond)
        {
            dynamic exp = new DynamicDictionary();
            exp.Act = (Func<bool, string>)((a) => { return a ? "yes" : "false"; });
            exp.Act(cond);
            Console.WriteLine($"{nameof(DynamicObject)}: {cond}");
        }
        #endregion
        #region Async/await
        public async Task Async_Task(bool cond)
        {
            if (cond)
                await Task.Delay(50);
            else
                await Delay100();
            Console.WriteLine($"{nameof(Async_Task)}: {cond}");
        }

        internal Task Delay100()
        {
            return Task.Delay(100);
        }

        public async Task Async_Lambda(bool cond)
        {
            await Task.Run(async () =>
            {
                if (cond)
                    await Task.Delay(50);
                else
                    await Task.Delay(100);
                Console.WriteLine($"{nameof(Async_Lambda)}: {cond}");
            });
        }

        public void Async_Linq_Blocking(bool cond)
        {
            var data = GetDataForAsyncLinq();
            var inputs = data.Select(async ev => await ProcessElement(ev, cond))
                   .Select(t => t.GetAwaiter().GetResult().Prop)
                   .Where(i => i != null)
                   .ToList();

            Console.WriteLine($"{nameof(Async_Linq_Blocking)}: {string.Join(", ", inputs)}");
        }

        public async Task Async_Linq_NonBlocking(bool cond)
        {
            var data = GetDataForAsyncLinq();
            var tasks = await Task.WhenAll(data.Select(ev => ProcessElement(ev, cond)));
            var inputs = tasks
                .Select(a => a.Prop)
                .Where(result => result != null)
                .ToList();

            Console.WriteLine($"{nameof(Async_Linq_NonBlocking)}: {string.Join(", ", inputs)}");
        }

        internal List<GenStr> GetDataForAsyncLinq()
        {
            return new List<GenStr> { new GenStr("A"), new GenStr("B"), new GenStr("C"), };
        }

        internal async Task<GenStr> ProcessElement(GenStr element, bool cond)
        {
            return await Task.Run(() =>
            {
                if (cond) //If_5
                    element.Prop += "/1";
                return element;
            });
        }

        #region Stream
#if !NET461 && !NETSTANDARD2_0
        #region Simple
        public async Task Async_Stream()
        {
            IAsyncEnumerable<int> enumerable = GenerateSequenceAsync();
            var s = "";
            await foreach (var i in enumerable)
            {
                if (i > 2) //If_54
                    s += i;
            }
            Console.WriteLine($"{nameof(Async_Stream)}: {s}");
        }

        public async IAsyncEnumerable<int> GenerateSequenceAsync()
        {
            for (int i = 1; i <= 3; i++) //Else_24
            {
                if (i % 2 == 1) //If_36
                {
                    await Task.Delay(10);
                    yield return i;
                }
            }
        }
        #endregion
        #region Cancellation
        public async Task Async_Stream_Cancellation()
        {
            //data
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            IAsyncEnumerable<int> enumerable = GenerateSequenceWithCancellationAsync(token);
            enumerable.ConfigureAwait(false);

            //processing
            var s = "";
            try
            {
                await foreach (var i in enumerable)
                {
                    if (i == 2)
                    {
                        s += i;
                        cts.Cancel();
                    }
                }
            }
            catch { } //on cancellation

            Console.WriteLine($"{nameof(Async_Stream_Cancellation)}: {s}");
        }

        public async IAsyncEnumerable<int> GenerateSequenceWithCancellationAsync
            ([EnumeratorCancellation] CancellationToken token = default)
        {
            for (int i = 1; i <= 3; i++)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(10);
                yield return i;
            }
        }
        #endregion

        //https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
#endif
        #endregion
        #endregion
        #region Parallel
        public void Parallel_Linq(bool cond)
        {
            var data = GetDataForParallel(5);
            int sum = data.AsParallel().Where(a => !cond || (cond && a % 2 == 0)).Sum();
            Console.WriteLine($"{nameof(Parallel_Linq)}: {sum}");
        }

        public void Parallel_For(bool cond)
        {
            var data = GetDataForParallel(5);
            int sum = 0;

            Parallel.For(0, data.Count(), a =>
            {
                if (!cond || (cond && a % 2 == 0))
                    Interlocked.Add(ref sum, a);
            });
            Console.WriteLine($"{nameof(Parallel_For)}: {sum}");
        }

        public void Parallel_Foreach(bool cond)
        {
            var data = GetDataForParallel(5);
            int sum = 0;
            Parallel.ForEach(data, a =>
            {
                if (!cond || (cond && a % 2 == 0))
                    Interlocked.Add(ref sum, a);
            });
            Console.WriteLine($"{nameof(Parallel_Foreach)}: {sum}");
        }

        private IEnumerable<int> GetDataForParallel(int cnt = 5)
        {
            return Enumerable.Range(0, cnt);
        }

        public void Parallel_Task_New(bool cond)
        {
            Task[] tasks = new Task[2];
            List<string> list1 = null;
            List<string> list2 = null;

            tasks[0] = Task.Factory.StartNew(() => list1 = GetStringListForTaskNew(cond));
            tasks[1] = Task.Factory.StartNew(() => list2 = !cond ? new List<string> { "Y2" } : new List<string> { "A2, B2, C2" });

            Task.WaitAll(tasks);
            Console.WriteLine($"{nameof(Parallel_Task_New)}: {cond} -> {string.Join(",", list1)} / {string.Join(",", list2)}");
        }

        public List<string> GetStringListForTaskNew(bool cond)
        {
            return cond ? new List<string> { "X1" } : new List<string> { "A1, B1, C1" };
        }

        public void Parallel_Thread_New(bool cond)
        {
            var tr = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                GetStringListForThreadNew(cond);
            });
            tr.Start();
            tr.Join();
        }

        public void GetStringListForThreadNew(bool cond)
        {
            var list = cond ? new List<string> { "XYZ" } : new List<string> { "A, B, C" };
            Console.WriteLine($"{nameof(Parallel_Thread_New)}: {cond} -> {string.Join(",", list)}");
        }
        #endregion
        #region Disposable
        public void Disposable_Using_Last_Exception()
        {
            Console.WriteLine($"{nameof(Disposable_Using_Last_Exception)}");
            #pragma warning disable IDE0063 // Use simple 'using' statement
            using (var ms = new MemoryStream())
            #pragma warning restore IDE0063 // Use simple 'using' statement
            {
                throw new Exception($"The exception has been thrown");
            }
        }

        public void Disposable_Using_Exception(bool cond)
        {
            Console.WriteLine($"{nameof(Disposable_Using_Exception)}: {cond}");
            #pragma warning disable IDE0063 // Use simple 'using' statement
            using (var ms = new MemoryStream())
            #pragma warning restore IDE0063 // Use simple 'using' statement
            {
                if (cond)
                    throw new Exception($"The exception has been thrown");
            }
        }

        public void Disposable_Using_SyncRead(bool cond)
        {
            byte cnt = 5;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    ms.Read(res, 0, (int)ms.Length);
            }
            Console.WriteLine($"{nameof(Disposable_Using_SyncRead)}: {cond}");
        }

        public async Task Disposable_Using_AsyncRead(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    #pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                    await ms.ReadAsync(res, 0, (int)ms.Length);
                    #pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
            }
            Console.WriteLine($"{nameof(Disposable_Using_AsyncRead)}: {cond}");
        }

        public async Task Disposable_Using_AsyncTask(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    await AsyncWait();
            }
            Console.WriteLine($"{nameof(Disposable_Using_AsyncRead)}: {cond}");
        }

        private Task AsyncWait()
        {
            return Task.Run(() => { Thread.Sleep(10); });
        }

        public void Disposable_Finalizer(int len)
        {
            CreateDisposable(len);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine($"{nameof(Disposable_Finalizer)}: {len}");
        }

        private void CreateDisposable(int len)
        {
            new Finalizer(len);
        }
        #endregion
        #region Anonymous
        #region AnonymousFunc
        delegate int Operation(int x, int y);
        public void Anonymous_Func()
        {
            int z = 8;
            #pragma warning disable IDE0039 // Use local function
            Operation operation = delegate (int x, int y)
            #pragma warning restore IDE0039 // Use local function
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            };
            int d = operation(4, 5);
            Console.WriteLine($"{nameof(Anonymous_Func)}: {d}"); // 15
        }

        public void Anonymous_Func_Invoke()
        {
            int z = 8;
            #pragma warning disable IDE0039 // Use local function
            Operation operation = delegate (int x, int y)
            #pragma warning restore IDE0039 // Use local function
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            };
            int d = operation.Invoke(4, 5);
            Console.WriteLine($"{nameof(Anonymous_Func)}: {d}"); // 15
        }

        public void Anonymous_Func_WithLocalFunc()
        {
            int z = 8;
            int d = operation(4, 5);
            Console.WriteLine($"{nameof(Anonymous_Func_WithLocalFunc)}: {d}"); // 15

            //local func
            int operation(int x, int y)
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            }
        }
        #endregion

        public void Anonymous_Type(bool cond)
        {
            var tom = new { Name = "Tom", Age = cond ? 21 : 9 };
            Console.WriteLine($"{nameof(Anonymous_Type)}: {cond} -> {tom.Age}");
        }
        #endregion
        #region VB.NET
        public void Try_Catch_VB(bool cond)
        {
            var lib = new VBLibrary();
            lib.VB_Try_Catch(cond);
        }

        public void Try_Finally_VB(bool cond)
        {
            var lib = new VBLibrary();
            lib.VB_Try_Finally(cond);
        }
        #endregion
        #region F#

        #endregion
        #region Misc
        #region WinAPI
        [DllImport("user32.dll")]
        public static extern void SetWindowText(IntPtr hwnd, String lpString);

        public void WinAPI(bool cond)
        {
            SetWindowText(IntPtr.Zero, cond ? "Bye!" : "Hello!");
            Console.WriteLine($"{nameof(WinAPI)}: {cond}");
        }
        #endregion
        #region Lock
        private readonly object _locker = new object();
        public void Lock_Statement(bool cond)
        {
            string s;
            lock (_locker)
            {
                s = cond ? "YES" : "NO";
            }
            Console.WriteLine($"{nameof(Lock_Statement)}: {s}");
        }
        #endregion
        #region Yield
        public void Yield(bool cond)
        {
            var list = GetForYield(cond);
            Console.WriteLine($"{nameof(Yield)}: {cond} -> {string.Join(",", list)}");
        }

        public IEnumerable<string> GetForYield(bool cond)
        {
            var list = new List<string> { "Y1, Y2, Y3" };
            foreach (var a in list)
                yield return cond ? a : "z";
        }
        #endregion
        #region Goto
        public void Goto_Statement(bool cond)
        {
            var s = "aaa";
            if (cond)
                goto label;
            s = "bbb";
            label:
            Console.WriteLine($"{nameof(Goto_Statement)}: {cond} -> {s}");
        }

        public void Goto_Statement_Cycle_Forward(bool cond)
        {
            var s = "a";
            //jump inside of cycle is forbidden
            for (var i = 0; i < 2; i++)
            {
                s += i;
                if (cond)
                    goto label;
                s += "b";
            }
            label:
            Console.WriteLine($"{nameof(Goto_Statement_Cycle_Forward)}: {cond} -> {s}");
        }
        
        public void Goto_Statement_Cycle_Backward()
        {
            var a = -1;
            label:
            a++;
            var s = a.ToString();
            while (true)
            {
                if(a == 0)
                    goto label;
                break;
            }
            Console.WriteLine($"{nameof(Goto_Statement_Cycle_Backward)}: -> {s}");
        }
        #endregion

        public void CallAnotherTarget()
        {
            var s = new Another.AnotherTarget().WhoAreU();
            Console.WriteLine($"{nameof(CallAnotherTarget)} -> {s}");
        }

        public void Event()
        {
            Console.WriteLine($"{nameof(Event)} started");

            #pragma warning disable IDE0039 // Use local function
            NotifyHandler p = delegate (string mes)
            {
                Console.WriteLine($"{nameof(Event)} -> {mes}");
            };
            #pragma warning restore IDE0039 // Use local function

            var eventer = new Eventer();
            eventer.Notify += p;
            eventer.NotifyAbout("AAA");
            eventer.Notify -= p;
        }

        public void LocalFunc(bool cond)
        {
            //we don't take into account local func as separate entity
            Console.WriteLine($"{nameof(LocalFunc)}: {GetString(cond)}");

            string GetString(bool cond)
            {
                return cond ? "YES" : "NO";
            }
        }

        public void Extension(bool cond)
        {
            Console.WriteLine($"{nameof(Extension)}: {cond.ToWord()}");
        }

        //TODO: not working yet!
        public void Expression(int x)
        {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x < 10 ? 0 : x * x;
            var dlg = e.Compile();
            var d = dlg(x);
            Console.WriteLine($"{nameof(Expression)}: {d}");
        }

        public void Enumerator_Implementation()
        {
            var enumerable = new StringEnumerable();
            var s = "";
            foreach (var a in enumerable)
                s += a;
            Console.WriteLine($"{nameof(Enumerator_Implementation)}: {s}");
        }

#if NETFRAMEWORK
        public bool ContextBound(bool cond)
        {
            new ContextBound(cond);
            Console.WriteLine($"{nameof(ContextBound)}: {cond}");
            return true;
        }
#endif
        public bool Unsafe(bool cond)
        {
            MyPoint point;
            unsafe
            {
                MyPoint* p = &point;
                p->x = cond ? 10 : 20;
                p->y = 35;
                Console.WriteLine($"{nameof(Unsafe)}: {p->ToString()}");
            }
            return true;
        }
        #endregion

        private byte[] GetBytes(byte cnt)
        {
            var arr = new byte[cnt];
            for (byte i = 0; i < cnt; i++)
                arr[i] = i;
            return arr;
        }

        private static void PrintInfo()
        {
            var s = "UNKNOWN";
#if NET461
            s = "Net461";
#endif
#if NET48
            s = "Net48";
#endif
#if NET5_0
            s = "Net50";
#endif
#if NETCOREAPP3_1
            s = "Core31";
#endif
#if NETCOREAPP2_2
            s = "Core22";
#endif
            Console.WriteLine($"  *** Version: {s}  ***\n");
        }

        //TODO: LinqToXML, LinkToEntity, a || b (with PDB)...
    }
}
