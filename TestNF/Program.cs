using System;
using System.Collections.Generic;
using System.Linq;

namespace TestNF
{
    class Program
    {
        static void Main(string[] args)
        {
            //anonymous func, lambda...
            AnonymousFunc();

            Lambda10(5);
            Lambda10(10);

            Expression10(5);
            Expression10(10);

            Linq(false);
            Linq(true);

            try
            {
                Exception_Conditional(false);
                Exception_Conditional(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{ex}");
            }

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

            Switch_WithReturn(-1);
            Switch_WithReturn(0);
            Switch_WithReturn(1);
            Switch_WithReturn(2);

            Switch_AsReturn(-1);
            Switch_AsReturn(0);
            Switch_AsReturn(1);
            Switch_AsReturn(2);

            var list = new List<string> { "a", "b", "c" };
            GenericParameter(list, false);
            GenericParameter(list, true);

            GenericVar(false);
            GenericVar(true);

            While_Operator(-1);
            While_Operator(3);

            Do_Operator();

            Console.ReadKey(true);
        }

        internal static void Exception_Conditional(bool isException)
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

        delegate int Operation(int x, int y);
        internal static void AnonymousFunc()
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

        internal static void Lambda10(int x)
        {
            Func<int, int> square = x => x < 10 ? 0 : x * x;
            int d = square(x);
            Console.WriteLine($"{nameof(Lambda10)}: {d}"); // 15
        }

        //yet no working!
        internal static void Expression10(int x)
        {
            System.Linq.Expressions.Expression<Func<int, int>> e = x => x < 10 ? 0 : x * x;
            var dlg = e.Compile();
            int d = dlg(x);
            Console.WriteLine($"{nameof(Expression10)}: {d}"); // 15
        }

        internal static void Linq(bool all)
        {
            var customers = new List<string> { "Paris", "London", "Moscow" };
            var res = customers.Where(c => all ? c != null : c == "London");
            Console.WriteLine($"{nameof(Expression10)}: {string.Join(",", res)}"); 
        }

        #region IF/ELSE
        internal static void IfElse_Half(bool cond)
        {
            string type = "no"; 
            if (cond)
                type = "yes";

            Console.WriteLine($"{nameof(IfElse_Half)}: {type}");
        }

        internal static bool IfElse_Consec_Full(bool a, bool b)
        {
            if (a)
            {
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: YES1");
            }
            else
            {
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: NO1");
            }
            //
            if (b)
            {
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: YES2");
            }
            else
            {
                Console.WriteLine($"{nameof(IfElse_Consec_Full)}: NO2");
            }
            return false;
        }

        internal static bool IfElse_Consec_HalfA_FullB(bool a, bool b)
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

        internal static bool IfElse_Half_EarlyReturn_Func(bool cond)
        {
            string type = "no";
            if (cond)
            {
                type = "yes";
                Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Func)}: YES");
                return true;
            }

            Console.WriteLine($"{nameof(IfElse_Half_EarlyReturn_Func)}: NO");

            return false;
        }

        internal static void IfElse_FullSimple(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple)}: {type}");
        }

        internal static void Ternary_Positive(bool cond)
        {
            string type = cond ? "yes" : "no";
            Console.WriteLine($"{nameof(Ternary_Positive)}: {type}");
        }

        internal static void Ternary_Negative(bool cond)
        {
            string type = !cond ? "no" : "yes";
            Console.WriteLine($"{nameof(Ternary_Negative)}: {type}");
        }

        internal static void IfElse_FullCompound(bool a, bool b)
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

        internal static void IfElse_HalfA_FullB(bool a, bool b)
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

        internal static void IfElse_HalfA_HalfB(bool a, bool b)
        {
            if (a)
            {
                if (b)
                {
                    Console.WriteLine($"{nameof(IfElse_HalfA_FullB)}: ab");
                }
            }
        }

        internal static void IfElse_FullA_HalfB(bool a, bool b)
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
        internal static void Switch_WithReturn(int a)
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

        internal static string Switch_AsReturn(int a)
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
        #region Generics
        internal static void GenericParameter(List<string> list, bool a)
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

        internal static void GenericVar(bool a)
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
        #endregion
        #region Do/While
        internal static void While_Operator(int count)
        {
            Console.WriteLine($"{nameof(While_Operator)} -> {count}");
            while (count > 0)
                count--;
        }

        //in principle, it is not necessary, because 
        //it is not a branch with a precondition
        internal static void Do_Operator() 
        {
            int i = 3;
            Console.WriteLine($"{nameof(Do_Operator)} -> {i}");
            do { i--; } while (i > 0);              
        }
        #endregion

        //TODO: unsafe, LINQ (both forms), catch, async/await
    }
}
