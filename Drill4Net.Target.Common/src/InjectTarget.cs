using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Drill4Net.Target.Comon.Tests")]

namespace Drill4Net.Target.Common
{
    public class InjectTarget
    {
        public async Task RunTests()
        {
            #region If/Else
            IfElse_Half(false);
            IfElse_Half(true);

            IfElse_FullSimple(false);
            IfElse_FullSimple(true);

            Ternary_Positive(false);
            Ternary_Positive(true);

            Ternary_Negative(false);
            Ternary_Negative(true);

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

            IfElse_Half_EarlyReturn_Func(false);
            IfElse_Half_EarlyReturn_Func(true);
            #endregion
            #region Switch
            Switch_WithReturn(-1);
            Switch_WithReturn(0);
            Switch_WithReturn(1);
            Switch_WithReturn(2);

            Switch_WithoutDefault(-1);
            Switch_WithoutDefault(0);
            Switch_WithoutDefault(1);
            Switch_WithoutDefault(2);

            Switch_AsReturn(-1);
            Switch_AsReturn(0);
            Switch_AsReturn(1);
            Switch_AsReturn(2);
            #endregion
            #region Generics
            var list = new List<string> { "a", "b", "c" };
            GenericParameter(list, false);
            GenericParameter(list, true);

            GenericVar(false);
            GenericVar(true);

            Generic_Call_Base(false);
            Generic_Call_Base(true);

            Generic_Call_Child(false);
            Generic_Call_Child(true);
            #endregion
            #region Anonymous, lambda, Linq
            AnonymousFunc();

            AnonymousType(false);
            AnonymousType(true);

            Lambda10(5);
            Lambda10(10);

            Lambda10_AdditionalBranch(10);

            Lambda10_AdditionalSwitch(5);
            Lambda10_AdditionalSwitch(10);
            Lambda10_AdditionalSwitch(12);

            Expression10(5);
            Expression10(10);

            Linq_Query(false);
            Linq_Query(true);

            Linq_Fluent(false);
            Linq_Fluent(true);
            #endregion
            #region Try/cath/finally
            try
            {
                Exception_Conditional(false);
                Exception_Conditional(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{ex}");
            }

            Catch_Statement(false);
            Catch_Statement(true);

            Catch_When_Statement(false, false);
            Catch_When_Statement(false, true);
            Catch_When_Statement(true, false);
            Catch_When_Statement(true, true);

            Finally_Statement(false);
            Finally_Statement(true);
            #endregion
            #region Async
            await AsyncTask(false);
            await AsyncTask(true);

            await AsyncLambda(false);
            await AsyncLambda(true);

            AsyncLinq_Blocking(false);
            AsyncLinq_Blocking(true);

            await AsyncLinq_NonBlocking(false);
            await AsyncLinq_NonBlocking(true);
            #endregion
            #region Parallel
            Plinq(false);
            Plinq(true);

            ForParallel(false);
            ForParallel(true);

            ForeachParallel(false);
            ForeachParallel(true);
            #endregion
            #region Using/finalizer
            UsingStatement_SyncRead(false);
            UsingStatement_SyncRead(true);

            await UsingStatement_AsyncRead(false);
            await UsingStatement_AsyncRead(true);

            await UsingStatement_AsyncTask(false);
            await UsingStatement_AsyncTask(true);

            Finalizer(17);
            Finalizer(18);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            #endregion
            #region Misc
            While_Operator(-1);
            While_Operator(3);

            Do_Operator();

            Lock_Statement(false);
            Lock_Statement(true);

            ContextBound(-1);
            ContextBound(1);

            ExpandoObject(false);
            ExpandoObject(true);

            DynamicObject(false);
            DynamicObject(true);

            Unsafe(false);
            Unsafe(true);

            WinAPI(false);
            WinAPI(true);
            #endregion
        }

        #region IF/ELSE
        internal void IfElse_Half(bool cond)
        {
            var type = "no";
            if (cond)
                type = "yes";

            //var stackTrace = new StackTrace();
            //StackFrame[] stackFrames = stackTrace.GetFrames();

            //// write call stack method names
            //foreach (StackFrame stackFrame in stackFrames)
            //{
            //    var method = stackFrame.GetMethod() as MethodInfo;
            //    if (method == null)
            //        continue;
            //    var mType = method.DeclaringType;
            //    if (mType.Name.StartsWith("<"))
            //        continue;
            //    var asmName = mType.Assembly.GetName().Name;
            //    //GUANO! By file path is better?
            //    if (asmName.StartsWith("System.") || asmName.StartsWith("Microsoft."))
            //        continue;

            //    var typeName = method.DeclaringType.FullName;
            //    var name = $"{typeName}::{method.Name}";
            //    var pars = method.GetParameters();
            //    var parNames = string.Empty;
            //    var lastInd = pars.Length - 1;
            //    for (var j = 0; j <= lastInd; j++)
            //    {
            //        var p = pars[j];
            //        parNames += p.ParameterType.FullName;
            //        if (j < lastInd)
            //            parNames += ",";
            //    }
            //    name = $"{method.ReturnType.FullName} {name}({parNames})";

            //    Console.WriteLine($"{name} -> {stackFrame.GetILOffset()}");
            //}

            Console.WriteLine($"{nameof(IfElse_Half)}: {type}");
        }

        internal void IfElse_FullSimple(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple)}: {type}");
        }

