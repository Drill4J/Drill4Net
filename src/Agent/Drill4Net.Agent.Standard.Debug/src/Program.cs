using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard.Debug
{
    class Program
    {
        private static int _pointRange = 200;
        private static List<CrossPoint> _points;
        private const ConsoleColor COLOR_DEFAULT = ConsoleColor.Green;

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

                StandardAgent.Init();

                //point data (in fact, from the TestEngine's tree)
                var rep = new StandardAgentRepository();
                var tree = rep.ReadInjectedTree();
                const string moniker = "net5.0";
                var asmTree = tree.GetFrameworkRootDirectory(moniker);
                if (asmTree == null)
                    throw new Exception($"Data for moniker {moniker} not found");
                _points = asmTree.Filter(typeof(CrossPoint), true).Cast<CrossPoint>().ToList();

                ////debug
                //var treeCnv = new TreeConverter();
                //var types = tree.GetAllTypes().Where(a => a.Name == "CoverageTarget");
                //treeCnv.CreateCoverageRegistrator(new Abstract.Transfer.StartSessionPayload(), types);

                //range
                await Task.Delay(1500).ConfigureAwait(false); //wait for connect to admin side
                WriteMessage($"\nCross points: {_points.Count}", ConsoleColor.Magenta);
                WriteMessage($"Input point count for the extraction range [{_pointRange}]: ", ConsoleColor.Yellow);
                var expr = Console.ReadLine()?.Trim();
                if (int.TryParse(expr, out var pntCnt))
                {
                    if (pntCnt > 0)
                        _pointRange = pntCnt;
                }

                // info
                const string mess = @"  *** Firstly, start session on admin side...
  *** Press 1 for start some portion of target method's cross-points
  *** Press q for exit
  *** Good luck and... keep on dancing!";
                WriteMessage($"\n{mess}", ConsoleColor.Yellow);

                //polling
                while (true)
                {
                    WriteMessage("\nInput:");
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
                WriteMessage("No more points!", ConsoleColor.Red);
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
                StandardAgent.RegisterStatic($"{point.PointUid}");
            }
            WriteMessage($"Remaining points: {_points.Count}", ConsoleColor.Blue);
            return true;
        }

        private static void WriteMessage(string mess, ConsoleColor color = COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = COLOR_DEFAULT;
        }
    }
}