using System;

namespace Drill4Net.Agent.File.Debug
{
    //all dll including from NuGet must be replaced in plugin (agent) directory
    //you need add this in csproj file of target project: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    class Program
    {
        static void Main(string[] args)
        {
            //var perf = new Performancer();
            //var cnt = 10*1000*1000;

            //perf.Calc(cnt);
            //Console.WriteLine();
            //perf.Calc(cnt);
            //Console.WriteLine();
            //perf.Calc(cnt);

            LoggerAgent.RegisterStatic($"{Guid.NewGuid()}^aaa^bbb^ccc^If_5");

            Console.WriteLine("\nDone.");
            Console.ReadKey(true);
        }
    }
}
