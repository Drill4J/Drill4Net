using System;

namespace TestA
{
    class Program
    {
        static void Main(string[] args)
        {
            FuncA();
            //
            var r = new Random(DateTime.Now.Millisecond);
            for(var i=0; i<5; i++)
                FuncB(r.Next(0, 2));
        }

        internal static void FuncA()
        {
            Console.WriteLine("FuncA");
        }

        internal static void FuncB(int b)
        {
            if (b % 2 == 0)
                Console.WriteLine("FunB: even");
            else
                Console.WriteLine("FunB: odd");
        }
    }
}
