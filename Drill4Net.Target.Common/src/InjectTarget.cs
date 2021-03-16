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
            Anonymous_Func_WithLocalFunc();

            Anonymous_Type(false);
            Anonymous_Type(true);

            Expression(5);
            Expression(10);
            #endregion
            #region Try/cath/finally
            try
            {
                Try_Exception_Conditional(false);
                Try_Exception_Conditional(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{ex}");
            }

            Try_Catch(false);
            Try_Catch(true);

            Try_CatchWhen(false, false);
            Try_CatchWhen(false, true);
            Try_CatchWhen(true, false);
            Try_CatchWhen(true, true);

            Try_Finally(false);
            Try_Finally(true);
            #endregion
            #region Async
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
            Parallel_Plinq(false);
            Parallel_Plinq(true);

            Parallel_For(false);
            Parallel_For(true);

            Parallel_Foreach(false);
            Parallel_Foreach(true);

            Parallel_Task_New(false);
            Parallel_Task_New(true);

            Parallel_ThreadNew(false);
            Parallel_ThreadNew(true);
            #endregion
            #region IDisposable
            Disposable_Using_SyncRead(false);
            Disposable_Using_SyncRead(true);

            await Disposable_Using_AsyncRead(false);
            await Disposable_Using_AsyncRead(true);

            await Disposable_Using_AsyncTask(false);
            await Disposable_Using_AsyncTask(true);

            Disposable_Finalizer(17);
            Disposable_Finalizer(18);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            #endregion
            #region Cycle
            Cycle_While(-1);
            Cycle_While(3);

            Cycle_Do_Operator();
            #endregion
            #region Misc
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

        internal void IfElse_Ternary_Positive(bool cond)
        {
            var type = cond ? "yes" : "no";
            Console.WriteLine($"{nameof(IfElse_Ternary_Positive)}: {type}");
        }

        internal void IfElse_Ternary_Negative(bool cond)
        {
            var type = !cond ? "no" : "yes";
            Console.WriteLine($"{nameof(IfElse_Ternary_Negative)}: {type}");
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
        #region Cycle
        internal bool Cycle_While(int count)
        {
            Console.WriteLine($"{nameof(Cycle_While)} -> {count}");
            while (count > 0)
                count--;
            return true;
        }

        //in principle, it is not necessary, because 
        //it is not a branch with a precondition
        internal void Cycle_Do_Operator()
        {
            int i = 3;
            Console.WriteLine($"{nameof(Cycle_Do_Operator)} -> {i}");
            do { i--; } while (i > 0);
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
        internal void Lambda(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            Console.WriteLine($"{nameof(Lambda)}: {d}");
        }

        internal void Lambda_AdditionalBranch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            if (d > 100)
                d /= 2;
            Console.WriteLine($"{nameof(Lambda_AdditionalBranch)}: {d}");
        }

        internal void Lambda_AdditionalSwitch(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            switch (d)
            {
                case 100: d = 50; break;
                case 144: d = 75; break;
            }
            Console.WriteLine($"{nameof(Lambda_AdditionalSwitch)}: {d}");
        }
        #endregion
        #region Generics
        internal void Generics_Var(bool cond)
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

        internal void Generics_Call_Base(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = gen.GetDesc(cond);
            Console.WriteLine($"{nameof(Generics_Call_Base)}: {s}");
        }

        internal void Generics_Call_Child(bool cond)
        {
            var gen = new GenStr("AAA");
            var s = cond ? gen.GetShortDesc() : "no desc";
            Console.WriteLine($"{nameof(Generics_Call_Child)}: {s}");
        }
        #endregion
        #region Try/cath/finally
        internal void Try_Exception_Conditional(bool isException)
        {
            try
            {
                Console.WriteLine(nameof(Try_Exception_Conditional));
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

        internal void Try_Catch(bool cond)
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

        internal void Try_CatchWhen(bool cond, bool cond2)
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

        internal void Try_Finally(bool cond)
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
        internal async Task Async_Task(bool cond)
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

        internal async Task Async_Lambda(bool cond)
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

        internal void Async_Linq_Blocking(bool cond)
        {
            var data = GetDataForAsyncLinq();
            var inputs = data.Select(async ev => await ProcessElement(ev, cond))
                   .Select(t => t.GetAwaiter().GetResult().Prop)
                   .Where(i => i != null)
                   .ToList();

            Console.WriteLine($"{nameof(Async_Linq_Blocking)}: {string.Join(", ", inputs)}");
        }

        internal async Task Async_Linq_NonBlocking(bool cond)
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
                if (cond)
                    element.Prop += "/1";
                return element;
            });
        }
        #endregion
        #region Parallel
        internal void Parallel_Plinq(bool cond)
        {
            var data = GetDataForParallel(5);
            int sum = data.AsParallel().Where(a => !cond || (cond && a % 2 == 0)).Sum();
            Console.WriteLine($"{nameof(Parallel_Plinq)}: {sum}");
        }

        internal void Parallel_For(bool cond)
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

        internal void Parallel_Foreach(bool cond)
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

        internal void Parallel_Task_New(bool cond)
        {
            Task[] tasks = new Task[2];
            List<string> list1 = null;
            List<string> list2 = null;

            tasks[0] = Task.Factory.StartNew(() => list1 = GetStringListForTaskNew(cond));
            tasks[1] = Task.Factory.StartNew(() => list2 = !cond ? new List<string> { "Y2" } : new List<string> { "A2, B2, C2" });

            Task.WaitAll(tasks);
            Console.WriteLine($"{nameof(Parallel_Task_New)}: {cond} -> {string.Join(",", list1)} / {string.Join(",", list2)}");
        }

        internal List<string> GetStringListForTaskNew(bool cond)
        {
            return cond ? new List<string> { "X1" } : new List<string> { "A1, B1, C1" };
        }

        internal void Parallel_ThreadNew(bool cond)
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
            Console.WriteLine($"{nameof(Parallel_ThreadNew)}: {cond} -> {string.Join(",", list)}");
        }
        #endregion
        #region IDisposable
        internal void Disposable_Using_SyncRead(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    ms.Read(res, 0, (int)ms.Length);
            }
            Console.WriteLine($"{nameof(Disposable_Using_SyncRead)}: {cond}");
        }

        internal async Task Disposable_Using_AsyncRead(bool cond)
        {
            byte cnt = 10;
            var res = new byte[cnt];
            using (var ms = new MemoryStream(GetBytes(cnt)))
            {
                if (cond)
                    await ms.ReadAsync(res, 0, (int)ms.Length);
            }
            Console.WriteLine($"{nameof(Disposable_Using_AsyncRead)}: {cond}");
        }

        internal async Task Disposable_Using_AsyncTask(bool cond)
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
            return Task.Run(() => { Thread.Sleep(50); });
        }

        internal bool Disposable_Finalizer(int len)
        {
            new Finalizer(len);
            Console.WriteLine($"{nameof(Disposable_Finalizer)}: {len}");
            return true;
        }
        #endregion
        #region Anonymous
        #region AnonymousFunc
        delegate int Operation(int x, int y);
        internal void Anonymous_Func()
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

        internal void Anonymous_Func_WithLocalFunc()
        {
            int z = 8;
            int operation(int x, int y)
            {
                if (x > 1)
                    x /= 2;
                return x + y + z;
            }
            int d = operation(4, 5);
            Console.WriteLine($"{nameof(Anonymous_Func_WithLocalFunc)}: {d}"); // 15
        }
        #endregion

        internal void Anonymous_Type(bool cond)
        {
            var tom = new { Name = "Tom", Age = cond ? 21 : 9 };
            Console.WriteLine($"{nameof(Anonymous_Type)}: {cond} -> {tom.Age}");
        }
        #endregion
        #region Misc
        #region WinAPI
        [DllImport("user32.dll")]
        public static extern void SetWindowText(IntPtr hwnd, String lpString);

        internal void WinAPI(bool cond)
        {
            SetWindowText(IntPtr.Zero, cond ? "Bye!" : "Hello!");
            Console.WriteLine($"{nameof(WinAPI)}: {cond}");
        }
        #endregion

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
        internal void Expression(int x)
        {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x < 10 ? 0 : x * x;
            var dlg = e.Compile();
            int d = dlg(x);
            Console.WriteLine($"{nameof(Expression)}: {d}");
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

        //TODO: a || b, local funcs, extensions, own enumerator, async iterator, for, foreach, EF, Visual Basic...
    }
}
