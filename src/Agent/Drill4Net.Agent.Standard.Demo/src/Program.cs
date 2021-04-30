using System;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard.Demo
{
    class Program
    {
        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            try
            {
                Console.WriteLine("Initializing...");

                // emulating the init (primary) instrumented request - it won't included in session, in fact
                var pointUid = "7848799f-77ee-444d-9a9d-6fd6d90f5d82"; //must be real from Injected Tree
                var asmName = $"Drill4Net.Target.Common.dll";
                const string funcSig = "System.Void Drill4Net.Agent.Standard.StandardAgent::Register(System.String)";
                StandardAgent.RegisterStatic($"{pointUid}^{asmName}^{funcSig}^If_6");

                // emulating second request, but it will be skipped, too
                await Task.Delay(250);
                StandardAgent.RegisterStatic($"{pointUid}^{asmName}^{funcSig}^If_6");

                // calling the methods
                var mess = @"  *** Press 1 for start some portion of target methods
  *** Press q for exit
  *** Good luck... and keep on dancing!";
                Console.WriteLine($"\n{mess}");

                //polling
                while (true)
                {
                    Console.WriteLine("\nInput:");
                    var expr = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(expr))
                        continue;
                    if (expr == "q" || expr == "Q")
                        break;
                    //
                    string output = null;
                    try
                    {
                        StartMethods(expr);
                    }
                    catch (Exception ex)
                    {
                        output = $"error -> {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Done.");
        }

        private static bool StartMethods(string input)
        {

            return true;
        }
    }
}