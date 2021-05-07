using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard.Debug
{
    class Program
    {
        //private static Assembly _asm;
        //private static Type[] _types;
        //private static Dictionary<Type, MethodInfo[]> _methods;
        private static int _pointRange = 100;
        private static List<CrossPoint> _points;

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



                //point data
                var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_STD_NAME);
                var injRep = new InjectorRepository(cfg_path);
                var tree = injRep.ReadInjectedTree();
                var moniker = "net5.0";
                var asmTree = tree.GetFrameworkVersionRootDirectory(moniker);
                if (asmTree == null)
                    throw new Exception($"Data for moniker {moniker} not found");
                _points = asmTree.Filter(typeof(CrossPoint), true).Cast<CrossPoint>().ToList();
                Console.WriteLine($"\nCross points: {_points.Count}");

                //range
                Console.Write($"Input point count for the simulating execution range [{_pointRange}]: ");
                var expr = Console.ReadLine()?.Trim();
                if (int.TryParse(expr, out var pntCnt))
                {
                    if (pntCnt > 0)
                        _pointRange = pntCnt;
                }

                // info
                var mess = @"  *** Firstly, start session on admin side...
  *** Press 1 for start some portion of target methods
  *** Press q for exit
  *** Good luck and... keep on dancing!";
                Console.WriteLine($"\n{mess}");

                //loading methods
                //TODO: do norm!!!
                //var dir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected.Tests\Drill4Net.Target.Net50.App\net5.0\";
                //var path = $"{dir}Drill4Net.Target.Common.dll";
                //_asm = Assembly.LoadFrom(path);
                //_types = _asm.GetTypes().Where(a => a.IsPublic).ToArray();
                //_methods = new Dictionary<Type, MethodInfo[]>();
                //foreach (var type in _types)
                //    _methods.Add(type, type.GetMethods());

                //polling
                while (true)
                {
                    Console.WriteLine("\nInput:");
                    expr = Console.ReadLine()?.Trim();
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
            if (_points.Count == 0)
            {
                Console.WriteLine("No more points!");
                return false;
            }
            //
            var r = new Random(DateTime.Now.Millisecond);
            var end = Math.Min(_pointRange, _points.Count);
            for (var i = 0; i < end; i++)
            {
                var ind = r.Next(0, _points.Count);
                var point = _points[ind];
                _points.RemoveAt(ind);
                StandardAgent.RegisterStatic($"{point.PointUid}^asmName^funcSig^probe");
            }

            //var injType = _types.First(a => a.Name == "InjectTarget");
            //var meths = _methods[injType];
            //var meth = meths.First(a => a.Name == "RunTests");
            //var obj = Activator.CreateInstance(injType);
            //meth.Invoke(obj, null);
            return true;
        }
    }
}