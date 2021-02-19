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

            //FuncA
            try
            {
                FuncA(false);
                FuncA(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{ex}");
            }

            if (DateTime.Now.Second % 2 == 0)
                Console.WriteLine("!!!");

            //FuncB
            FuncB(0);
            FuncB(1);

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
            {
                Console.WriteLine("Will be throw!");
                throw new Exception("Throw!");
            }
        }

        internal static void FuncB(int b)
        {
            string type;
            if (b % 2 == 0)
                type = "even";
            else
                type = "odd";

            Console.WriteLine($"FunB: {type}");
        }
    }
}
