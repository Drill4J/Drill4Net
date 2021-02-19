using System;

namespace TestNF
{
    class Program
    {
        //delegate int Operation(int x, int y);

        static void Main(string[] args)
        {
            ////anonymous func
            //int z = 8;
            //Operation operation = delegate (int x, int y)
            //{
            //    return x + y + z;
            //};
            //int d = operation(4, 5);
            //Console.WriteLine($"Anonym func: {d}"); // 17

            //Exception_Conditional
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

            Ternar_Direct(false);
            Ternar_Direct(true);

            Ternar_Negative(false);
            Ternar_Negative(true);

            //IfElse_Half_Func(false);
            //IfElse_Half_Func(true);

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

        internal static void IfElse_Half(bool cond)
        {
            string type = "no"; 
            if (cond)
                type = "yes";

            Console.WriteLine($"{nameof(IfElse_Half)}: {type}");
        }

        //internal static bool IfElse_Half_Func(bool cond)
        //{
        //    string type = "no";
        //    if (cond)
        //    {
        //        type = "yes";
        //        return true;
        //    }

        //    Console.WriteLine($"{nameof(IfElse_Half_Func)}: NO");
        //    return false;
        //}

        internal static void IfElse_FullSimple(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple)}: {type}");
        }

        internal static void Ternar_Direct(bool cond)
        {
            string type = cond ? "yes" : "no";
            Console.WriteLine($"{nameof(Ternar_Direct)}: {type}");
        }

        internal static void Ternar_Negative(bool cond)
        {
            string type = !cond ? "no" : "yes";
            Console.WriteLine($"{nameof(Ternar_Negative)}: {type}");
        }
    }
}