        internal void IfElse_Consec_Full(bool a, bool b)
        {
            var info = new bool?[2,2];
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

        internal bool IfElse_Consec_HalfA_FullB(bool a, bool b)
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

        internal bool IfElse_Half_EarlyReturn_Func(bool cond)
        {
            var type = "no"; //let it be... Let it beeee!...
            if (cond)
            {
                type = "yes";
                Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Func)}: {type}");
                return true;
            }

            Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Func)}: {type}");

            return false;
        }

        internal void Ternary_Positive(bool cond)
        {
            var type = cond ? "yes" : "no";
            Console.WriteLine($"{nameof(Ternary_Positive)}: {type}");
        }

        internal void Ternary_Negative(bool cond)
        {
            var type = !cond ? "no" : "yes";
            Console.WriteLine($"{nameof(Ternary_Negative)}: {type}");
        }

        internal void IfElse_FullCompound(bool a, bool b)
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

        internal void IfElse_HalfA_FullB(bool a, bool b)
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

        internal void IfElse_HalfA_HalfB(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_HalfA_FullB)}: ab");
                }
            }
        }

        internal void IfElse_FullA_HalfB(bool a, bool b)
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
        #region Switch
        internal void Switch_WithReturn(int a)
        {
            var s = "";
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_WithReturn)}: {a} -> return"); return;
                case 0: s = "A"; break;
                case 1: s = "B"; break;
                default: s = "default"; break;
            }

            Console.WriteLine($"{nameof(Switch_WithReturn)}: {a} -> {s}");
        }

        internal void Switch_WithoutDefault(int a)
        {
            var s = "default";
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_WithReturn)}: {a} -> return"); return;
                case 0: s = "A"; break;
                case 1: s = "B"; break;
            }

            Console.WriteLine($"{nameof(Switch_WithReturn)}: {a} -> {s}");
        }

        internal string Switch_AsReturn(int a)
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
        #endregion
        #region Linq
        internal void Linq_Query(bool all)
        {
            var customers = new List<string> { "Paris", "London", "Moscow" };
            var res = from c in customers where all || c == "London" select c;
            Console.WriteLine($"{nameof(Linq_Query)}: {string.Join(",", res)}");
        }

        internal void Linq_Fluent(bool all)
        {
            var customers = new List<string> { "Paris", "London", "Moscow" };
            var res = customers.Where(c => all ? c != null : c == "London");
            Console.WriteLine($"{nameof(Linq_Fluent)}: {string.Join(",", res)}");
        }
        #endregion
        #region Lambda
        internal void Lambda10(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            Console.WriteLine($"{nameof(Lambda10)}: {d}");
        }

        internal void Lambda10_AdditionalBranch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            if (d >= 100)
                d /= 2;
            Console.WriteLine($"{nameof(Lambda10_AdditionalBranch)}: {d}");
        }

        internal void Lambda10_AdditionalSwitch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            switch (d)
            {
                case 100: d = 50; break;
                case 144: d = 75; break;
            }
            Console.WriteLine($"{nameof(Lambda10_AdditionalSwitch)}: {d}");
        }
        #endregion
        #region Generics
        internal void GenericParameter(List<string> list, bool a)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            //
            if (a)
            {
                if (list.Count > 0)
                    list.RemoveAt(0);
            }
            else
            {
                list.Reverse();
            }

            Console.WriteLine($"{nameof(GenericParameter)}: {a} -> {string.Join(",", list)}");
        }

        internal void GenericVar(bool a)
        {
            var list = new List<string> { "a", "b", "c" };
            if (a)
            {
                if (list.Count > 0)
                    list.RemoveAt(0);
            }
            else
            {
                list.Reverse();
            }

            Console.WriteLine($"{nameof(GenericVar)}: {a} -> {string.Join(",", list)}");
        }

        internal void Generic_Call_Base(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = gen.GetDesc(cond);
            Console.WriteLine($"{nameof(Generic_Call_Base)}: {s}");
        }

        internal void Generic_Call_Child(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = cond ? gen.GetShortDesc() : "no desc";
            Console.WriteLine($"{nameof(Generic_Call_Child)}: {s}");
        }
