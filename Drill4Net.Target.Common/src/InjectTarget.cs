using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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

            IfElse_Half_EarlyReturn_Bool(false);
            IfElse_Half_EarlyReturn_Bool(true);

            IfElse_Half_EarlyReturn_Tuple(false);
            IfElse_Half_EarlyReturn_Tuple(true);
            #endregion
            #region Switch
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

            Switch_AsReturn(-1);
            Switch_AsReturn(0);
            Switch_AsReturn(1);
            Switch_AsReturn(2);
            #endregion
            #region Linq
            Linq_Query(false);
            Linq_Query(true);

            Linq_Fluent(false);
            Linq_Fluent(true);
            #endregion
            #region Lambda
            Lambda10(5);
            Lambda10(10);

            Lambda10_AdditionalBranch(10);

            Lambda10_AdditionalSwitch(5);
            Lambda10_AdditionalSwitch(10);
            Lambda10_AdditionalSwitch(12);
            #endregion
            #region Generics
            GenericVar(false);
            GenericVar(true);

            Generic_Call_Base(false);
            Generic_Call_Base(true);

            Generic_Call_Child(false);
            Generic_Call_Child(true);
            #endregion
            #region Anonymous, Expression
            AnonymousFunc();
            AnonymousFunc_WithLocalFunc();

            AnonymousType(false);
            AnonymousType(true);

            Expression10(5);
            Expression10(10);
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

            TaskContinueWhenAll(false);
            TaskContinueWhenAll(true);

            ThreadNew(false);
            ThreadNew(true);
            #endregion
            #region IDisposable
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

            Yield(false);
            Yield(true);

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

        internal bool IfElse_Half_EarlyReturn_Bool(bool cond)
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

        internal (bool, bool) IfElse_Half_EarlyReturn_Tuple(bool cond)
        {
            var type = "no"; //let it be... Let it beeee!...
            if (cond)
            {
                type = "yes";
                Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Tuple)}: {type}");
                return (true,true);
            }
            Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Tuple)}: {type}");

            return (false, false);
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
        internal void Switch_ExplicitDefault(int a)
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

        internal void Switch_ImplicitDefault(int a)
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

        internal void Switch_WithoutDefault(int a)
        {
            switch (a)
            {
                case -1: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
                case 0: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
                case 1: Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a}"); return;
            }
            Console.WriteLine($"{nameof(Switch_WithoutDefault)}: {a} -> no default");
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
            if (d > 100)
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
        internal void GenericVar(bool cond)
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
            Console.WriteLine($"{nameof(GenericVar)}: {cond} -> {string.Join(",", list)}");
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

        internal Task Delay100()
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
            var inputs = tasks
                .Select(a => a.Prop)
                .Where(result => result != null)
                .ToList();

            Console.WriteLine($"{nameof(AsyncLinq_NonBlocking)}: {string.Join(", ", inputs)}");
        }

        internal List<GenStr> GetDataForAsyncLinq()
        {
            return new List<GenStr> { new GenStr("A"), new GenStr("B"), new GenStr("C"), };
        }

        internal async Task<GenStr> ProcessElement(GenStr element, bool cond)
        {
            return await Task.Run(() =>
            {
                if (cond)
                    element.Prop += "/1";
                return element;
            });
        }
        #endregion
        #region Parallel
        internal void Plinq(bool cond)
        {
            var data = GetDataForParallel(5);
            int sum = data.AsParallel().Where(a => !cond || (cond && a % 2 == 0)).Sum();
            Console.WriteLine($"{nameof(Plinq)}: {sum}");
        }

        internal void ForParallel(bool cond)
        {
            var data = GetDataForParallel(5);
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
            var data = GetDataForParallel(5);
            int sum = 0;
            Parallel.ForEach(data, a =>
            {
                if (!cond || (cond && a % 2 == 0))
                    Interlocked.Add(ref sum, a);
            });
            Console.WriteLine($"{nameof(ForeachParallel)}: {sum}");
        }

        private IEnumerable<int> GetDataForParallel(int cnt = 5)
        {
            return Enumerable.Range(0, cnt);
        }

        internal void TaskContinueWhenAll(bool cond)
        {
            Task[] tasks = new Task[2];
            List<string> list1 = null;
            List<string> list2 = null;

            tasks[0] = Task.Factory.StartNew(() => list1 = GetStringListForTaskContinue(cond));
            tasks[1] = Task.Factory.StartNew(() => list2 = !cond ? new List<string> { "Y2" } : new List<string> { "A2, B2, C12" });

            Task.Factory.ContinueWhenAll(tasks, completedTasks => {
                Console.WriteLine($"{nameof(TaskContinueWhenAll)}: {cond} -> {string.Join(",", list1)} / {string.Join(",", list2)}");
            });
        }

        internal List<string> GetStringListForTaskContinue(bool cond)
        {
            return cond ? new List<string> { "X1" } : new List<string> { "A1, B1, C1" };
        }

        internal void ThreadNew(bool cond)
        {
            var tr = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                GetStringListForThreadNew(cond);
            });
            tr.Start();
            tr.Join();
        }

        internal void GetStringListForThreadNew(bool cond)
        {
            var list = cond ? new List<string> { "XYZ" } : new List<string> { "A, B, C" };
            Console.WriteLine($"{nameof(ThreadNew)}: {cond} -> {string.Join(",", list)}");
        }
        #endregion
        #region IDisposable
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

        internal bool Finalizer(int len)
        {
            new Finalizer(len);
            Console.WriteLine($"{nameof(Finalizer)}: {len}");
            return true;
        }
        #endregion
        #region Misc
        #region AnonymousFunc
        delegate int Operation(int x, int y);
        internal void AnonymousFunc()
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
            Console.WriteLine($"{nameof(AnonymousFunc)}: {d}"); // 15
        }

        internal void AnonymousFunc_WithLocalFunc()
        {
            int z = 8;
            int operation(int x, int y)
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            }
            int d = operation(4, 5);
            Console.WriteLine($"{nameof(AnonymousFunc_WithLocalFunc)}: {d}"); // 15
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

        internal bool While_Operator(int count)
        {
            Console.WriteLine($"{nameof(While_Operator)} -> {count}");
            while (count > 0)
                count--;
            return true;
        }

        //in principle, it is not necessary, because 
        //it is not a branch with a precondition
        internal void Do_Operator()
        {
            int i = 3;
            Console.WriteLine($"{nameof(Do_Operator)} -> {i}");
            do { i--; } while (i > 0);
        }

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

        internal void AnonymousType(bool cond)
        {
            var tom = new { Name = "Tom", Age = cond ? 21 : 9 };
            Console.WriteLine($"{nameof(AnonymousType)}: {cond} -> {tom.Age}");
        }

        internal void Yield(bool cond)
        {
            var list = GetForYield(cond);
            Console.WriteLine($"{nameof(Yield)}: {cond} -> {string.Join(",", list)}");
        }

        internal IEnumerable<string> GetForYield(bool cond)
        {
            var list = new List<string> { "Y1, Y2, Y3" };
            foreach (var a in list)
                yield return cond ? a : "z";
        }

        //TODO: not working yet!
        internal void Expression10(int x)
        {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x < 10 ? 0 : x * x;
            var dlg = e.Compile();
            int d = dlg(x);
            Console.WriteLine($"{nameof(Expression10)}: {d}");
        }

        //only for NetFramework?
        internal bool ContextBound(int prop)
        {
            new ContextBound(prop);
            Console.WriteLine($"{nameof(ContextBound)}: {prop}");
            return true;
        }

        internal bool Unsafe(bool cond)
        {
            Point point;
            unsafe
            {
                Point* p = &point;
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

        //TODO: a || b, local funcs, extensions, async iterator, for, foreach, EF, Visual Basic...
    }
}
