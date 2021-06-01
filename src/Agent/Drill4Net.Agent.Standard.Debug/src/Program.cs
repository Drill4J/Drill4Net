using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard.Debug
{
    class Program
    {
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

                //// emulating the init (primary) instrumented request - in fact, it won't included in session
                //var pointUid = "7848799f-77ee-444d-9a9d-6fd6d90f5d82"; //must be real from Injected Tree
                //var asmName = $"Drill4Net.Target.Common.dll";
                //const string funcSig = "System.Void Drill4Net.Agent.Standard.StandardAgent::Register(System.String)";
                //StandardAgent.RegisterStatic($"{pointUid}^{asmName}^{funcSig}^If_6");

                //// emulating second request, but it will be skipped, too
                //await Task.Delay(250);
                //StandardAgent.RegisterStatic($"{pointUid}^{asmName}^{funcSig}^If_6");

                StandardAgent.Init();

                //point data
                var injRep = new StandardAgentRepository(); 
                var tree = injRep.ReadInjectedTree();
                var moniker = "net5.0";
                var asmTree = tree.GetFrameworkVersionRootDirectory(moniker);
                if (asmTree == null)
                    throw new Exception($"Data for moniker {moniker} not found");
                _points = asmTree.Filter(typeof(CrossPoint), true).Cast<CrossPoint>().ToList();
                Console.WriteLine($"\nCross points: {_points.Count}");

                //debug
                var treeCnv = new TreeConverter();
                var types = tree.GetAllTypes().Where(a => a.Name == "CoverageTarget");
                treeCnv.CreateCoverageDispatcher(new Abstract.Transfer.StartSessionPayload(), types);

                //range
                await Task.Delay(1500);
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n{mess}");
                Console.ForegroundColor = ConsoleColor.Green;

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