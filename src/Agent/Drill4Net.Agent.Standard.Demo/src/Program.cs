using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard.Demo
{
    class Program
    {
        private static Assembly _asm;
        private static Type[] _types;
        private static Dictionary<Type, MethodInfo[]> _methods;

        /*********************************************************************************/

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

                //              // calling the methods
                //              var mess = @"  *** Press 1 for start some portion of target methods
                //*** Press q for exit
                //*** Good luck and... keep on dancing!";
                //              Console.WriteLine($"\n{mess}");

                //              //loading methods
                //              //TODO: do norm!!!
                //              var dir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected.Tests\Drill4Net.Target.Net50.App\net5.0\";
                //              var path = $"{dir}Drill4Net.Target.Common.dll";
                //              _asm = Assembly.LoadFrom(path);
                //              _types = _asm.GetTypes().Where(a => a.IsPublic).ToArray();
                //              _methods = new Dictionary<Type, MethodInfo[]>();
                //              foreach (var type in _types)
                //                  _methods.Add(type, type.GetMethods());

                //              //polling
                //              while (true)
                //              {
                //                  Console.WriteLine("\nInput:");
                //                  var expr = Console.ReadLine()?.Trim();
                //                  if (string.IsNullOrWhiteSpace(expr))
                //                      continue;
                //                  if (expr == "q" || expr == "Q")
                //                      break;
                //                  //
                //                  string output = null;
                //                  try
                //                  {
                //                      StartMethods(expr);
                //                  }
                //                  catch (Exception ex)
                //                  {
                //                      output = $"error -> {ex.Message}";
                //                  }
                //              }

                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Done.");
        }

        private static bool StartMethods(string input)
        {
            if (input != "1")
                return false;
            //var r = new Random(DateTime.Now.Millisecond);
            //var t = _types[r.Next(0, _types.Length)];
            //var methods = _methods[t];
            //var method = methods[r.Next(0, methods.Length)];

            var injType = _types.First(a => a.Name == "InjectTarget");
            var meths = _methods[injType];
            var meth = meths.First(a => a.Name == "RunTests");
            var obj = Activator.CreateInstance(injType);
            meth.Invoke(obj, null);
            return true;
        }
    }
}