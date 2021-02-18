using System;

namespace TestNF
{
    class Program
    {
        delegate int Operation(int x, int y);

        static void Main(string[] args)
        {
            //anonymous func is not crack the injection
            int z = 8;
            Operation operation = delegate (int x, int y)
            {
                return x + y + z;
            };
            int d = operation(4, 5);
            Console.WriteLine($"Anonym func: {d}"); // 17

            //FuncA
            try
            {
                FuncA(false);
                FuncA(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //FuncB
            var r = new Random(DateTime.Now.Millisecond);
                for (var i = 0; i < 5; i++)
                    FuncB(r.Next(0, 2));

            Console.ReadKey(true);
        }

        internal static void FuncA(bool isException)
        {
            try
            {
                Console.WriteLine("FuncA1");
            }
            //exception rethrow is not crack the injection
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            Console.WriteLine("FuncA2");

            //exception throw is not crack the injection
            if (isException)
                throw new Exception("Throw!");

            Console.WriteLine("FunA: last instruction");
        }

        internal static void FuncB(int b)
        {
            try
            {
                if (b % 2 == 0)
                    Console.WriteLine("FunB: even");
                else
                    Console.WriteLine("FunB: odd");
            }
            //exception rethrow is not crack the injection
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            Console.WriteLine("FunB: last instruction"); //for guaranteed injection for exit from the method (if this event is needed)
        }
    }
}