#endregion
        #region Do/While
        internal void While_Operator(int count)
        {
            Console.WriteLine($"{nameof(While_Operator)} -> {count}");
            while (count > 0)
                count--;
        }

        //in principle, it is not necessary, because 
        //it is not a branch with a precondition
        internal void Do_Operator()
        {
            int i = 3;
            Console.WriteLine($"{nameof(Do_Operator)} -> {i}");
            do { i--; } while (i > 0);
        }
        #endregion
        #region Try/cath/finally
        internal void Exception_Conditional(bool isException)
        {
            try
            {
                Console.WriteLine(nameof(Exception_Conditional));
            }
            //exception rethrow is not crack the injection
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            //exception throw is not crack the injection
            if (isException)
            {
                throw new Exception("Throw!");
            }
        }

        internal void Catch_Statement(bool cond)
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
            Console.WriteLine($"{nameof(Catch_Statement)}: {s}");
        }

        internal void Catch_When_Statement(bool cond, bool cond2)
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
            Console.WriteLine($"{nameof(Catch_When_Statement)}: {s}");
        }

        internal void Finally_Statement(bool cond)
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
            Console.WriteLine($"{nameof(Finally_Statement)}: {s}");
        }
        #endregion
        #region Dynamic
        internal void ExpandoObject(bool cond)
        {
            dynamic exp = new ExpandoObject();
            exp.Act = (Func<bool, string>)((a) => { return a ? "yes" : "false"; });
            exp.Act(cond);
            Console.WriteLine($"{nameof(ExpandoObject)}: {cond}");
        }

        internal void DynamicObject(bool cond)
        {
            dynamic exp = new DynamicDictionary();
            exp.Act = (Func<bool, string>)((a) => { return a ? "yes" : "false"; });
            exp.Act(cond);
            Console.WriteLine($"{nameof(DynamicObject)}: {cond}");
        }
        #endregion
        #region Async/await
        internal async Task AsyncTask(bool cond)
        {
            if (cond)
                await Task.Delay(50);
            else
                await Delay100();
            Console.WriteLine($"{nameof(AsyncTask)}: {cond}");
        }

        private Task Delay100()
        {
            return Task.Delay(100);
        }

        internal async Task AsyncLambda(bool cond)
        {
            await Task.Run(async () =>
            {
                if (cond)
                    await Task.Delay(50);
                else
                    await Task.Delay(100);
                Console.WriteLine($"{nameof(AsyncLambda)}: {cond}");
            });
        }

        internal void AsyncLinq_Blocking(bool cond)
        {
            var data = GetDataForAsyncLinq();
            var inputs = data.Select(async ev => await ProcessElement(ev, cond))
                   .Select(t => t.GetAwaiter().GetResult().Prop)
                   .Where(i => i != null)
                   .ToList();
            Console.WriteLine($"{nameof(AsyncLinq_Blocking)}: {string.Join(", ", inputs)}");
        }

        internal async Task AsyncLinq_NonBlocking(bool cond)
        {
            var data = GetDataForAsyncLinq();
            var tasks = await Task.WhenAll(data.Select(ev => ProcessElement(ev, cond)));
            var inputs = tasks.Select(a => a.Prop).Where(result => result != null).ToList();
            Console.WriteLine($"{nameof(AsyncLinq_NonBlocking)}: {string.Join(", ", inputs)}");
        }

        private List<GenStr> GetDataForAsyncLinq()
        {
            return new List<GenStr> { new GenStr("A"), new GenStr("B"), new GenStr("C") };
        }

        private async Task<GenStr> ProcessElement(GenStr element, bool cond)
        {
            await Task.Delay(10);
            if (cond)
                element.Prop += "/1";
            return element;
        }
        #endregion
        #region Parallel
        internal void Plinq(bool cond)
        {
            var data = GetDataForParallel();
            int sum = data.AsParallel().Where(a => !cond || (cond && a % 2 == 0)).Sum();
            Console.WriteLine($"{nameof(Plinq)}: {sum}");
        }

        internal void ForParallel(bool cond)
        {
            var data = GetDataForParallel();
            int sum = 0;

            Parallel.For(0, data.Count(), a =>
            {
                if (!cond || (cond && a % 2 == 0))
                    Interlocked.Add(ref sum, a);
            });
            Console.WriteLine($"{nameof(ForParallel)}: {sum}");
        }

        internal void ForeachParallel(bool cond)
        {
            var data = GetDataForParallel();
            int sum = 0;
            Parallel.ForEach(data, a =>
            {
                if (!cond || (cond && a % 2 == 0))
                    Interlocked.Add(ref sum, a);
            });
            Console.WriteLine($"{nameof(ForeachParallel)}: {sum}");
        }

        private IEnumerable<int> GetDataForParallel(int cnt = 10)
        {
            return Enumerable.Range(0, cnt);
        }
        #endregion
        #region Using, finalizer
        internal void UsingStatement_SyncRead(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    ms.Read(res, 0, (int)ms.Length);
            }
            Console.WriteLine($"{nameof(UsingStatement_SyncRead)}: {cond}");
        }

        internal async Task UsingStatement_AsyncRead(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    await ms.ReadAsync(res, 0, (int)ms.Length);
            }
            Console.WriteLine($"{nameof(UsingStatement_AsyncRead)}: {cond}");
        }

        internal async Task UsingStatement_AsyncTask(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    await AsyncWait();
            }
            Console.WriteLine($"{nameof(UsingStatement_AsyncRead)}: {cond}");
        }

        private Task AsyncWait()
        {
            return Task.Run(() => { Thread.Sleep(50); });
        }

        internal void Finalizer(ushort len)
        {
            new Finalizer(len);
            Console.WriteLine($"{nameof(Finalizer)}: {len}");
        }
        #endregion
        #region WinAPI
        [DllImport("user32.dll")]
        public static extern void SetWindowText(IntPtr hwnd, String lpString);

        internal void WinAPI(bool cond)
        {
            SetWindowText(IntPtr.Zero, cond ? "Bye!" : "Hello!");
            Console.WriteLine($"{nameof(WinAPI)}: {cond}");
        }
        #endregion
        #region Misc
        private readonly object _locker = new object();
        internal void Lock_Statement(bool cond)
        {
            string s;
            lock (_locker)
            {
                s = cond ? "YES" : "NO";
            }
            Console.WriteLine($"{nameof(Lock_Statement)}: {s}");
        }

        delegate int Operation(int x, int y);
        internal void AnonymousFunc()
        {
            int z = 8;
            Operation operation = delegate (int x, int y)
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            };
            int d = operation(4, 5);
            Console.WriteLine($"{nameof(AnonymousFunc)}: {d}"); // 15
        }

        internal void AnonymousType(bool cond)
        {
            var tom = new { Name = "Tom", Age = cond ? 21 : 9 };
            Console.WriteLine($"{nameof(AnonymousType)}: {cond} -> {tom.Age}");
        }

        //TODO: not working yet!
        internal void Expression10(int x)
        {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x < 10 ? 0 : x * x;
            var dlg = e.Compile();
            int d = dlg(x);
            Console.WriteLine($"{nameof(Expression10)}: {d}");
        }

        internal void ContextBound(int prop)
        {
            new ContextBound(prop);
            Console.WriteLine($"{nameof(ContextBound)}: {prop}");
        }

        internal void Unsafe(bool cond)
        {
            Point point;
            unsafe
            {
                Point* p = &point;
                p->x = cond ? 10 : 20;
                p->y = 35;
                Console.WriteLine($"{nameof(Unsafe)}: {p->ToString()}");
            }
        }
        #endregion

        private byte[] GetBytes(byte cnt)
        {
            var arr = new byte[cnt];
            for (byte i = 0; i < cnt; i++)
                arr[i] = i;
            return arr;
        }

        //TODO: a || b, yield, catch filter, async iterator, for, foreach, EF, Visual Basic...
    }
}
